using System;

namespace Mntone.NotifyGenerator
{
	internal sealed class NameConversion
	{
		public static readonly NameConversion LeadingUnderscoreAndLowerCaseLetters = new NameConversion(s => s.Length == 1 ? "_" + char.ToLower(s[0]) : '_' + char.ToLower(s[0]) + s.Substring(1));
		public static readonly NameConversion LeadingUnderscore = new NameConversion(s => '_' + s);
		public static readonly NameConversion LeadingLowerCaseLetters = new NameConversion(s => s.Length == 1 ? char.ToLower(s[0]).ToString() : char.ToLower(s[0]) + s.Substring(1));
		//public static readonly NameConversion TrailingUnderscore = new NameConversion(s => s + '_');
		public static readonly NameConversion Hungarian = new NameConversion(s => 'm' + s);
		public static readonly NameConversion HungarianLikeCPlusPlus = new NameConversion(s => s.Length == 1 ? "m_" + char.ToLower(s[0]) : "m_" + char.ToLower(s[0]) + s.Substring(1));

		public NameConversion(Func<string, string> conversionFunction)
		{
			this.ConversionFunction = conversionFunction;
		}

		public Func<string, string> ConversionFunction { get; }
	}
}
