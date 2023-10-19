#if !(UNITY_EDITOR || DEVELOPMENT_BUILD || QA_BUILD)
#define CONTAINERDEBUGSERVICE_DISABLED
#endif

#if !CONTAINERDEBUGSERVICE_DISABLED

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IPTech.DebugConsoleService.Api;
using System.Text;

namespace IPTech.DebugConsoleService
{
    public class DebugConsoleService : IDebugConsoleService {
        public static string WEAK_REF_NO_LONGER_AVAIL = "The command has been garbage collected and is no longer available.";
        public static string COMMAND_NOT_REGISTERED = "The command requested has not been registered.";

        static private IDictionary<string, CommandInfo> preRegisteredCommands = new Dictionary<string, CommandInfo>(StringComparer.InvariantCultureIgnoreCase);

        private IDictionary<string, CommandInfo> commandDictionary;

        private Action<string, string> messageLogged;

		public DebugConsoleService(bool useBuiltInCommands = true) {
            this.commandDictionary = new Dictionary<string, CommandInfo>(preRegisteredCommands, StringComparer.InvariantCultureIgnoreCase);

			if(useBuiltInCommands) {
				DebugConsoleServiceMemoryCommands.RegisterDebugCommands(this);
			}
        }

        public static void PreRegisterCommand(string commandString, DebugCommandCallback commandCallback, string category, string help = null) {
            CommandInfo cmd = CommandInfo.CreateCommand(commandString, commandCallback, category, help);
            DebugConsoleService.preRegisteredCommands.Add(commandString, cmd);
        }

        public static void PreRegisterAlias(string commandString, string commandStringToExecute, string shortName, string category, string help = null) {
            CommandInfo cmd = CommandInfo.CreateAlias(commandString, commandStringToExecute, shortName, category, help);
            DebugConsoleService.preRegisteredCommands.Add(commandString, cmd);
        }

        private void LogError(Exception e) {
            UnityEngine.Debug.LogError(e.ToString());
        }

        #region IContainerDebugService implementation

        public event Action<string,string> MessageLogged
        {
            add {
                this.messageLogged += value;
            }
            remove {
                this.messageLogged -= value;
            }
        }

        public void LogMessage(string message, string category = null) {
            if(this.messageLogged != null) {
                Delegate[] dels = this.messageLogged.GetInvocationList();
                for(int i = 0; i < dels.Length; i++) {
                    Delegate del = dels[i];
                    try {
                        del.DynamicInvoke(message, category ?? string.Empty);
                    } catch(Exception e) {
                        LogError(e);
                    }
                }
            }
        }

        public void RegisterAlias(string commandString, string commandStringToExecute, string shortName, string category, string help = null) {
            CommandInfo cmd = CommandInfo.CreateAlias(commandString, commandStringToExecute, shortName, category, help);
            InternalRegsiterCommand(cmd);
        }

        public void UnregisterAlias(string aliasCommandName) {
            UnregisterCommand(aliasCommandName);
        }

        public void RegisterCommand(string commandString, DebugCommandCallback commandCallback, string category, string help = null) {
            CommandInfo cmd = CommandInfo.CreateCommand(commandString, commandCallback, category, help);
            InternalRegsiterCommand(cmd);
        }

        private void InternalRegsiterCommand(CommandInfo cmd) {
            CommandInfo cmdExists;
            if(this.commandDictionary.TryGetValue(cmd.Command, out cmdExists)) {
                if(!cmdExists.IsAlive) {
                    this.commandDictionary[cmd.Command] = cmd;
                    return;
                }
            }
            this.commandDictionary.Add(cmd.Command, cmd);
        }

        public void UnregisterCommand(string commandString) {
            if(this.commandDictionary.ContainsKey(commandString)) {
                this.commandDictionary.Remove(commandString);
            }
        }

        public void ExecDebugCommand(string commandString, Action<string> completedCallback) {
            string[] args = ParseCommandString(commandString);
            ExecDebugCommand(args[0], args, completedCallback);
        }

        public void ExecDebugCommand(string commandName, string[] args, Action<string> completedCallback) {
            CommandInfo cmdInfo;
            string retVal = COMMAND_NOT_REGISTERED;
            if(this.commandDictionary.TryGetValue(commandName, out cmdInfo)) {
                if(cmdInfo.CommandType == ECommandType.Alias) {
                    string newCommandString = CreateNewCommandString(cmdInfo.AliasCommand, args);
                    ExecDebugCommand(newCommandString, completedCallback);
                    return;
                }
                if(cmdInfo.IsAlive) {
                    cmdInfo.Invoke(args, completedCallback);
                    return;
                }
                else {
                    retVal = WEAK_REF_NO_LONGER_AVAIL;
                }
            }
            if(completedCallback != null) {
                try {
                    completedCallback(retVal);
                } catch {
                }
            }
        }

