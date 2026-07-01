using System;

namespace IPTech.AgeVerification.iOS
{
	public class AgeRangeNotAvailableException : Exception
	{
		public AgeRangeNotAvailableException() : base("Age range service was not available") { }
		public AgeRangeNotAvailableException(string message) : base(message) { }
		public AgeRangeNotAvailableException(string message, Exception innerException) : base(message, innerException) { }
	}
}