
namespace IPTech.SlotEngine.Unity.Model.Editor.Api
{
	public interface IReelEditor : IInspectorGUI
	{
		ISymbolEditor symbolEditor { get; }
	}
}
