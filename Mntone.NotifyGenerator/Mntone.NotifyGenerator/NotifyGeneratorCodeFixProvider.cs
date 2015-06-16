using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mntone.NotifyGenerator.Helpers;
using System.Text;
using Microsoft.CodeAnalysis.Formatting;

namespace Mntone.NotifyGenerator
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NotifyGeneratorCodeFixProvider))]
	[Shared]
	public sealed class NotifyGeneratorCodeFixProvider : CodeFixProvider
	{
		public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(NotifyGeneratorDiagnosticAnalyzer.DiagnosticId);
		public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext ctx)
		{
			var root = (CompilationUnitSyntax)await ctx.Document.GetSyntaxRootAsync(ctx.CancellationToken).ConfigureAwait(false);

			var diagnostic = ctx.Diagnostics.First();
			var diagnosticSpan = diagnostic.Location.SourceSpan;
			var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<ClassDeclarationSyntax>().First();

			ctx.RegisterCodeFix(
				CodeAction.Create(Resources.AnalyzerInvokeMessage, c => MakeNotificationAsync(ctx.Document, root, declaration, c)),
				diagnostic);
		}

		#region MyRegion

		public int x;
		public int y;

		#endregion


		private async Task<Document> MakeNotificationAsync(Document document, CompilationUnitSyntax root, ClassDeclarationSyntax @class, CancellationToken cancellationToken)
		{
			var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
			var originalClass = @class;

			AttributeSyntax notifyAttributeSyntaxWithClassDeclaration;
			if (!@class.HasSelfAttribute(out notifyAttributeSyntaxWithClassDeclaration))
			{
				return document;
			}

			// delete region
			var selfRegions = @class.FindRegionDirectiveTrivias(NotifyGeneratorValidation.DefaultRegionName);
			var deleteMembers = selfRegions.SelectMany(r =>
			{
				var start = r.SpanStart;
				var er = r.GetNextDirective();
				var end = er.SpanStart;
				return @class.Members.Where(x => start <= x.SpanStart && x.SpanStart <= end);
			});

			// get format
			var classHasNotify = notifyAttributeSyntaxWithClassDeclaration != null;
			var useThis = notifyAttributeSyntaxWithClassDeclaration.GetUseThisOrDefault();
			var usePrivateAccessModifierClearly = notifyAttributeSyntaxWithClassDeclaration.GetUsePrivateAccessModifierClearlyOrDefault();
			var threadSafe = notifyAttributeSyntaxWithClassDeclaration.GetIsThreadSafeOrDefault();
			var isFieldHidden = notifyAttributeSyntaxWithClassDeclaration.GetIsFieldHiddenOrDefault();
			var compareMethod = notifyAttributeSyntaxWithClassDeclaration.GetCompareMethodOrDefault();
			var nameConversion = notifyAttributeSyntaxWithClassDeclaration.GetNameConversionOrDefault();

			// analysis tree
			var newMemberList = new List<MemberDeclarationSyntax>();
			var propertyInfo = new List<Tuple<string, string, string>>();
			foreach (var member in @class.Members)
			{
				if (deleteMembers.Contains(member)) continue;

				var property = member as PropertyDeclarationSyntax;
				if (property == null)
				{
					newMemberList.Add(member);
					continue;
				}

				bool isTarget;
				property.NeedImplemantation(classHasNotify, nameConversion, useThis, out isTarget);
				if (!isTarget)
				{
					newMemberList.Add(member);
					continue;
				}

				var accessLevel = property.Modifiers.First();
				var type = property.Type.ToString();
				var propertyName = property.Identifier.Text;
				var fieldName = nameConversion.ConversionFunction(propertyName);

				// build property text
				var builder = new StringBuilder();
				builder.AppendLine($"{accessLevel} {type} {propertyName}");
				builder.AppendLine("{");
				builder.AppendFormat(useThis ? NotifyGeneratorFormats.GetPropertyTextWithThisFormat : NotifyGeneratorFormats.GetPropertyTextWithoutThisFormat, fieldName);
				builder.AppendFormat(useThis ? NotifyGeneratorFormats.SetPropertyTextWithThisFormat : NotifyGeneratorFormats.SetPropertyTextWithoutThisFormat, fieldName);
				builder.AppendLine("}");

				var properyTree = CSharpSyntaxTree.ParseText(builder.ToString());
				var newProperty = properyTree.GetRoot().ChildNodes().OfType<PropertyDeclarationSyntax>().First();

				// add
				newMemberList.Add(newProperty);
				propertyInfo.Add(Tuple.Create(type, propertyName, fieldName));
			}

			// build region
			var regionBuilder = new StringBuilder();
			regionBuilder.AppendLine();

			regionBuilder.AppendFormat(NotifyGeneratorFormats.BeginRegionTextFormat, NotifyGeneratorValidation.DefaultRegionName);

			var fieldTextFormat = usePrivateAccessModifierClearly ? NotifyGeneratorFormats.FieldTextFormatWithPrivate : NotifyGeneratorFormats.FieldTextFormatWithoutPrivate;
			foreach (var property in propertyInfo)
			{
				if (isFieldHidden)
				{
					regionBuilder.AppendLine(NotifyGeneratorFormats.FieldHiddenText);
				}
				regionBuilder.AppendFormat(fieldTextFormat, property.Item1, property.Item3);
			}
			regionBuilder.AppendLine();

			var eventArgsTextFormat = usePrivateAccessModifierClearly
				? NotifyGeneratorFormats.EventArgsTextFormatWithPrivate
				: NotifyGeneratorFormats.EventArgsTextFormatWithoutPrivate;
			foreach (var property in propertyInfo)
			{
				if (isFieldHidden)
				{
					regionBuilder.AppendLine(NotifyGeneratorFormats.FieldHiddenText);
				}
				regionBuilder.AppendFormat(eventArgsTextFormat, property.Item3, property.Item2);
			}
			regionBuilder.AppendLine();

			regionBuilder.AppendLine(NotifyGeneratorFormats.EventFieldText);
			regionBuilder.AppendLine();

			regionBuilder.Append(usePrivateAccessModifierClearly
				? NotifyGeneratorFormats.SetValueFunctionTextWithPrivate
				: NotifyGeneratorFormats.SetValueFunctionTextWithoutPrivate);
			regionBuilder.AppendLine("{");
			if (compareMethod != CompareMethod.None)
			{
				regionBuilder.AppendFormat(
					NotifyGeneratorFormats.CompareFormat,
					compareMethod == CompareMethod.Equals
						? NotifyGeneratorFormats.CompareEqualsText
						: compareMethod == CompareMethod.ReferenceEquals
							? NotifyGeneratorFormats.CompareReferenceEqualsText
							: NotifyGeneratorFormats.CompareEqualityComparerEqualsText);
				regionBuilder.AppendLine("{");
				regionBuilder.AppendLine(NotifyGeneratorFormats.ReturnFalseText);
				regionBuilder.AppendLine("}");
			}
			regionBuilder.AppendLine(NotifyGeneratorFormats.AssignText);
			regionBuilder.AppendLine(threadSafe
				? (useThis ? NotifyGeneratorFormats.ThreadSafeInvokeWithThisText : NotifyGeneratorFormats.ThreadSafeInvokeWithoutThisText)
				: (useThis ? NotifyGeneratorFormats.NonThreadSafeInvokeWithThisText : NotifyGeneratorFormats.NonThreadSafeInvokeWithoutThisText));
			regionBuilder.AppendLine(NotifyGeneratorFormats.ReturnTrueText);
			regionBuilder.AppendLine("}");
			regionBuilder.AppendLine();
			regionBuilder.Append(NotifyGeneratorFormats.EndRegionText);

			// apply
			var regionTree = CSharpSyntaxTree.ParseText(regionBuilder.ToString()).GetRoot();
			var newCloseBraceToken = @class.CloseBraceToken.WithLeadingTrivia(
				SyntaxFactory.CarriageReturnLineFeed,
				regionTree.DescendantTrivia().First(x => x.IsKind(SyntaxKind.EndRegionDirectiveTrivia)));
			@class = @class
				.WithMembers(SyntaxFactory.List(newMemberList))
				.AddMembers(regionTree.ChildNodes().OfType<MemberDeclarationSyntax>().ToArray())
				.WithCloseBraceToken(newCloseBraceToken)
				.WithINotifyPropertyChangedInterface();
			var newRoot = root.ReplaceNode(originalClass, @class)
				.WithUsing("System.ComponentModel")
				.WithAdditionalAnnotations(Formatter.Annotation);
			newRoot = newRoot.WithUsings(SyntaxFactory.List(newRoot.Usings.OrderBy(u => u.Name.ToString())));
			var newDocument = document.WithSyntaxRoot(newRoot);
			return newDocument;
		}
	}
}