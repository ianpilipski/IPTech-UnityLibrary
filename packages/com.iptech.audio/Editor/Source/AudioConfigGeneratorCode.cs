using System;
namespace IPTech.Audio.EditorExtensions {
	public partial class AudioConfigGenerator {
		AudioEngineConfig config;
		string className;

		public AudioConfigGenerator(AudioEngineConfig config) {
			this.config = config;
			className = MakeSafeCodeName(config.name);
		}

		string MakeSafeCodeName(string name) {
			return AudioEngineConfig.MakeSafeCodeName(name);
		}
	}
}
