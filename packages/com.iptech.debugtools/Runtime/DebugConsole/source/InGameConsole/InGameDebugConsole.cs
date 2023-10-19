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

namespace IPTech.DebugConsoleService.InGameConsole
{
	public class InGameDebugConsole {
		const string MESSAGE_SERVICE_UNAVAILABLE = "The debugConsoleService is unavailable.";
		const string MESSAGE_INVALID_COMMAND = "The command entered is invalid.";

		private IDebugConsoleService debugConsoleService;
		private IList<string> parsedCommand;
		private InGameDebugConsoleView inGameConsoleView;
		private IDebugCommands debugCommands;

		public string Command {
			get {
				return inGameConsoleView.Command;
			}
		}

        public InGameDebugConsole(IDebugConsoleService debugConsoleService) : this(debugConsoleService, new DebugCommands()) { }
        
		public InGameDebugConsole(IDebugConsoleService debugConsoleService, IDebugCommands debugCommands) {
			SetDebugConsoleService(debugConsoleService);
			this.debugCommands = debugCommands;
			debugCommands.RegisterDebugCommands(debugConsoleService);
		}

		public void ExecuteCommand() {
			if(IsDebugConsoleServiceAvailable()) {
				ParseCommand();
				if(IsParsedCommandValid()) {
					ProcessParsedCommand();
				} else {
					Output(MESSAGE_INVALID_COMMAND);
				}
			} else {
				Output(MESSAGE_SERVICE_UNAVAILABLE);
			}
		}

		private bool IsDebugConsoleServiceAvailable() {
			return this.debugConsoleService!=null;
		}

		private void ParseCommand() {
			bool inString = false;
			string currentString = "";
			List<string> results = new List<string>();
			for(int i=0;i<Command.Length;i++) {
				char token = Command[i];
				if(inString) {
					if( token == '"') {
						results.Add(currentString);
						currentString = "";
					} else {
						currentString += token;
					}
				} else if(token=='"') {
					inString = true;
				} else if(token==' ') {
					if(!string.IsNullOrEmpty(currentString)) {
						results.Add(currentString);
						currentString = "";
					}
				} else {
					currentString += token;
				}
			}
			if(!string.IsNullOrEmpty(currentString)) {
				results.Add(currentString);
			}
			this.parsedCommand = results;
		}

		private bool IsParsedCommandValid() {
			return this.parsedCommand.Count > 0;
		}

		private void ProcessParsedCommand() {
			if(this.parsedCommand[0]=="/?" || this.parsedCommand[0]=="/??") {
				ShowHelp();
			} else if(this.parsedCommand[0]=="/hide") {
				this.inGameConsoleView.MinimizeConsole();
			} else if(this.parsedCommand[0]=="/show") {
				this.inGameConsoleView.RestoreConsole();
			} else {
				SendParsedCommandToDebugConsoleService();
			}
		}

		private void ShowHelp() {
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("<b>** Available Commands **</b>");
            sb.AppendLine("<b>-------------------------</b>");
            sb.AppendLine("<b>category : command : help</b>");
            sb.AppendLine("<b>-------------------------</b>");
            if(this.parsedCommand.Count==1 || this.parsedCommand[1]=="console") {
				sb.AppendLine("console : <b>/?</b> : quick help (no alias commands)");
				sb.AppendLine("console : <b>/??</b> : quick help (show alias commands)");
				sb.AppendLine("console : <b>/?</b> <category> : only show help for specific category (no alias commands)");
				sb.AppendLine("console : <b>/??</b> <category> : only show help for specific category (with alias commands)");
				sb.AppendLine("console : <b>/cls</b> : clears the console scrollback");
                sb.AppendLine("console : <b>/hide</b> : hides the console");
                sb.AppendLine("console : <b>/show</b> : shows the console");                
			}
			bool showAliasCommands = this.parsedCommand[0] == "/??";
			foreach(ICommandInfo cmd in this.debugConsoleService.GetDebugCommands()) {
				if (cmd.CommandType == ECommandType.Alias && !showAliasCommands) {
					//don't list aliases
					continue;
				}
				if(this.parsedCommand.Count==1 
					|| this.parsedCommand[1].Equals(cmd.Category, StringComparison.InvariantCultureIgnoreCase)) {
					sb.AppendFormat("{0} : <b>{1}</b> : <i>{2}</i>\n",
						cmd.Category, cmd.Command, cmd.Help
					);
				}
			}
			Output(sb.ToString());
		}

		private void SendParsedCommandToDebugConsoleService() {
			this.debugConsoleService.ExecDebugCommand(
				this.parsedCommand[0], 
				this.parsedCommand.ToArray(),
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
			RegisterDebugCommands();
		}

		private void RegisterDebugCommands() {

		}

        private void HandleMessageLogged(string message, string category)
        {
            Output(string.Format("[{0}] : {1}", category, message));
        }

		public void SetInGameConsoleView(GameObject inGameConsoleView) {
			UnregisterHandler();
			this.inGameConsoleView = inGameConsoleView.GetComponentInChildren<InGameDebugConsoleView>(true);
			UpdateButtons();
			RegisterHandler();
		}

		private void UpdateButtons() {
			if(this.debugConsoleService!=null && this.inGameConsoleView!=null) {
				IList<ICommandInfo> cmdInfos = this.debugConsoleService.GetDebugCommands();
                IEnumerable<InGameDebugConsoleView.CommandData> actions = cmdInfos
					.Where( ci => ci.CommandType==ECommandType.Alias )
                    .Select( ci => new InGameDebugConsoleView.CommandData() { Category = ci.Category, Command = ci.Command, ShortName = ci.ShortName } );
					
				this.inGameConsoleView.UpdateButtons(actions);
			}
		}

		private void UnregisterHandler() {
			if(this.inGameConsoleView!=null) {
				this.inGameConsoleView.ExecuteCommandClicked.RemoveListener(ExecuteCommand);
				this.inGameConsoleView.WantsUpdatedButtons.RemoveListener(UpdateButtons);
			}
		}

		private void RegisterHandler() {
			if(this.inGameConsoleView!=null) {
				this.inGameConsoleView.ExecuteCommandClicked.AddListener(ExecuteCommand);
				this.inGameConsoleView.WantsUpdatedButtons.AddListener(UpdateButtons);
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

                    newConsole.SetInGameConsoleView(go);
                    return newConsole;
                }
            }
            return null;
        }
	}
}
#endif