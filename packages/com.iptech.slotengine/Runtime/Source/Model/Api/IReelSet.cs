using System.Collections.Generic;

namespace IPTech.SlotEngine.Model.Api
{
	public interface IReelSet : ICollection<IReel>
	{
		string ID { get; set; }
		IReel this[int index] { get; }
	}
}