

namespace IPTech.SlotEngine.Unity.Model.Editor.Api
{
	public interface ISlotModelEditor : IInspectorGUI
	{
		IReelSetEditor reelSetEditor { get; }
	}
}
