using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace Mntone.NotifyGenerator.Helpers
{
	internal static class CodeAnalysisExtensions
	{
		public static IEnumerable<BaseTypeSyntax> GetBaseTypes(this ClassDeclarationSyntax source)
			=> source.BaseList?.Types ?? Enumerable.Empty<BaseTypeSyntax>();

		public static IEnumerable<RegionDirectiveTriviaSyntax> GetRegionDirectiveTrivias(this ClassDeclarationSyntax source)
			=> source.DescendantTrivia().Where(x => x.IsKind(SyntaxKind.RegionDirectiveTrivia)).Select(x => x.GetStructure()).OfType<RegionDirectiveTriviaSyntax>();

		public static IEnumerable<RegionDirectiveTriviaSyntax> FindRegionDirectiveTrivias(this ClassDeclarationSyntax source, string regionName)
			=> source.GetRegionDirectiveTrivias().Where(x => x.ToString().Substring(8) == regionName);

		public static IEnumerable<FieldDeclarationSyntax> GetFields(this ClassDeclarationSyntax source)
			=> source.Members.OfType<FieldDeclarationSyntax>();

		public static IEnumerable<MethodDeclarationSyntax> GetMethods(this ClassDeclarationSyntax source)
			=> source.Members.OfType<MethodDeclarationSyntax>();

		public static IEnumerable<PropertyDeclarationSyntax> GetProperties(this ClassDeclarationSyntax source)
			=> source.Members.OfType<PropertyDeclarationSyntax>();

		public static IEnumerable<PropertyDeclarationSyntax> FindProperties(this ClassDeclarationSyntax source, string propertyName)
			=> source.GetProperties().Where(p => p.ToString() == propertyName);

		public static AccessorDeclarationSyntax GetGetAccessorOrNull(this PropertyDeclarationSyntax property)
			=> property.AccessorList.Accessors.SingleOrDefault(a => a.IsKind(SyntaxKind.GetAccessorDeclaration));

		public static AccessorDeclarationSyntax GetSetAccessorOrNull(this PropertyDeclarationSyntax property)
			=> property.AccessorList.Accessors.SingleOrDefault(a => a.IsKind(SyntaxKind.SetAccessorDeclaration));

		public static bool CheckAutoGenerateAccessor(this AccessorDeclarationSyntax accessor)
			=> accessor.SemicolonToken.Text == ";";

		public static bool CheckAutoGenerateGetOnlyProperty(this PropertyDeclarationSyntax property, string propertyName)
		{
			if (property.Identifier.Text != propertyName) return false;

			var getAccessor = property.GetGetAccessorOrNull();
			if (getAccessor == null || getAccessor.CheckAutoGenerateAccessor()) return false;

			var setAccessor = property.GetSetAccessorOrNull();
			if (setAccessor != null) return false;
			return true;
		}

		public static bool CheckAutoGenerateGetSetProperty(this PropertyDeclarationSyntax property, string propertyName)
		{
			if (property.Identifier.Text != propertyName) return false;

			var getAccessor = property.GetGetAccessorOrNull();
			if (getAccessor == null || getAccessor.CheckAutoGenerateAccessor()) return false;

			var setAccessor = property.GetSetAccessorOrNull();
			if (setAccessor == null || setAccessor.CheckAutoGenerateAccessor()) return false;
			return true;
		}

		public static bool CheckSimpleGetOnlyProperty(this PropertyDeclarationSyntax property, string propertyName, string fieldName, bool? isThisKeywordNecessary = null)
		{
			if (property.Identifier.Text != propertyName) return false;

			var getAccessor = property.GetGetAccessorOrNull();
			if (getAccessor == null || getAccessor.CheckSimpleGetAccessorInternal(fieldName, isThisKeywordNecessary)) return false;

			var setAccessor = property.GetSetAccessorOrNull();
			if (setAccessor != null) return false;
			return true;
		}

		public static bool CheckSimpleGetSetProperty(this PropertyDeclarationSyntax property, string propertyName, string fieldName, bool? isThisKeywordNecessary = null)
		{
			if (property.Identifier.Text != propertyName) return false;

			var getAccessor = property.GetGetAccessorOrNull();
			if (getAccessor == null || getAccessor.CheckSimpleGetAccessorInternal(fieldName, isThisKeywordNecessary)) return false;

			var setAccessor = property.GetSetAccessorOrNull();
			if (setAccessor == null || setAccessor.CheckSimpleSetAccessorInternal(fieldName, isThisKeywordNecessary)) return false;
			return true;
		}

		public static bool CheckSimpleGetAccessor(this AccessorDeclarationSyntax accessor, string fieldName, bool? isThisKeywordNecessary = null)
		{
			if (!accessor.IsKind(SyntaxKind.GetAccessorDeclaration)) return false;

			return accessor.CheckSimpleGetAccessorInternal(fieldName, isThisKeywordNecessary);
		}

		internal static bool CheckSimpleGetAccessorInternal(this AccessorDeclarationSyntax accessor, string fieldName, bool? isThisKeywordNecessary)
		{
			var returnStatement = accessor.Body?.Statements.FirstOrDefault() as ReturnStatementSyntax;
			if (returnStatement == null) return false;

			if (!isThisKeywordNecessary.HasValue)
			{
				return returnStatement.Expression.CheckIdentifierName(fieldName);
			}
			else if (isThisKeywordNecessary.Value)
			{
				return returnStatement.Expression.CheckIdentifierNameWithThis(fieldName);
			}

			return returnStatement.Expression.CheckIdentifierNameWithoutThis(fieldName);
		}

		public static bool CheckSimpleSetAccessor(this AccessorDeclarationSyntax accessor, string fieldName, bool? isThisKeywordNecessary = null)
		{
			if (!accessor.IsKind(SyntaxKind.SetAccessorDeclaration)) return false;

			return accessor.CheckSimpleSetAccessorInternal(fieldName, isThisKeywordNecessary);
		}

		internal static bool CheckSimpleSetAccessorInternal(this AccessorDeclarationSyntax accessor, string fieldName, bool? isThisKeywordNecessary)
		{
			var expressionStatement = accessor.Body?.Statements.FirstOrDefault() as ExpressionStatementSyntax;
			if (expressionStatement == null) return false;

			var assignmentExpression = expressionStatement.Expression as AssignmentExpressionSyntax;

			// Left
			if (!isThisKeywordNecessary.HasValue)
			{
				if (!assignmentExpression.Left.CheckIdentifierName(fieldName)) return false;
			}
			else if (isThisKeywordNecessary.Value)
			{
				if (!assignmentExpression.Left.CheckIdentifierNameWithThis(fieldName)) return false;
			}
			else
			{
				if (!assignmentExpression.Left.CheckIdentifierNameWithoutThis(fieldName)) return false;
			}

			// Right
			return assignmentExpression.Left.IsValueIdentifierName();
		}

		public static IEnumerable<EventDeclarationSyntax> GetEvents(this ClassDeclarationSyntax source)
			=> source.Members.OfType<EventDeclarationSyntax>();

		public static IEnumerable<EventFieldDeclarationSyntax> GetEventFields(this ClassDeclarationSyntax source)
			=> source.Members.OfType<EventFieldDeclarationSyntax>();

		public static IEnumerable<AttributeSyntax> GetAttributes(this ClassDeclarationSyntax source)
			=> source.AttributeLists.SelectMany(a => a.Attributes);

		public static IEnumerable<AttributeSyntax> GetAttributes(this FieldDeclarationSyntax source)
			=> source.AttributeLists.SelectMany(a => a.Attributes);

		public static IEnumerable<AttributeSyntax> GetAttributes(this PropertyDeclarationSyntax source)
			=> source.AttributeLists.SelectMany(a => a.Attributes);

		public static IEnumerable<AttributeSyntax> GetAttributes(this EventDeclarationSyntax source)
			=> source.AttributeLists.SelectMany(a => a.Attributes);

		public static IEnumerable<AttributeSyntax> GetAttributes(this EventFieldDeclarationSyntax source)
			=> source.AttributeLists.SelectMany(a => a.Attributes);

		public static IEnumerable<AttributeSyntax> FindAttributes(this IEnumerable<AttributeSyntax> source, string attributeName)
			=> source.Where(x => x.Name.ToString() == attributeName);

		public static IEnumerable<AttributeSyntax> FindAttributes(this ClassDeclarationSyntax source, string attributeName)
			=> source.GetAttributes().FindAttributes(attributeName);

		public static IEnumerable<AttributeSyntax> FindAttributes(this FieldDeclarationSyntax source, string attributeName)
			=> source.GetAttributes().FindAttributes(attributeName);

		public static IEnumerable<AttributeSyntax> FindAttributes(this PropertyDeclarationSyntax source, string attributeName)
			=> source.GetAttributes().FindAttributes(attributeName);

		public static IEnumerable<AttributeSyntax> FindAttributes(this EventDeclarationSyntax source, string attributeName)
			=> source.GetAttributes().FindAttributes(attributeName);

		public static IEnumerable<AttributeSyntax> FindAttributes(this EventFieldDeclarationSyntax source, string attributeName)
			=> source.GetAttributes().FindAttributes(attributeName);

		public static IEnumerable<AttributeArgumentSyntax> GetAttributeArguments(this AttributeSyntax source)
			=> source.ArgumentList?.Arguments ?? Enumerable.Empty<AttributeArgumentSyntax>();

		public static AttributeArgumentSyntax FindAttributeArgumentWithPropertyName(this AttributeSyntax source, string propertyName)
			=> source.GetAttributeArguments().Where(a => a.NameEquals == null ? false : a.NameEquals.Name.ToString() == propertyName).SingleOrDefault();

		public static ExpressionSyntax GetPropertyValue(this AttributeSyntax attribute, string propertyName)
		{
			var attributeArgumentSyntax = attribute.FindAttributeArgumentWithPropertyName(propertyName);
			if (attributeArgumentSyntax == null)
			{
				return null;
			}

			return attributeArgumentSyntax.Expression;
		}

		public static bool? GetBooleanPropertyValue(this AttributeSyntax attribute, string propertyName)
		{
			var attributeArgumentSyntax = attribute.FindAttributeArgumentWithPropertyName(propertyName);
			if (attributeArgumentSyntax == null)
			{
				return null;
			}

			var expressionSyntax = attributeArgumentSyntax.Expression;
			if (expressionSyntax == null)
			{
				return null;
			}
			if (expressionSyntax.IsKind(SyntaxKind.TrueLiteralExpression))
			{
				return true;
			}
			if (expressionSyntax.IsKind(SyntaxKind.FalseLiteralExpression))
			{
				return false;
			}
			return null;
		}

		public static bool HasRefKeyword(this ArgumentSyntax argument)
			=> argument.RefOrOutKeyword.Text == "ref";

		public static bool HasOutKeyword(this ArgumentSyntax argument)
			=> argument.RefOrOutKeyword.Text == "out";

		public static bool CheckIdentifierName(this ExpressionSyntax expression, string fieldName)
		{
			if (expression.CheckIdentifierNameWithThis(fieldName)) return false;
			if (expression.CheckIdentifierNameWithoutThis(fieldName)) return false;
			return true;
		}

		public static bool CheckIdentifierNameWithThis(this ExpressionSyntax expression, string fieldName)
		{
			var memberAccessExpression = expression as MemberAccessExpressionSyntax;
			if (memberAccessExpression == null) return false;

			var thisExpression = memberAccessExpression.Expression as ThisExpressionSyntax;
			if (thisExpression == null) return false;

			var identiferName = memberAccessExpression.Name as IdentifierNameSyntax;
			if (identiferName == null || identiferName.Identifier.Text != fieldName) return false;

			return true;
		}

		public static bool CheckIdentifierNameWithoutThis(this ExpressionSyntax expression, string fieldName)
		{
			var identiferName = expression as IdentifierNameSyntax;
			if (identiferName == null || identiferName.Identifier.Text != fieldName) return false;

			return true;
		}

		public static bool IsValueIdentifierName(this ExpressionSyntax expression)
		{
			var identiferName = expression as IdentifierNameSyntax;
			if (identiferName == null || identiferName.Identifier.Text != "value") return false;

			return true;
		}

		public static CompilationUnitSyntax WithUsing(this CompilationUnitSyntax source, string usingName)
			=> !source.Usings.Any(u => u.Name.ToString() == usingName)
				? source.AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(usingName)))
				: source;
	}
}