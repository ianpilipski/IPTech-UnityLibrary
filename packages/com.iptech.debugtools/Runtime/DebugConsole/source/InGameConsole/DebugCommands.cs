using System;
using System.Text;
using IPTech.DebugConsoleService.Api;
using UnityEngine;

namespace IPTech.DebugConsoleService { 
	public interface IDebugCommands {
		void RegisterDebugCommands(IDebugConsoleService debugConsoleService);
	}

	public class DebugCommands : IDebugCommands {
		public void RegisterDebugCommands(IDebugConsoleService debugConsoleService) {
			debugConsoleService.RegisterCommand("sys", CMDSysInfo, "System", "Display system information");
			debugConsoleService.RegisterAlias("sys.info", "sys", "Info", "System", "Display system information");
		}

		private void CMDSysInfo(string[] args, Action<string> completedCallback) {
			var info = new StringBuilder();

			info.AppendFormat("Unity Ver: {0}\n", Application.unityVersion);
			info.AppendFormat("Platform: {0} Language: {1}\n", Application.platform, Application.systemLanguage);
			info.AppendFormat("Screen:({0},{1}) DPI:{2} Target:{3}fps\n", Screen.width, Screen.height, Screen.dpi, Application.targetFrameRate);
			info.AppendFormat("Level: {0} ({1} of {2})\n", global::UnityEngine.SceneManagement.SceneManager.GetActiveScene().name, global::UnityEngine.SceneManagement.SceneManager.GetActiveScene().name, global::UnityEngine.SceneManagement.SceneManager.sceneCount);
			info.AppendFormat("Quality: {0}\n", QualitySettings.names[QualitySettings.GetQualityLevel()]);
			info.AppendLine();
			info.AppendFormat("Data Path: {0}\n", Application.dataPath);
			info.AppendFormat("Cache Path: {0}\n", Application.temporaryCachePath);
			info.AppendFormat("Persistent Path: {0}\n", Application.persistentDataPath);
			info.AppendFormat("Streaming Path: {0}\n", Application.streamingAssetsPath);
			info.AppendLine();
			info.AppendFormat("Net Reachability: {0}\n", Application.internetReachability);
			info.AppendFormat("Multitouch: {0}\n", Input.multiTouchEnabled);

			completedCallback(info.ToString());
		}
	}
}
