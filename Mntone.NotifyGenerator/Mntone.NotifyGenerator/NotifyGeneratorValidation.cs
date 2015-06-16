using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using Mntone.NotifyGenerator.Helpers;
using System.Linq;

namespace Mntone.NotifyGenerator
{
	internal static class NotifyGeneratorValidation
	{
		public const string DefaultAttributeName = "NotifyGenerate";
		public const string DefaultNotifyUngenerateAttributeName = "NotifyUngenerate";
		public const string DefaultRegionName = "NotifyGenerator";
		public const string FullINotifyProertyChangedText = "System.ComponentModel.INotifyPropertyChanged";
		public const string INotifyProertyChangedText = "INotifyPropertyChanged";

		public const string PropertyUseThisText = "UseThis";
		public const bool PropertyUseThisDefault = true;

		public const string PropertyUsePrivateAccessModifierClearlyText = "UsePrivateAccessModifierClearly";
		public const bool PropertyUsePrivateAccessModifierClearlyDefault = true;

		public const string PropertyIsThreadSafeText = "IsThreadSafe";
		public const bool PropertyIsThreadSafeDefault = true;

		public const string PropertyIsFieldHiddenText = "IsFieldHidden";
		public const bool PropertyIsFieldHiddenDefault = true;

		public const string PropertyCompareMethodText = "CompareMethod";
		public const string PropertyCompareMethodNone = "NotifyCompareMethod.None";
		public const string PropertyCompareMethodEquals = "NotifyCompareMethod.Equals";
		public const string PropertyCompareMethodReferenceEquals = "NotifyCompareMethod.ReferenceEquals";
		public const string PropertyCompareMethodEqualityComparerEquals = "NotifyCompareMethod.EqualityComparerEquals";
		public const string PropertyCompareMethodDefault = PropertyCompareMethodEqualityComparerEquals;

		public const string PropertyNameConversionMethodText = "NameConversionMethod";
		public const string PropertyNameConversionMethodLeadingUnderscoreAndLowerCaseLetters = "NotifyNameConversionMethod.LeadingUnderscoreAndLowerCaseLetters";
		public const string PropertyNameConversionMethodLeadingUnderscore = "NotifyNameConversionMethod.LeadingUnderscore";
		public const string PropertyNameConversionMethodLeadingLowerCaseLetters = "NotifyNameConversionMethod.LeadingLowerCaseLetters";
		//public const string PropertyNameConversionMethodTrailingUnderscore = "NotifyNameConversionMethod.TrailingUnderscore";
		public const string PropertyNameConversionMethodHungarian = "NotifyNameConversionMethod.Hungarian";
		public const string PropertyNameConversionMethodHungarianLikeCPlusPlus = "NotifyNameConversionMethod.HungarianLikeCPlusPlus";
		public const string PropertyNameConversionMethodDefault = PropertyNameConversionMethodLeadingUnderscoreAndLowerCaseLetters;

		public static bool HasSelfAttribute(
			this ClassDeclarationSyntax @class,
			out AttributeSyntax notifyAttributeSyntaxWithClassDeclaration)
		{
			notifyAttributeSyntaxWithClassDeclaration = @class.FindAttributes(DefaultAttributeName).SingleOrDefault();
			var properties = CodeAnalysisExtensions.GetProperties(@class);
			var propertiesWithSelfAttribute = properties.Where(p => p.FindAttributes(DefaultAttributeName).Any());
			return notifyAttributeSyntaxWithClassDeclaration != null || propertiesWithSelfAttribute.Any();
		}


		public static bool GetUseThisOrDefault(this AttributeSyntax attribute)
			=> attribute.GetBooleanPropertyValue(PropertyUseThisText) ?? PropertyUseThisDefault;

		public static bool GetUsePrivateAccessModifierClearlyOrDefault(this AttributeSyntax attribute)
			=> attribute.GetBooleanPropertyValue(PropertyUsePrivateAccessModifierClearlyText) ?? PropertyUsePrivateAccessModifierClearlyDefault;

		public static bool GetIsThreadSafeOrDefault(this AttributeSyntax attribute)
			=> attribute.GetBooleanPropertyValue(PropertyIsThreadSafeText) ?? PropertyIsThreadSafeDefault;

		public static bool GetIsFieldHiddenOrDefault(this AttributeSyntax attribute)
			=> attribute.GetBooleanPropertyValue(PropertyIsFieldHiddenText) ?? PropertyIsFieldHiddenDefault;

		public static CompareMethod GetCompareMethodOrDefault(this AttributeSyntax attribute)
		{
			var propertyValue = attribute.GetPropertyValue(PropertyCompareMethodText)?.ToString();
			if (propertyValue == PropertyCompareMethodNone)
			{
				return CompareMethod.None;
			}
			if (propertyValue == PropertyCompareMethodEquals)
			{
				return CompareMethod.Equals;
			}
			if (propertyValue == PropertyCompareMethodReferenceEquals)
			{
				return CompareMethod.ReferenceEquals;
			}
			return CompareMethod.EqualityComparerEquals;
		}

