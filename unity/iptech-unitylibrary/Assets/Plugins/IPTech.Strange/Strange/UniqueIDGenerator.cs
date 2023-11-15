using IPTech.Strange.Api;

namespace IPTech.Strange
{
	public class UniqueIDGenerator : IUniqueIDGenerator
	{
		private int intCounter = 0;

		public int GenerateIntID() {
			return intCounter++;
		}
	}
}
