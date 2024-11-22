#if !(UNITY_EDITOR || DEVELOPMENT_BUILD || QA_BUILD)
#define CONTAINERDEBUGSERVICE_DISABLED
#endif

#if !CONTAINERDEBUGSERVICE_DISABLED

using UnityEngine;
using IPTech.DebugConsoleService.Api;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine.UIElements;

namespace IPTech.DebugConsoleService.InGameConsole
{
	public interface IInGameDebugConsoleView {
		void MinimizeConsole();
        void RestoreConsole();
        void UpdateButtons(IEnumerable<InGameDebugConsoleView.CommandData> actions);
		event Action<string> OnExecuteCommand;
		event Action OnWantsUpdatedCommands;
		void Output(string msg);
		void Notify(string msg);
		void Show(bool value);
    }

	public class InGameDebugConsole {
		const string MESSAGE_SERVICE_UNAVAILABLE = "The debugConsoleService is unavailable.";
		const string MESSAGE_INVALID_COMMAND = "The command entered is invalid.";

		private IDebugConsoleService debugConsoleService;
		private IInGameDebugConsoleView inGameConsoleView;
		private IDebugCommands debugCommands;
		private List<InGameDebugConsoleView.CommandData> debugPanels;

		public InGameDebugConsole(IDebugConsoleService debugConsoleService) : this(debugConsoleService, new DebugCommands()) { }
        
		public InGameDebugConsole(IDebugConsoleService debugConsoleService, IDebugCommands debugCommands) {
			this.debugPanels = new List<InGameDebugConsoleView.CommandData>();
			SetDebugConsoleService(debugConsoleService);
			this.debugCommands = debugCommands;
			debugCommands.RegisterDebugCommands(debugConsoleService);
		}

		public void ExecuteCommand(string command) {
			if(IsDebugConsoleServiceAvailable()) {
				if(CommandParser.TryParseCommand(command, out var parsedCommand)) {
					ProcessParsedCommand(parsedCommand);
				} else {
					Output(MESSAGE_INVALID_COMMAND);
				}
			} else {
				Output(MESSAGE_SERVICE_UNAVAILABLE);
			}
		}

		public void RegisterDebugPanel(string category, string name, Func<VisualElement> panelFactory) {
			debugPanels.Add(new InGameDebugConsoleView.CommandData() {
				Category = category,
				Command = $"DEBUGPANEL.{category}.{name}",
				ShortName = name,
				visualElementFactory = panelFactory
			});
        }

		public void UnregisterDebugPanel(string category, string name) {
			for(int i = debugPanels.Count-1;i>=0;i--) {
				var dp = debugPanels[i];
				if(dp.Category == category && dp.ShortName == name) {
					debugPanels.RemoveAt(i);
					return;
                }
            }
        }

		private bool IsDebugConsoleServiceAvailable() {
			return this.debugConsoleService!=null;
		}

		private void ProcessParsedCommand(List<string> parsedCommand) {
			if(parsedCommand[0]=="/?" || parsedCommand[0]=="/??") {
				ShowHelp(parsedCommand);
			} else if(parsedCommand[0]=="/hide") {
				this.inGameConsoleView.MinimizeConsole();
			} else if(parsedCommand[0]=="/show") {
				this.inGameConsoleView.RestoreConsole();
			} else {
				SendParsedCommandToDebugConsoleService(parsedCommand);
			}
		}

		private void ShowHelp(List<string> parsedCommand) {
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("<b>** Available Commands **</b>");
            sb.AppendLine("<b>-------------------------</b>");
            sb.AppendLine("<b>category : command : help</b>");
            sb.AppendLine("<b>-------------------------</b>");
            if(parsedCommand.Count==1 || parsedCommand[1]=="console") {
				sb.AppendLine("console : <b>/?</b> : quick help (no alias commands)");
				sb.AppendLine("console : <b>/??</b> : quick help (show alias commands)");
				sb.AppendLine("console : <b>/?</b> <category> : only show help for specific category (no alias commands)");
				sb.AppendLine("console : <b>/??</b> <category> : only show help for specific category (with alias commands)");
				sb.AppendLine("console : <b>/cls</b> : clears the console scrollback");
                sb.AppendLine("console : <b>/hide</b> : hides the console");
                sb.AppendLine("console : <b>/show</b> : shows the console");                
			}
			bool showAliasCommands = parsedCommand[0] == "/??";
			foreach(ICommandInfo cmd in this.debugConsoleService.GetDebugCommands()) {
				if (cmd.CommandType == ECommandType.Alias && !showAliasCommands) {
					//don't list aliases
					continue;
				}
				if(parsedCommand.Count==1 
					|| parsedCommand[1].Equals(cmd.Category, StringComparison.InvariantCultureIgnoreCase)) {
					sb.AppendFormat("{0} : <b>{1}</b> : <i>{2}</i>\n",
						cmd.Category, cmd.Command, cmd.Help
					);
				}
			}
			Output(sb.ToString());
		}

