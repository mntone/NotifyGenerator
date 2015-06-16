using System;
using System.Diagnostics;

namespace Mntone.CodeAnalysis.Annotations
{
	[Conditional("NEVER_USED_AT_RUNTIME")]
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
	public sealed class NotifyUngenerateAttribute : Attribute
	{ }
}