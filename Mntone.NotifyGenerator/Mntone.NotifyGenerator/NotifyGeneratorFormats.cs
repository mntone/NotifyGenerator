namespace Mntone.NotifyGenerator
{
	internal static class NotifyGeneratorFormats
	{
		public const string BeginRegionTextFormat = "#region {0}\r\n\r\n";
		public const string EndRegionText = "#endregion\r\n";

		public const string GetPropertyTextWithThisFormat = "get {{ return this.{0}; }}\r\n";
		public const string GetPropertyTextWithoutThisFormat = "get {{ return {0}; }}\r\n";

		public const string SetPropertyTextWithThisFormat = "set {{ this._SetValue(ref this.{0}, value, {0}" + EventArgsNameText + "); }}\r\n";
		public const string SetPropertyTextWithoutThisFormat = "set {{ _SetValue(ref {0}, value, {0}" + EventArgsNameText + "); }}\r\n";

		public const string FieldHiddenText = "[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]";

		public const string FieldTextFormatWithPrivate = "private {0} {1};\r\n";
		public const string FieldTextFormatWithoutPrivate = "{0} {1};\r\n";

		public const string EventArgsNameText = "StaticPropertyChangedEventArgs";

		public const string EventArgsTextFormatWithPrivate = "private static readonly PropertyChangedEventArgs {0}" + EventArgsNameText + " = new PropertyChangedEventArgs(nameof({1}));\r\n";
		public const string EventArgsTextFormatWithoutPrivate = "static readonly PropertyChangedEventArgs {0}" + EventArgsNameText + " = new PropertyChangedEventArgs(nameof({1}));\r\n";

		public const string SetValueFunctionTextWithPrivate = @"private bool _SetValue<T>(ref T storage, T value, PropertyChangedEventArgs e)";
		public const string SetValueFunctionTextWithoutPrivate = @"bool _SetValue<T>(ref T storage, T value, PropertyChangedEventArgs e)";

		public const string CompareFormat = "if ({0}(storage, value))\r\n";
		public const string CompareEqualsText = "object.Equals";
		public const string CompareReferenceEqualsText = "object.ReferenceEquals";
		public const string CompareEqualityComparerEqualsText = "System.Collections.Generic.EqualityComparer<T>.Default.Equals";

		public const string AssignText = "storage = value;";

		public const string NonThreadSafeInvokeWithThisText = @"this.PropertyChanged?.Invoke(this, e);";
		public const string NonThreadSafeInvokeWithoutThisText = @"PropertyChanged?.Invoke(this, e);";
		public const string ThreadSafeInvokeWithThisText = @"System.Threading.Interlocked.CompareExchange(ref this.PropertyChanged, null, null)?.Invoke(this, e);";
		public const string ThreadSafeInvokeWithoutThisText = @"System.Threading.Interlocked.CompareExchange(ref PropertyChanged, null, null)?.Invoke(this, e);";

		public const string EventFieldText = "public event PropertyChangedEventHandler PropertyChanged;";

		public const string ReturnTrueText = "return true;";
		public const string ReturnFalseText = "return false;";
	}
}