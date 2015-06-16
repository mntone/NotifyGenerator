using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Mntone.NotifyGenerator.Helpers;

namespace Mntone.NotifyGenerator
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed class NotifyGeneratorDiagnosticAnalyzer : DiagnosticAnalyzer
	{
		public const string DiagnosticId = "NotifyGenerator";

		internal static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
		internal static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessage), Resources.ResourceManager, typeof(Resources));
		internal static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
		internal const string Category = "Generating";

		internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
			DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Info, isEnabledByDefault: true, description: Description);

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

		public override void Initialize(AnalysisContext ctx) => ctx.RegisterSyntaxNodeAction(AnalyzeNodeSyntax, SyntaxKind.ClassDeclaration);

		private static void AnalyzeNodeSyntax(SyntaxNodeAnalysisContext ctx)
		{
			var semanticModel = ctx.SemanticModel;
			var @class = (ClassDeclarationSyntax)ctx.Node;

			AttributeSyntax notifyAttributeSyntaxWithClassDeclaration;
			if (!@class.HasSelfAttribute(out notifyAttributeSyntaxWithClassDeclaration))
			{
				return;
			}

			if (!@class.HasINotifyProertyChanged())
			{
				ReportDiagnostic(ctx);
				return;
			}

			var classHasNotify = notifyAttributeSyntaxWithClassDeclaration != null;
			var useThis = notifyAttributeSyntaxWithClassDeclaration.GetUseThisOrDefault();
			var nameConversion = notifyAttributeSyntaxWithClassDeclaration.GetNameConversionOrDefault();
			foreach (var property in @class.Members.OfType<PropertyDeclarationSyntax>())
			{
				bool ignored;
				if (property.NeedImplemantation(classHasNotify, nameConversion, useThis, out ignored))
				{
					ReportDiagnostic(ctx);
					return;
				}
			}
		}

		private static void ReportDiagnostic(SyntaxNodeAnalysisContext ctx)
		{
			var diagnotic = Diagnostic.Create(Rule, ctx.Node.GetLocation());
			ctx.ReportDiagnostic(diagnotic);
		}
	}
}
