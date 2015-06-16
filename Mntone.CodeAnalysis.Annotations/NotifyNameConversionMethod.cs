namespace Mntone.CodeAnalysis.Annotations
{
	public enum NotifyNameConversionMethod
	{
		/// <summary>
		/// Start with "_" and lower case letters.
		/// </summary>
		LeadingUnderscoreAndLowerCaseLetters,

		/// <summary>
		/// Start with "_".
		/// </summary>
		LeadingUnderscore,

		/// <summary>
		/// Start with lower case letters.
		/// </summary>
		LeadingLowerCaseLetters,

		/// <summary>
		/// End with "_".
		/// </summary>
		//TrailingUnderscore,
		
		/// <summary>
		/// Start with "m".
		/// </summary>
		Hungarian,

		/// <summary>
		/// Start with "m_" and lower case letters.
		/// </summary>
		HungarianLikeCPlusPlus,
	}
}
