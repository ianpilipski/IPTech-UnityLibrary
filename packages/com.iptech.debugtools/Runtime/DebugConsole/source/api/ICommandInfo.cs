using System;

namespace IPTech.DebugConsoleService.Api
{
	public enum ECommandType {
		Alias,
		Command
	}

	public interface ICommandInfo {
		string Command { get; }
		ECommandType CommandType { get; }
		string Help { get; }
		string Category { get; }
        string ShortName { get; }
	}
}

