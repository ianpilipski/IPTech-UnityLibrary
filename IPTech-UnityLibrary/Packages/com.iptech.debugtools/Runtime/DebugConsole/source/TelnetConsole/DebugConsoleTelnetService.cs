#if !(UNITY_EDITOR || DEVELOPMENT_BUILD || QA_BUILD)
#define CONTAINERDEBUGSERVICE_DISABLED
#endif

#if !CONTAINERDEBUGSERVICE_DISABLED
using System;
using System.Net;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using IPTech.DebugConsoleService.Api;
using System.Linq;

namespace IPTech.DebugConsoleService.TelnetConsole
{
	public class DebugConsoleTelnetService : IDisposable {
		private IDebugConsoleService debugConsoleService;
		private ITelnetServer telnetServer;
		private MonoBehaviour monoBehaviour;
		private object semaphore;

		private class ClientMessage {
			public TelnetClient client;
			public string message;
			public ClientMessage(TelnetClient client, string message) {
				this.client = client;
				this.message = message;
			}
		}

		private Queue<ClientMessage> queue;
		private int port;
		
		public DebugConsoleTelnetService(IDebugConsoleService debugConsoleService, int port = 2323, ITelnetServerFactory serverFactory = null) {
			this.semaphore = new object();
			this.queue = new Queue<ClientMessage>();
			this.port = port;
			this.debugConsoleService = debugConsoleService;

			serverFactory = serverFactory ?? new TelnetServer.Factory();
			this.telnetServer = serverFactory.Create(IPAddress.Any, port);
			telnetServer.ClientConnected       += clientConnected;
			telnetServer.ClientDisconnected    += clientDisconnected;
			telnetServer.ConnectionBlocked     += connectionBlocked;
			telnetServer.MessageReceived       += messageReceived;
			Start();
		}

		private void LogStartupMessage(int port) {
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("telnet console enabled, connect with> telnet <ipaddress> " + port);
			sb.AppendLine("  ipaddress(es):");
			try {
				IPAddress[] hostAddresses = Dns.GetHostAddresses(Dns.GetHostName());
				foreach(IPAddress a in hostAddresses) {
					sb.AppendLine("      " + a.ToString());
				}
			} catch {
				sb.AppendLine("unable to determine");
			}
			this.debugConsoleService.LogMessage(sb.ToString(), "telnet");
		}

		private IEnumerator Update() {
			bool executingCommand = false;
			while(true) {
				if(!executingCommand) {
					if(this.queue.Count > 0) {	
						ClientMessage cm;
						lock(this.semaphore) {
							cm = this.queue.Dequeue();
						}
						executingCommand = true;
						ExecuteCommand(cm.message, s => {
							executingCommand = false;
							Console.WriteLine(s);
							s = s.Replace("\n","\r\n");
							this.telnetServer.sendMessageToClient(cm.client, "\r\n" + s + "\r\n > ");
						});
					}
				}
				yield return null;
			}
		}

		public void Start() {
			try {
				this.telnetServer.start();
			} catch(Exception e) {
				Console.WriteLine("FAILED TO START TELNET SERVER: Port=" + port);
				Console.WriteLine(e.ToString());
				return;
			}
			Console.WriteLine("TELNET SERVER STARTED: Port=" + port);
			LogStartupMessage(port);
			HookUpdateCoroutine();
		}

		private void HookUpdateCoroutine() {
			if(Application.isPlaying) {
				GameObject go = new GameObject("DebugConsoleTelnet");
				this.monoBehaviour = go.AddComponent<MonoBehaviour>();
				this.monoBehaviour.StartCoroutine(Update());
				UnityEngine.Object.DontDestroyOnLoad(go);
			}
		}

		public void Stop() {
			this.telnetServer.stop();
			Console.WriteLine("TELNET SERVER STOPPED: Port=" + port);
		}

		private void UnhookUpdateCoroutine() {
			if(monoBehaviour!=null && Application.isPlaying) {
				this.monoBehaviour.StopAllCoroutines();
				GameObject.Destroy(this.monoBehaviour.gameObject);
				this.monoBehaviour = null;
			}
		}

