using System;
using System.Diagnostics;

namespace Mntone.CodeAnalysis.Annotations
{
	[Conditional("NEVER_USED_AT_RUNTIME")]
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
	public sealed class NotifyGenerateAttribute : Attribute
	{
		public NotifyGenerateAttribute()
		{ }

		public bool UseThis { get; set; } = true;
		public bool UsePrivateAccessModifierClearly { get; set; } = true;
		public bool IsThreadSafe { get; set; } = true;
		public bool IsFieldHidden { get; set; } = true;
		public NotifyCompareMethod CompareMethod { get; set; } = NotifyCompareMethod.EqualityComparerEquals;
		public NotifyNameConversionMethod NameConversionMethod { get; set; } = NotifyNameConversionMethod.LeadingUnderscoreAndLowerCaseLetters;
	}
}
