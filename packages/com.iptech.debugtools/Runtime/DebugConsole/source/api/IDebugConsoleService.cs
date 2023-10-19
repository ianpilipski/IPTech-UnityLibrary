using System;
using System.Collections.Generic;

namespace IPTech.DebugConsoleService.Api
{
	public delegate void DebugActionCallback(Action<string> completedCallback);
	public delegate void DebugCommandCallback(string[] args, Action<string> completedCallback);

	public interface IDebugConsoleService
	{
		void LogMessage(string message, string category);
		event Action<string, string> MessageLogged;

        void RegisterAlias(string commandString, string commandStringToExecute, string shortName, string category, string help);
		void UnregisterAlias(string commandName);

		void RegisterCommand(string commandName, DebugCommandCallback commandCallback, string category, string help);
		void UnregisterCommand(string commandName);

        void ExecDebugCommand(string commandStringToExec, Action<string> completed);
		void ExecDebugCommand(string commandName, string[] args, Action<string> completed);

		IList<ICommandInfo> GetDebugCommands();
	}
}

