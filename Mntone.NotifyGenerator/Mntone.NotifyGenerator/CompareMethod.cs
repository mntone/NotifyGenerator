namespace Mntone.NotifyGenerator
{
	public enum CompareMethod
	{
		/// <summary>
		/// Always raise PropertyChanged.
		/// </summary>
		None,

		/// <summary>
		/// Use <see cref="object.Equals(object, object)"/> to compare old and new values.
		/// </summary>
		Equals,

		/// <summary>
		/// Use <see cref="object.ReferenceEquals(object, object)"/> to compare old and new values.
		/// </summary>
		ReferenceEquals,

		/// <summary>
		/// Use <see cref="System.Collections.Generic.EqualityComparer{T}.Equals(T, T)"/> to compare old and new values.
		/// </summary>
		EqualityComparerEquals,
	}
}