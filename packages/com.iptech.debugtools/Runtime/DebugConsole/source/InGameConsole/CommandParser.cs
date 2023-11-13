#if !(UNITY_EDITOR || DEVELOPMENT_BUILD || QA_BUILD)
#define CONTAINERDEBUGSERVICE_DISABLED
#endif

#if !CONTAINERDEBUGSERVICE_DISABLED

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace IPTech.DebugConsoleService.InGameConsole {
    public static class CommandParser {
		public static bool TryParseCommand(string command, out List<string> parsedCommand) {
			bool inString = false;
			string currentString = "";
			List<string> results = new List<string>();
			for(int i = 0; i < command.Length; i++) {
				char token = command[i];
				if(inString) {
					if(token == '"') {
						results.Add(currentString);
						currentString = "";
					} else {
						currentString += token;
					}
				} else if(token == '"') {
					inString = true;
				} else if(token == ' ') {
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
			parsedCommand = results;
			return parsedCommand.Count > 0;
		}
	}
}

#endif