		private void SendParsedCommandToDebugConsoleService(List<string> parsedCommand) {
			this.debugConsoleService.ExecDebugCommand(
				parsedCommand[0], 
				parsedCommand.ToArray(),
				CommandResultHandler
			);
		}

		private void CommandResultHandler(string result) {
			Output(result);
            Notify(result);
		}

		private void SetDebugConsoleService(IDebugConsoleService debugConsoleService) {
            this.debugConsoleService = debugConsoleService;	
            this.debugConsoleService.MessageLogged += HandleMessageLogged;
			this.debugConsoleService.ShowDebugViews += HandleShowDebugViews;
			RegisterDebugCommands();
		}

		private void RegisterDebugCommands() {

		}

        private void HandleMessageLogged(string message, string category)
        {
            Output(string.Format("[{0}] : {1}", category, message));
        }

		private void HandleShowDebugViews(bool value) {
			this.inGameConsoleView?.Show(value);
        }

		public void SetInGameConsoleView(IInGameDebugConsoleView inGameConsoleView) {
			UnregisterHandler();
			this.inGameConsoleView = inGameConsoleView;
			UpdateButtons();
			RegisterHandler();
		}

		private void UpdateButtons() {
			if(this.debugConsoleService!=null && this.inGameConsoleView!=null) {
				IList<ICommandInfo> cmdInfos = this.debugConsoleService.GetDebugCommands();
                IEnumerable<InGameDebugConsoleView.CommandData> actions = cmdInfos
					.Where( ci => ci.CommandType==ECommandType.Alias )
                    .Select( ci => new InGameDebugConsoleView.CommandData() { Category = ci.Category, Command = ci.Command, ShortName = ci.ShortName } )
					.Concat(debugPanels);
					
				this.inGameConsoleView.UpdateButtons(actions);
			}
		}

		private void UnregisterHandler() {
			if(this.inGameConsoleView!=null) {
				this.inGameConsoleView.OnExecuteCommand -= ExecuteCommand;
				this.inGameConsoleView.OnWantsUpdatedCommands -= UpdateButtons;
			}
		}

		private void RegisterHandler() {
			if(this.inGameConsoleView!=null) {
				this.inGameConsoleView.OnExecuteCommand += ExecuteCommand;
				this.inGameConsoleView.OnWantsUpdatedCommands += UpdateButtons;
			}
		}

		private void Output(string message) {
			this.inGameConsoleView.Output(message);
		}

        public void Notify(string message) {
            if(!string.IsNullOrEmpty(message)) {
                message = TakeLastLines(message, 10);
                this.inGameConsoleView.Notify(message);
            }
        }

        private string TakeLastLines(string text, int count)
        {
            StringBuilder retVal = new StringBuilder();
            Match match = Regex.Match(text, "^.*$", RegexOptions.Multiline | RegexOptions.RightToLeft);

            bool first = true;
            while (match.Success && 0<count--)
            {
                if(first) {
                    first = false;
                    retVal.Insert(0, match.Value);
                } else {
                    retVal.Insert(0, match.Value + "\n");
                }
                match = match.NextMatch();
            }

            return retVal.ToString();
        }

        public static InGameDebugConsole CreateDefault(IDebugConsoleService debugConsoleService) {
            InGameConsoleScriptableObject inGameConsoleSO = Resources.Load<InGameConsoleScriptableObject>("IPTech.DebugConsoleService.InGameConsoleView");
            if(inGameConsoleSO!=null && inGameConsoleSO.InGameConsoleViewPrefab!=null) {
                GameObject go = UnityEngine.Object.Instantiate(inGameConsoleSO.InGameConsoleViewPrefab);
                if(go!=null) {
                    InGameDebugConsole newConsole = new InGameDebugConsole(debugConsoleService);


                    newConsole.SetInGameConsoleView(go.GetComponentInChildren<IInGameDebugConsoleView>());
                    return newConsole;
                }
            }
            return null;
        }
	}
}
#endif