		public static NameConversion GetNameConversionOrDefault(this AttributeSyntax attribute)
		{
			var propertyValue = attribute.GetPropertyValue(PropertyNameConversionMethodText)?.ToString();
			if (propertyValue == PropertyNameConversionMethodLeadingUnderscore)
			{
				return NameConversion.LeadingUnderscore;
			}
			if (propertyValue == PropertyNameConversionMethodLeadingLowerCaseLetters)
			{
				return NameConversion.LeadingLowerCaseLetters;
			}
			//if (propertyValue == PropertyNameConversionMethodTrailingUnderscore)
			//{
			//	return NameConversion.TrailingUnderscore;
			//}
			if (propertyValue == PropertyNameConversionMethodHungarian)
			{
				return NameConversion.Hungarian;
			}
			if (propertyValue == PropertyNameConversionMethodHungarianLikeCPlusPlus)
			{
				return NameConversion.HungarianLikeCPlusPlus;
			}
			return NameConversion.LeadingUnderscoreAndLowerCaseLetters;
		}


		public static bool NeedImplemantation(
			this PropertyDeclarationSyntax property,
			bool classHasNotify,
			NameConversion nameConversion,
			bool useThis,
			out bool isTarget)
		{
			isTarget = false;

			var hasGenerate = property.FindAttributes(DefaultAttributeName).SingleOrDefault();
			if (!classHasNotify && hasGenerate == null) return false;

			var hasUngenerate = property.FindAttributes(DefaultNotifyUngenerateAttributeName).SingleOrDefault();
			if (hasUngenerate != null) return false;

			var isPublicOrInternalOrProtected = property.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword) || m.IsKind(SyntaxKind.InternalKeyword) || m.IsKind(SyntaxKind.ProtectedKeyword));
			if (!isPublicOrInternalOrProtected) return false;

			var getAccessor = property.GetGetAccessorOrNull();
			if (getAccessor == null) return false;

			var setAccessor = property.GetSetAccessorOrNull();
			if (setAccessor == null) return false;

			var propertyName = property.Identifier.Text;
			var fieldName = nameConversion.ConversionFunction(propertyName);
			var isGetAccessorAutoGenerate = getAccessor.CheckAutoGenerateAccessor();
			var isSetAccessorAutoGenerate = setAccessor.CheckAutoGenerateAccessor();
			var isGetAccessorGetValue = getAccessor.CheckSimpleGetAccessor(fieldName, useThis);
			var isSetAccessorSetValue = setAccessor.CheckSetValueSetAccessor("_SetValue", fieldName, fieldName + NotifyGeneratorFormats.EventArgsNameText, useThis);

			var isAutoGetSet = isGetAccessorAutoGenerate && isSetAccessorAutoGenerate;
			var isNotAutoGetSet = !isGetAccessorAutoGenerate && !isSetAccessorAutoGenerate;
			var isPropertyChangedGetSet = isGetAccessorGetValue && isSetAccessorSetValue;
			var isNotPropertyChangedGetSet = !isGetAccessorGetValue && !isSetAccessorSetValue;

			isTarget = isAutoGetSet && isNotPropertyChangedGetSet || isPropertyChangedGetSet && isNotAutoGetSet;
			return isAutoGetSet && isNotPropertyChangedGetSet;
		}


		public static bool HasINotifyProertyChanged(this ClassDeclarationSyntax @class)
			=> @class.GetBaseTypes().Any(b => b.Type.ToString().EndsWith(INotifyProertyChangedText));

		public static ClassDeclarationSyntax WithINotifyPropertyChangedInterface(this ClassDeclarationSyntax @class)
		{
			if (!@class.HasINotifyProertyChanged())
			{
				var iNotifyPropertyChangedSyntax = SyntaxFactory.ParseTypeName(FullINotifyProertyChangedText)
					.WithAdditionalAnnotations(Simplifier.Annotation)
					.WithAdditionalAnnotations(Formatter.Annotation);

				var newClass = @class.AddBaseListTypes(SyntaxFactory.SimpleBaseType(iNotifyPropertyChangedSyntax));
				return newClass.WithIdentifier(newClass.Identifier.WithTrailingTrivia());
			}

			return @class;
		}

		public static bool CheckSetValueSetAccessor(this AccessorDeclarationSyntax accessor, string functionName, string fieldName, string eventArgsName, bool isThisKeywordNecessary)
		{
			var expressionStatement = accessor.Body?.Statements.FirstOrDefault() as ExpressionStatementSyntax;
			if (expressionStatement == null) return false;

			var invocationExpression = expressionStatement.Expression as InvocationExpressionSyntax;

			// MemberAccess
			if (isThisKeywordNecessary)
			{
				if (!invocationExpression.Expression.CheckIdentifierNameWithThis(functionName)) return false;
			}
			else
			{
				if (!invocationExpression.Expression.CheckIdentifierNameWithoutThis(functionName)) return false;
			}

			// Argument
			var arguments = invocationExpression.ArgumentList.Arguments;
			if (arguments.Count != 3) return false;
			if (!arguments[0].HasRefKeyword()) return false;
			if (isThisKeywordNecessary)
			{
				if (!arguments[0].Expression.CheckIdentifierNameWithThis(fieldName)) return false;
			}
			else
			{
				if (!arguments[0].Expression.CheckIdentifierNameWithoutThis(fieldName)) return false;
			}
			if (!arguments[1].Expression.IsValueIdentifierName()) return false;
			if (!arguments[2].Expression.CheckIdentifierNameWithoutThis(eventArgsName)) return false;

			return true;
		}
	}
}