		private void clientConnected(TelnetClient c)
		{
			Console.WriteLine("CONNECTED: " + c);
			// Just log in, if we want auth in the future remove this line.
			c.setStatus(EClientStatus.LoggedIn);
			telnetServer.sendMessageToClient(c, "IPTech Telnet Server\r\nUse '/?' to see available commands\r\n > ");
		}

		private void clientDisconnected(TelnetClient c)
		{
			Console.WriteLine("DISCONNECTED: " + c);
		}

		private void connectionBlocked(IPEndPoint ep)
		{
			Console.WriteLine(string.Format("BLOCKED: {0}:{1} at {2}", ep.Address, ep.Port, DateTime.Now));
		}

		private void messageReceived(TelnetClient c, string message)
		{
			Console.WriteLine("MESSAGE: " + message);

			if(message != "exit") {
				EClientStatus status = c.getCurrentStatus();

				if(status == EClientStatus.Guest) {
					if(message == "root") {
						telnetServer.sendMessageToClient(c, "\r\nPassword: ");
						c.setStatus(EClientStatus.Authenticating);
					}
				} else if(status == EClientStatus.Authenticating) {
					if(message == "r00t") {
						telnetServer.clearClientScreen(c);
						telnetServer.sendMessageToClient(c, "Successfully authenticated.\r\n > ");
						c.setStatus(EClientStatus.LoggedIn);
					}
				} else {
					lock(this.semaphore) {
						this.queue.Enqueue(new ClientMessage(c, message));
					}
				}
			} else {
				telnetServer.kickClient(c);
			}
		}

		private void ExecuteCommand(string message, Action<string> callback) {
			try {
				IList<string> parsedCommand = ParseCommand(message);
				if(parsedCommand.Count>0) {
					if(parsedCommand[0]=="/?" || parsedCommand[0]=="/??") {
						ShowHelp(parsedCommand, callback);
					} else {
						this.debugConsoleService.ExecDebugCommand(parsedCommand[0], parsedCommand.ToArray(), callback);
					}
				} else {
					callback("");
				}
			} catch {
				callback("something went wrong");
			}
		}

		private void ShowHelp(IList<string> parsedCommand, Action<string> callback) {
			IList<ICommandInfo> debugCommands = this.debugConsoleService.GetDebugCommands();
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("** Available Commands **");
			if(parsedCommand.Count==1) {
				sb.AppendLine("------------------------");
				sb.AppendLine("/?  : displays help (no alias commands)");
				sb.AppendLine("/?? : display help (with alias commands listed");
				sb.AppendLine("/? <category> : displays commands for the supplied category only (no alias commands)");
				sb.AppendLine("/?? <category> : displays commands for the supplied category only (with alias commands)");
				sb.AppendLine("exit : end the terminal session");
			}
			sb.AppendLine("-------------------------");
			sb.AppendLine("category : command : help");
			sb.AppendLine("-------------------------");

			bool includeAliasCommands = parsedCommand[0] == "/??";
			
			string formatString = CalculateFormatString(debugCommands, includeAliasCommands);

			foreach(ICommandInfo command in debugCommands) {
				if (command.CommandType == ECommandType.Alias && !includeAliasCommands) {
					// don't show alias commands
					continue;
				}
				if(parsedCommand.Count==1 || parsedCommand[1].Equals(command.Category, StringComparison.InvariantCultureIgnoreCase)) {
					sb.AppendFormat(formatString, command.Category, command.Command, command.Help);
				}
			}
			callback(sb.ToString());
		}

		private string CalculateFormatString(IList<ICommandInfo> debugCommands, bool includeAlias) {
			int catLength = 10;
			int cmdLength = 10;
			foreach(ICommandInfo command in debugCommands) {
				catLength = Math.Max(catLength, command.Category.Length);
				cmdLength = Math.Max(cmdLength, command.Command.Length);
			}
			return "{0,-" + catLength + "} : {1,-" + cmdLength + "} : {2}\n";
		}

		private IList<string> ParseCommand(string Command) {
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
			return results;
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected void Dispose(bool isDisposing) {
			if(isDisposing) {
				UnhookUpdateCoroutine();
				if(telnetServer != null) {
					telnetServer.Dispose();
					telnetServer = null;
				}
			}
		}
	}
}
#endif