        private string CreateNewCommandString(string firstString, string[] origArgs) {
            StringBuilder sb = new StringBuilder(firstString);
            if(origArgs!=null) {
                for(int i=1;i<origArgs.Length;i++) {
                    sb.AppendFormat(" {0}", origArgs[i]);
                }
            }
            return sb.ToString();
        }

        private string[] ParseCommandString(string commandString) {
            bool inString = false;
            string currentString = "";
            List<string> results = new List<string>();
            for(int i=0;i<commandString.Length;i++) {
                char token = commandString[i];
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
            return results.ToArray();
        }

        public IList<ICommandInfo> GetDebugCommands() {
			List<ICommandInfo> list = this.commandDictionary.Values.Select(ci => (ICommandInfo)ci).ToList();
			list.Sort((x,y) => {
				int cat = x.Category.CompareTo(y.Category);
				if(cat==0) {
					return x.Command.CompareTo(y.Command);
				}
				return cat;
			});
			return list;
        }

        #endregion

        #region CommandInfo

        private class CommandInfo : ICommandInfo {

            public ECommandType CommandType { get; private set; }
            public string Command { get; private set; }
            public string Category { get; private set; }
            public string ShortName { get; private set; }
            public string Help { get; private set; }
            public string AliasCommand { get; private set; }

            private WeakDelegate<DebugCommandCallback> WeakCallback;
           
            private CommandInfo(ECommandType cType, string command, string shortName, string category, string aliasCommand, string help) {
                this.CommandType = cType;
                this.Command = command;
                this.ShortName = shortName;
                this.AliasCommand = aliasCommand;
                this.Category = category ?? string.Empty;
                this.Help = help ?? string.Empty;
            }

            private CommandInfo(ECommandType cType, string command, DebugCommandCallback callback, string category, string help)
                : this(cType, command, command, category, command, help) {
                this.WeakCallback = new WeakDelegate<DebugCommandCallback>(callback);
            }

            public static CommandInfo CreateCommand(string command, DebugCommandCallback callback, string category, string help) {
                return new CommandInfo(ECommandType.Command, command, callback, category, help);
            }

            public static CommandInfo CreateAlias(string command, string commandStringToExec, string shortName, string category, string help) {
                return new CommandInfo(ECommandType.Alias, command, shortName, category, commandStringToExec, help);
            }

            public bool IsAlive { 
                get {
                    if(this.CommandType == ECommandType.Alias) return true;
                    return this.WeakCallback.IsAlive;
                }
            }

            public void Invoke(string[] args, Action<string> result) {
                Action<string> safeCallback = (s) => {
                    if(result != null) {
                        try {
                            result(s);
                        } catch {
                        }
                    }
                };
                try {
                    this.WeakCallback.GetDelegate().Invoke(args, safeCallback);
                } catch(Exception e) {
                    safeCallback("ERROR: " + e.ToString());
                }
            }
        }

        #endregion

        #region WeakDelegate

        private class WeakDelegate<TDelegate> : IEquatable<TDelegate> {
            private WeakReference _targetReference;
            private MethodInfo _method;

            public WeakDelegate(Delegate realDelegate) {
                if(realDelegate.Target != null)
                    _targetReference = new WeakReference(realDelegate.Target);
                else
                    _targetReference = null;
                _method = realDelegate.Method;
            }

            public TDelegate GetDelegate() {
                return (TDelegate)(object)GetDelegateInternal();
            }

            private Delegate GetDelegateInternal() {
                if(_targetReference != null) {
                    return Delegate.CreateDelegate(typeof(TDelegate), _targetReference.Target, _method);
                }
                else {
                    return Delegate.CreateDelegate(typeof(TDelegate), _method);
                }
            }

            public bool IsAlive {
                get { return _targetReference == null || _targetReference.IsAlive; }
            }


            #region IEquatable<TDelegate> Members

            public bool Equals(TDelegate other) {
                Delegate d = (Delegate)(object)other;
                return d != null
                && d.Target == _targetReference.Target
                && d.Method.Equals(_method);
            }

            #endregion
        }

        #endregion
    }
}

#endif

