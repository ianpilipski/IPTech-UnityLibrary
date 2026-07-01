using System;

namespace IPTech.AgeVerification.iOS
{
	public class AgeRangeInvalidRequestException : Exception
	{
		public AgeRangeInvalidRequestException() : base("Invalid age range request") { }
		public AgeRangeInvalidRequestException(string message) : base(message) { }
		public AgeRangeInvalidRequestException(string message, Exception innerException) : base(message, innerException) { }
	}
}