#if (DEBUG || UNITY_EDITOR || QA_BUILD || DEVELOPMENT_BUILD)


#define DEBUG_CONSOLE
#endif 

#define DEBUG_LEVEL_LOG
#define DEBUG_LEVEL_WARN
#define DEBUG_LEVEL_ERROR

#if (UNITY_EDITOR || DEVELOPMENT_BUILD || QA_BUILD)
#define QADEBUG
#endif

#if (UNITY_IOS || UNITY_ANDROID)
#define MOBILE
#endif

// V.M10.D31.2011.R1
/************************************************************************
* DebugConsole.cs
* Copyright 2011 Calvin Rien
* (http://the.darktable.com)
*
* Derived from version 2.0 of Jeremy Hollingsworth's DebugConsole
*
* Copyright 2008-2010 By: Jeremy Hollingsworth
* (http://www.ennanzus-interactive.com)
*
* Licensed for commercial, non-commercial, and educational use.
*
* THIS PRODUCT IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND. THE
* LICENSOR MAKES NO WARRANTY REGARDING THE PRODUCT, EXPRESS OR IMPLIED.
* THE LICENSOR EXPRESSLY DISCLAIMS AND THE LICENSEE HEREBY WAIVES ALL
* WARRANTIES, EXPRESS OR IMPLIED, INCLUDING, BUT NOT LIMITED TO, ALL
* IMPLIED WARRANTIES OF MERCHANTABILITY AND ALL IMPLIED WARRANTIES OF
* FITNESS FOR A PARTICULAR PURPOSE.
* ************************************************************************/
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;
using IPTech.DebugConsoleService.Api;

namespace IPTechShared {
	
	/// <summary>
	/// Provides a game-mode, multi-line console with command binding, logging and watch vars.
	///
	/// ==== Installation ====
	/// Just drop this script into your project. To use from JavaScript(UnityScript), just make sure
	/// you place this script in a folder such as "Plugins" so that it is compiled before your js code.
	///
	/// See the following Unity docs page for more info on this:
	/// http://unity3d.com/support/documentation/ScriptReference/index.Script_compilation_28Advanced29.html
	///
	/// ==== Usage (Logging) ====
	///
	/// To use, you only need to access the desired static Log functions. So, for example, to log a simple
	/// message you would do the following:
	///
	/// \code
	/// DebugConsole.Log("Hello World!");
	/// DebugConsole.LogWarning("Careful!");
	/// DebugConsole.LogError("Danger!");
	///
	/// // Now open it
	/// DebugConsole.IsOpen = true;
	/// \endcode
	///
	/// You can log any object that has a functional ToString() method.
	///
	/// Those static methods will automatically ensure that the console has been set up in your scene for you,
	/// so no need to worry about attaching this script to anything.
	///
	/// See the comments for the other static functions below for details on their use.
	///
	/// ==== Usage (DebugCommand Binding) ====
	///
	/// To use command binding, you create a function to handle the command, then you register that function
	/// along with the string used to invoke it with the console.
	///
	/// So, for example, if you want to have a command called "ShowFPS", you would first create the handler like
	/// this:
	///
	/// \code
	/// // JavaScript
	/// function ShowFPSCommand(args)
	/// {
	///     //...
	///   return "value you want printed to console";
	/// }
	///
	/// // C#
	/// public object ShowFPSCommand(params string[] args)
	/// {
	///     //...
	///   return "value you want printed to console";
	/// }
	/// \endcode
	///
	/// Then, to register the command with the console to be run when "ShowFPS" is typed, you would do the following:
	///
	/// \code
	/// DebugConsole.RegisterCommand("ShowFPS", ShowFPSCommand);
	/// \endcode
	///
	/// That's it! Now when the user types "ShowFPS" in the console and hits enter, your function will be run.
	///
	/// You can also use anonymous functions to register commands
	/// \code
	/// DebugConsole.RegisterCommand("echo", args => {if (args.Length < 2) return ""; args[0] = ""; return string.Join(" ", args);});
	/// \endcode
	///
	/// If you wish to capture input entered after the command text, the args array will contain every space-separated
	/// block of text the user entered after the command. "SetFOV 90" would pass the string "90" to the SetFOV command.
	///
	/// Note: Typing "/?" followed by enter will show the list of currently-registered commands.
	///
	/// ==== Usage (Watch Vars) ===
	///
	/// For the Watch Vars feature, you need to use the provided class, or your own subclass of WatchVarBase, to store
	/// the value of your variable in your project. You then register that WatchVar with the console for tracking.
	///
	/// Example:
	/// \code
	/// // JavaScript
	/// var myWatchInt = new WatchVar<int>("PowerupCount", 23);
	///
	/// myWatchInt.Value = 230;
	///
	/// myWatchInt.UnRegister();
	/// myWatchInt.Register();
	/// \endcode
	///
	/// As you use that WatchVar<int> to store your value through the project, its live value will be shown in the console.
	///
	/// You can create a WatchVar<T> for any object that has a functional ToString() method;
	///
	/// If you subclass WatchVarBase, you can create your own WatchVars to represent more types than are currently built-in.
	/// </summary>
	///
	#if DEBUG_CONSOLE
	public class DebugConsole : MonoBehaviour {
		readonly string VERSION = "3.0";
		readonly string ENTRYFIELD = "DebugConsoleEntryField";
		
		public struct DebugCommandData {
			public string name;
			public string help;
			public DebugCommand callback;
			
			public DebugCommandData(string _name, string _help, DebugCommand _callback) {
				name = _name;
				help = _help;
				callback = _callback;
			}
		}

		private const float MAX_SCALE = .9f;
		
		/// <summary>
		/// This is the signature for the DebugCommand delegate if you use the command binding.
		///
		/// So, if you have a JavaScript function named "SetFOV", that you wanted run when typing a
		/// debug command, it would have to have the following definition:
		///
		/// \code
		/// function SetFOV(args)
		/// {
		///     //...
		///   return "value you want printed to console";
		/// }
		/// \endcode
		/// </summary>
		/// <param name="args">The text typed in the console after the name of the command.</param>
		public delegate object DebugCommand(params string[] args);
		
		/// <summary>
		/// How many lines of text this console will display.
		/// </summary>
		public int maxLinesForDisplay = 500;
		
		/// <summary>
		/// Default color of the standard display text.
		/// </summary>
		public Color defaultColor = Message.defaultColor;
		public Color warningColor = Message.warningColor;
		public Color errorColor = Message.errorColor;
		public Color systemColor = Message.systemColor;
		public Color inputColor = Message.inputColor;
		public Color outputColor = Message.outputColor;
		
		/// <summary>
		/// Used to check (or toggle) the open state of the console.
		/// </summary>
		public static bool IsOpen {
			get { return DebugConsole.Instance._isOpen; }
			set { DebugConsole.Instance._isOpen = value; }
		}
		
		/// <summary>
		/// Static instance of the console.
		///
		/// When you want to access the console without a direct
		/// reference (which you do in mose cases), use DebugConsole.Instance and the required
		/// GameObject initialization will be done for you.
		/// </summary>
		static DebugConsole Instance {
			get {
				if (_instance == null) {
					#if UNITY_6000_0_OR_NEWER
					_instance = Object.FindFirstObjectByType<DebugConsole>();
					#else
					_instance = FindObjectOfType(typeof(DebugConsole)) as DebugConsole;
					#endif
					
					if (_instance != null) {
						return _instance;
					}
					
					GameObject console = new GameObject("__Debug Console__");
                    if (Application.isPlaying) {
                        GameObject.DontDestroyOnLoad(console);
                    }
					_instance = console.AddComponent<DebugConsole>();
				}
				
				return _instance;
			}
		}

		private static IDebugConsoleService debugConsoleService;
		public static void SetDebugConsoleService(IDebugConsoleService debugConsoleService) {
            UnHookDebugConsoleLogging();
			DebugConsole.debugConsoleService = debugConsoleService;
            HookDebugConsoleLogging();
		}

        private static void UnHookDebugConsoleLogging() {
            if(DebugConsole.debugConsoleService != null) {
                DebugConsole.debugConsoleService.MessageLogged -= DebugConsole.LogHook;
            }
        }

        private static void HookDebugConsoleLogging() {
            if(DebugConsole.debugConsoleService != null) {
                DebugConsole.debugConsoleService.MessageLogged += LogHook;
            }
        }

        static void LogHook (string message, string category) {
            DebugConsole.Instance.LogMessage(Message.Log(category + message));
        }

		/// <summary>
		/// Key to press to toggle the visibility of the console.
		/// </summary>
		public static KeyCode toggleKey = KeyCode.BackQuote;
		static DebugConsole _instance;
		Dictionary<string, DebugCommandData> _cmdTable = new Dictionary<string, DebugCommandData>();
		string _inputString = string.Empty;
		Rect _windowRect;
		#if MOBILE
		Rect _fakeWindowRect;
		Rect _fakeDragRect;
		bool dragging = false;
		GUIStyle windowOnStyle;
		GUIStyle windowStyle;
		Vector2 _menuScrollPos = Vector2.zero;
		#if UNITY_EDITOR
		Vector2 prevMousePos;
		#endif
		#endif
		
		Vector2 _logScrollPos = Vector2.zero;
		Vector2 _rawLogScrollPos = Vector2.zero;
		Vector3 _guiScale = Vector3.one;
		Matrix4x4 restoreMatrix = Matrix4x4.identity;
		bool _scaled = false;
		bool _isOpen;
		bool justToggledOpen;
		bool _disable = false;
		StringBuilder _displayString = new StringBuilder();
		FPSCounter fps;
		bool dirty;

		// a log to the console can use a category -- categories are subscribed to (added to the list) and this helps keep the log spam down
		private List<string> subscribedLogCategories = new List<string>();
		public static void SubscribeToLogs( string category, bool subscribe ) {
			category = category.ToLower ();

			if (subscribe) {
				if (!DebugConsole.Instance.subscribedLogCategories.Contains (category)) {
					DebugConsole.Instance.subscribedLogCategories.Add (category);
				}
			} else {
				if (DebugConsole.Instance.subscribedLogCategories.Contains (category)) {
					DebugConsole.Instance.subscribedLogCategories.Remove (category);
				}
			}
		}

		#region GUI position values
		// Make these values public if you want to adjust layout of console window
		const float defaultConsoleHeight = 450;
		
		Rect scrollRect = new Rect(10, 20, defaultConsoleHeight - 88, 362);
		Rect inputRect = new Rect(10, defaultConsoleHeight - 62, 228, 24);
		Rect enterRect = new Rect(240, defaultConsoleHeight - 62, 50, 24);
		Rect toolbarRect = new Rect(16, defaultConsoleHeight - 34, 266, 25);
		Rect messageLine = new Rect(4, 0, 264, 20);
		int lineOffset = -4;
		string[] tabs = new string[] { "Log", "Copy Log", "Menu"};
		
		// Keep these private, their values are generated automatically
		Rect innerRect = new Rect(0, 0, 0, 0);
		int innerHeight = 0;
		int toolbarIndex = 0;
		GUIContent guiContent = new GUIContent();
		GUI.WindowFunction[] windowMethods;
		GUIStyle labelStyle;
		#endregion
		
		/// <summary>
		/// This Enum holds the message types used to easily control the formatting and display of a message.
		/// </summary>
		public enum MessageType {
			NORMAL,
			WARNING,
			ERROR,
			SYSTEM,
			INPUT,
			OUTPUT
		}
		
		/// <summary>
		/// Represents a single message, with formatting options.
		/// </summary>
		struct Message {
			string text;
			string formatted;
			MessageType type;
			
			public Color color { get; private set; }
			
			public static Color defaultColor = Color.white;
			public static Color warningColor = Color.yellow;
			public static Color errorColor = Color.red;
			public static Color systemColor = Color.green;
			public static Color inputColor = Color.green;
			public static Color outputColor = Color.cyan;
			
			public Message(object messageObject) : this(messageObject, MessageType.NORMAL, Message.defaultColor) {
			}
			
			public Message(object messageObject, Color displayColor) : this(messageObject, MessageType.NORMAL, displayColor) {
			}
			
			public Message(object messageObject, MessageType messageType) : this(messageObject, messageType, Message.defaultColor) {
				switch (messageType) {
				case MessageType.ERROR:
					color = errorColor;
					break;
				case MessageType.SYSTEM:
					color = systemColor;
					break;
				case MessageType.WARNING:
					color = warningColor;
					break;
				case MessageType.OUTPUT:
					color = outputColor;
					break;
				case MessageType.INPUT:
					color = inputColor;
					break;
				}
			}
			
			public Message(object messageObject, MessageType messageType, Color displayColor) {
				this.text = messageObject == null ? "<null>" : messageObject.ToString();
				
				this.formatted = string.Empty;
				this.type = messageType;
				this.color = displayColor;
			}
			
			public static Message Log(object message) {
				return new Message(message, MessageType.NORMAL, defaultColor);
			}
			
			public static Message System(object message) {
				return new Message(message, MessageType.SYSTEM, systemColor);
			}
			
			public static Message Warning(object message) {
				return new Message(message, MessageType.WARNING, warningColor);
			}
			
			public static Message Error(object message) {
				return new Message(message, MessageType.ERROR, errorColor);
			}
			
			public static Message Output(object message) {
				return new Message(message, MessageType.OUTPUT, outputColor);
			}
			
			public static Message Input(object message) {
				return new Message(message, MessageType.INPUT, inputColor);
			}
			
			public override string ToString() {
				switch (type) {
				case MessageType.ERROR:
					return string.Format("[{0}] {1}", type, text);
				case MessageType.WARNING:
					return string.Format("[{0}] {1}", type, text);
				default:
					return ToGUIString();
				}
			}
			
			public string ToGUIString() {
				if (!string.IsNullOrEmpty(formatted)) {
					return formatted;
				}
				
				switch (type) {
				case MessageType.INPUT:
					formatted = string.Format(">>> {0}", text);
					break;
				case MessageType.OUTPUT:
					var lines = text.Trim('\n').Split('\n');
					var output = new StringBuilder();
					
					foreach (var line in lines) {
						output.AppendFormat("= {0}\n", line);
					}
					
					formatted = output.ToString();
					break;
				case MessageType.SYSTEM:
					formatted = string.Format("# {0}", text);
					break;
				case MessageType.WARNING:
					formatted = string.Format("* {0}", text);
					break;
				case MessageType.ERROR:
					formatted = string.Format("** {0}", text);
					break;
				default:
					formatted = text;
					break;
				}
				
				return formatted;
			}
		}
		
		class History {
			List<string> history = new List<string>();
			int index = 0;
			
			public void Add(string item) {
				history.Add(item);
				index = 0;
			}
			
			string current;
			
			public string Fetch(string current, bool next) {
				if (index == 0) {
					this.current = current;
				}
				
				if (history.Count == 0) {
					return current;
				}
				
				index += next ? -1 : 1;
				
				if (history.Count + index < 0 || history.Count + index > history.Count - 1) {
					index = 0;
					return this.current;
				}
				
				var result = history[history.Count + index];
				
				return result;
			}
		}
		
		List<Message> _messages = new List<Message>();
		History _history = new History();

		//TelnetServer.Server _telnetServer;

		void Awake ()
		{
			if (_instance != null && _instance != this) {
				DestroyImmediate (this, true);
				return;
			}
			
			_instance = this;

			/*try {
				_telnetServer = new TelnetServer.Server (TelnetCommandHandler, 2323, TelnetConsoleOutputHandler);
			} catch (System.Exception e) {
				global::UnityEngine.Debug.Log("Unable to start the telnet server.");
			}
			*/			
		}

		/*
		bool TelnetCommandHandler (object sender, string input, out string output)
		{
			output = EvalInputString(input);
			return true;
		}

		void TelnetConsoleOutputHandler (object sender, string output)
		{
			Log(output);
		} 
		*/
		void OnEnable() {

			//dynamically adjust the screen height.
			float height = defaultConsoleHeight;
			// iphone4 bug fix: iphone4 Screen.height. 
			if(height > ((float)Screen.height * 0.5f) || ((Screen.width>Screen.height) && (height > ((float)Screen.width * 0.5f))))
			{				
				height = (float)Screen.height * 0.4f;
			}
			
			#if MOBILE
			this.useGUILayout = false;
			_windowRect = new Rect(5.0f, 5.0f, 300.0f, height);
			_fakeWindowRect = new Rect(0.0f, 0.0f, _windowRect.width, _windowRect.height);
			_fakeDragRect = new Rect(0.0f, 0.0f, _windowRect.width - 32, 24);
			#else
			_windowRect = new Rect(30.0f, 30.0f, 300.0f, height);
			#endif
			
			scrollRect.height = height - 88;
			inputRect.y = height - 62;
			enterRect.y = inputRect.y;
			toolbarRect.y = height - 34;
			
			var scale = Screen.dpi / 160.0f;
			if (scale != 0.0f && scale >= 1.1f) {
				_scaled = true;

				// added by Joe Lavoine: On some devices the height * scale was actually too big so the console was unusable
				float maxHeight = Screen.height * MAX_SCALE;
				if (scale * height >= maxHeight) {
					scale = maxHeight / height;
				}

				_guiScale.Set(scale, scale, scale);
			}
			
			windowMethods = new GUI.WindowFunction[] { LogWindow, CopyLogWindow, MenuWindow };
			
			fps = new FPSCounter();
			StartCoroutine(fps.Update());

			Message.defaultColor = defaultColor;
			Message.warningColor = warningColor;
			Message.errorColor = errorColor;
			Message.systemColor = systemColor;
			Message.inputColor = inputColor;
			Message.outputColor = outputColor;
			
			LogMessage(Message.System(" type '/?' for available commands. H:"+Screen.height+" W:"+Screen.width));
			LogMessage(Message.Log(""));
			
			this.RegisterCommandCallback("close", CMDClose);
			this.RegisterCommandCallback("exit", CMDExit);
			this.RegisterCommandCallback("clear", CMDClear);
			this.RegisterCommandCallback("sys", CMDSystemInfo);
			this.RegisterCommandCallback("/?", CMDHelp);
		}
		
		[Conditional("DEBUG_CONSOLE"),
		 Conditional("UNITY_EDITOR"),
		 Conditional("DEVELOPMENT_BUILD")]
		void OnGUI() {
			var evt = Event.current;
			
			if (_scaled) {
				restoreMatrix = GUI.matrix;
				
				GUI.matrix = GUI.matrix * Matrix4x4.Scale(_guiScale);
			}
			
			while (_messages.Count > maxLinesForDisplay) {
				_messages.RemoveAt(0);
			}
			#if (!MOBILE && QADEBUG) || UNITY_EDITOR
			// Toggle key shows the console in non-iOS dev builds
			if (evt.keyCode == toggleKey && evt.type == EventType.KeyUp && !_disable)
				_isOpen = !_isOpen;
			#endif
			#if MOBILE
			if (Input.touchCount == 1) {
				var touch = Input.GetTouch(0);
				#if QADEBUG
				// Triple Tap shows/hides the console in iOS/Android dev builds.
				if (evt.type == EventType.Repaint && (touch.phase == TouchPhase.Canceled || touch.phase == TouchPhase.Ended) && touch.tapCount == 3 && !_disable) {
					_isOpen = !_isOpen;
				}
				#endif
				if (_isOpen) {
					var pos = touch.position;
					pos.y = Screen.height - pos.y;
					
					if (dragging && (touch.phase == TouchPhase.Canceled || touch.phase == TouchPhase.Ended)) {
						dragging = false;
					}
					else if (!dragging && (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Stationary)) {
						var dragRect = _fakeDragRect;
						
						dragRect.x = _windowRect.x * _guiScale.x;
						dragRect.y = _windowRect.y * _guiScale.y;
						dragRect.width *= _guiScale.x;
						dragRect.height *= _guiScale.y;
						
						// check to see if the touch is inside the dragRect.
						if (dragRect.Contains(pos)) {
							dragging = true;
						}
					}
					
					if (dragging && evt.type == EventType.Repaint) {
						#if UNITY_ANDROID
						var delta = touch.deltaPosition * 2.0f;
						#elif UNITY_IOS
						var delta = touch.deltaPosition;
						delta.x /= _guiScale.x;
						delta.y /= _guiScale.y;
						#endif
						delta.y = -delta.y;
						
						_windowRect.center += delta;
					}
					else {
						var tapRect = scrollRect;
						tapRect.x += _windowRect.x * _guiScale.x;
						tapRect.y += _windowRect.y * _guiScale.y;
						tapRect.width -= 32;
						tapRect.width *= _guiScale.x;
						tapRect.height *= _guiScale.y;
						
						if (tapRect.Contains(pos)) {
							var scrollY = (tapRect.center.y - pos.y) / _guiScale.y;
							
							switch (toolbarIndex) {
							case 0:
								_logScrollPos.y -= scrollY;
								break;
							case 1:
								_rawLogScrollPos.y -= scrollY;
								break;
							case 2:
								_menuScrollPos.y -= scrollY;
								break;
							}
						}
					}
				}
			}
			else if (dragging && Input.touchCount == 0) {
				dragging = false;
			}
			#endif
			if (!_isOpen) {
				return;
			}
			
			labelStyle = GUI.skin.label;
			
			innerRect.width = messageLine.width;
			#if !MOBILE
			_windowRect = GUI.Window(-1111, _windowRect, windowMethods[toolbarIndex], string.Format("Debug Console v{0}\tfps: {1:00.0}", VERSION, fps.current));
			GUI.BringWindowToFront(-1111);
			#else
			if (windowStyle == null) {
				windowStyle = new GUIStyle(GUI.skin.window);
				windowOnStyle = new GUIStyle(GUI.skin.window);
				windowOnStyle.normal.background = GUI.skin.window.onNormal.background;
			}
			
			GUI.BeginGroup(_windowRect);
			#if UNITY_EDITOR
			if (GUI.RepeatButton(_fakeDragRect, string.Empty, GUIStyle.none)) {
				Vector2 delta = (Vector2) Input.mousePosition - prevMousePos;
				delta.y = -delta.y;
				
				_windowRect.center += delta;
				dragging = true;
			}
			
			if (evt.type == EventType.Repaint) {
				prevMousePos = Input.mousePosition;
			}
			#endif
			GUI.Box(_fakeWindowRect, string.Format("Debug Console v{0}\tfps: {1:00.0}", VERSION, fps.current), dragging ? windowOnStyle : windowStyle);
			windowMethods[toolbarIndex](0);
			GUI.EndGroup();
			#endif
			
			if (GUI.GetNameOfFocusedControl() == ENTRYFIELD) {
				if (evt.isKey && evt.type == EventType.KeyUp) {
					if (evt.keyCode == KeyCode.Return) {
						EvalInputString(_inputString);
						_inputString = string.Empty;
					}
					else if (evt.keyCode == KeyCode.UpArrow) {
						_inputString = _history.Fetch(_inputString, true);
					}
					else if (evt.keyCode == KeyCode.DownArrow) {
						_inputString = _history.Fetch(_inputString, false);
					}
				}
			} else if(justToggledOpen) {
				justToggledOpen = false;
				GUI.FocusControl(ENTRYFIELD);
			}
			
			if (_scaled) {
				GUI.matrix = restoreMatrix;
			}
			
			if (dirty && evt.type == EventType.Repaint) {
				_logScrollPos.y = 50000.0f;
				_rawLogScrollPos.y = 50000.0f;
				
				BuildDisplayString();
				dirty = false;
			}
		}
		
		void OnDestroy() {
			StopAllCoroutines();
		}
		#region StaticAccessors
		
		/// <summary>
		/// Prints a message string to the console.
		/// </summary>
		/// <param name="message">Message to print.</param>
		[System.Obsolete("use IDebugConsoleService interface instead of static class")]
		public static object Log(object message) {
			DebugConsole.Instance.LogMessage(Message.Log(message));
			
			return message;
		}

		[System.Obsolete("use IDebugConsoleService interface instead of static class")]
		public static object Log(object message, string category) {
			return Log (message, category, Color.white);
		}
		[System.Obsolete("use IDebugConsoleService interface instead of static class")]
		public static object Log(object message, string category, Color displayColor) {
			category = category.ToLower ();

			if (DebugConsole.Instance.subscribedLogCategories.Contains (category) || string.IsNullOrEmpty(category)) {
				DebugConsole.Instance.LogMessage(new Message(message, displayColor));
			}

			return message;
		}

		[System.Obsolete("use IDebugConsoleService interface instead of static class")]
		public static object LogFormat(string format, params object[] args) {
			return Log(string.Format(format, args));
		}
		
		/// <summary>
		/// Prints a message string to the console.
		/// </summary>
		/// <param name="message">Message to print.</param>
		/// <param name="messageType">The MessageType of the message. Used to provide
		/// formatting in order to distinguish between message types.</param>
		[System.Obsolete("use IDebugConsoleService interface instead of static class")]
		public static object Log(object message, MessageType messageType) {
			DebugConsole.Instance.LogMessage(new Message(message, messageType));
			
			return message;
		}
		
		/// <summary>
		/// Prints a message string to the console.
		/// </summary>
		/// <param name="message">Message to print.</param>
		/// <param name="displayColor">The text color to use when displaying the message.</param>
		[System.Obsolete("use IDebugConsoleService interface instead of static class")]
		public static object Log(object message, Color displayColor) {
			DebugConsole.Instance.LogMessage(new Message(message, displayColor));
			
			return message;
		}
		
		/// <summary>
		/// Prints a message string to the console.
		/// </summary>
		/// <param name="message">Messate to print.</param>
		/// <param name="messageType">The MessageType of the message. Used to provide
		/// formatting in order to distinguish between message types.</param>
		/// <param name="displayColor">The color to use when displaying the message.</param>
		/// <param name="useCustomColor">Flag indicating if the displayColor value should be used or
		/// if the default color for the message type should be used instead.</param>
		[System.Obsolete("use IDebugConsoleService interface instead of static class")]
		public static object Log(object message, MessageType messageType, Color displayColor) {
			DebugConsole.Instance.LogMessage(new Message(message, messageType, displayColor));
			
			return message;
		}
		
		/// <summary>
		/// Prints a message string to the console using the "Warning" message type formatting.
		/// </summary>
		/// <param name="message">Message to print.</param>
		[System.Obsolete("use IDebugConsoleService interface instead of static class")]
		public static object LogWarning(object message) {
			DebugConsole.Instance.LogMessage(Message.Warning(message));
			
			return message;
		}
		
		/// <summary>
		/// Prints a message string to the console using the "Error" message type formatting.
		/// </summary>
		/// <param name="message">Message to print.</param>
		[System.Obsolete("use IDebugConsoleService interface instead of static class")]
		public static object LogError(object message) {
			DebugConsole.Instance.LogMessage(Message.Error(message));
			
			return message;
		}
		
		/// <summary>
		/// Clears all console output.
		/// </summary>
		[Conditional("DEBUG_CONSOLE"),
		 Conditional("UNITY_EDITOR"),
		 Conditional("DEVELOPMENT_BUILD")]
		[System.Obsolete("use IDebugConsoleService interface instead of static class")]
		public static void Clear() {
			DebugConsole.Instance.ClearLog();
		}
		
		/// <summary>
		/// Execute a console command directly from code.
		/// </summary>
		/// <param name="commandString">The command line you want to execute. For example: "sys"</param>
		[Conditional("DEBUG_CONSOLE"),
		 Conditional("UNITY_EDITOR"),
		 Conditional("DEVELOPMENT_BUILD")]
		[System.Obsolete("use IDebugConsoleService interface instead of static class")]
		public static void Execute(string commandString) {
			DebugConsole.Instance.EvalInputString(commandString);
		}
		
		/// <summary>
		/// Registers a debug command that is "fired" when the specified command string is entered.
		/// </summary>
		/// <param name="commandString">The string that represents the command. For example: "FOV"</param>
		/// <param name="commandCallback">The method/function to call with the commandString is entered.
		/// For example: "SetFOV"</param>
		[Conditional("DEBUG_CONSOLE"),
		 Conditional("UNITY_EDITOR"),
		 Conditional("DEVELOPMENT_BUILD")]
		[System.Obsolete("use IDebugConsoleService interface instead of static class")]
		public static void RegisterCommand(string commandString, DebugCommand commandCallback) {
			DebugConsole.Instance.RegisterCommandCallback(commandString, commandCallback, commandString);
		}
		
		/// <summary>
		/// Registers a debug command that is "fired" when the specified command string is entered.
		/// </summary>
		/// <param name="commandString">The string that represents the command. For example: "FOV"</param>
		/// <param name="commandCallback">The method/function to call with the commandString is entered.
		/// For example: "SetFOV"</param>
		[Conditional("DEBUG_CONSOLE"),
		 Conditional("UNITY_EDITOR"),
		 Conditional("DEVELOPMENT_BUILD")]
		[System.Obsolete("use IDebugConsoleService interface instead of static class")]
		public static void RegisterCommand(string commandString, DebugCommand commandCallback, string helpString) {
			DebugConsole.Instance.RegisterCommandCallback(commandString, commandCallback, helpString);
		}
		
		/// <summary>
		/// Removes a previously-registered debug command.
		/// </summary>
		/// <param name="commandString">The string that represents the command.</param>
		[Conditional("DEBUG_CONSOLE"),
		 Conditional("UNITY_EDITOR"),
		 Conditional("DEVELOPMENT_BUILD")]
		[System.Obsolete("use IDebugConsoleService interface instead of static class")]
		public static void UnRegisterCommand(string commandString) {
			DebugConsole.Instance.UnRegisterCommandCallback(commandString);
		}
		#endregion
		#region Console commands
		
		//==== Built-in example DebugCommand handlers ====
		object CMDClose(params string[] args) {
			_isOpen = false;
			
			return "closed";
		}
		
		object CMDExit(params string[] args) {
			_disable = true;
			_isOpen = false;
			return "exit";
		}
		
		object CMDClear(params string[] args) {
			this.ClearLog();
			
			return "clear";
		}
		
		object CMDHelp(params string[] args) {
			var output = new StringBuilder();
			
			output.AppendLine(":: Command List ::");
			
			foreach (string key in _cmdTable.Keys) {
				output.AppendLine(_cmdTable[key].help);
			}

			if(DebugConsole.debugConsoleService != null) {
				IList<ICommandInfo> debugCommands = DebugConsole.debugConsoleService.GetDebugCommands();
				foreach(ICommandInfo debugCommand in debugCommands) {
					output.AppendLine(debugCommand.Command);
				}
			}
			
			output.AppendLine(" ");
			
			return output.ToString();
		}
		
		object CMDSystemInfo(params string[] args) {
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
			#if UNITY_WEBPLAYER
			info.AppendLine();
			info.AppendFormat("URL: {0}\n", Application.absoluteURL);
			info.AppendFormat("srcValue: {0}\n", Application.srcValue);
			info.AppendFormat("security URL: {0}\n", Application.webSecurityHostUrl);
			#endif
			#if MOBILE
			info.AppendLine();
			info.AppendFormat("Net Reachability: {0}\n", Application.internetReachability);
			info.AppendFormat("Multitouch: {0}\n", Input.multiTouchEnabled);
			#endif
			#if UNITY_EDITOR
			info.AppendLine();
			info.AppendFormat("editorApp: {0}\n", UnityEditor.EditorApplication.applicationPath);
			info.AppendFormat("editorAppContents: {0}\n", UnityEditor.EditorApplication.applicationContentsPath);
			info.AppendFormat("scene: {0}\n", UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().name );
			#endif
			info.AppendLine();
			/*var devices = WebCamTexture.devices;
    if (devices.Length > 0) {
      info.AppendLine("Cameras: ");

      foreach (var device in devices) {
        info.AppendFormat("  {0} front:{1}\n", device.name, device.isFrontFacing);
      }
    }*/
			
			return info.ToString();
		}
		
		
		#endregion
		#region GUI Window Methods
		
		void DrawBottomControls() {
			GUI.SetNextControlName(ENTRYFIELD);
			_inputString = GUI.TextField(inputRect, _inputString);
			
			if (GUI.Button(enterRect, "Enter")) {
				EvalInputString(_inputString);
				_inputString = string.Empty;
			}
			
			var index = GUI.Toolbar(toolbarRect, toolbarIndex, tabs);
			
			if (index != toolbarIndex) {
				toolbarIndex = index;
			}
			#if !MOBILE
			GUI.DragWindow();
			#endif
		}
		
		void LogWindow(int windowID) {
			GUI.Box(scrollRect, string.Empty);
			
			innerRect.height = innerHeight < scrollRect.height ? scrollRect.height : innerHeight;
			
			_logScrollPos = GUI.BeginScrollView(scrollRect, _logScrollPos, innerRect, false, true);
			
			if (_messages != null || _messages.Count > 0) {
				Color oldColor = GUI.contentColor;
				
				messageLine.y = 0;
				
				foreach (Message m in _messages) {
					GUI.contentColor = m.color;
					
					guiContent.text = m.ToGUIString();
					
					messageLine.height = labelStyle.CalcHeight(guiContent, messageLine.width);
					
					GUI.Label(messageLine, guiContent);
					
					messageLine.y += (messageLine.height + lineOffset);
					
					innerHeight = messageLine.y > scrollRect.height ? (int) messageLine.y : (int) scrollRect.height;
				}
				GUI.contentColor = oldColor;
			}
			
			GUI.EndScrollView();
			
			DrawBottomControls();
		}
		
		string GetDisplayString() {
			if (_messages == null) {
				return string.Empty;
			}
			
			return _displayString.ToString();
		}
		
		void BuildDisplayString() {
			_displayString.Length = 0;
			
			foreach (Message m in _messages) {
				_displayString.AppendLine(m.ToString());
			}
		}
		
		void CopyLogWindow(int windowID) {
			
			guiContent.text = GetDisplayString();
			
			var calcHeight = GUI.skin.textArea.CalcHeight(guiContent, messageLine.width);
			
			innerRect.height = calcHeight < scrollRect.height ? scrollRect.height : calcHeight;
			
			_rawLogScrollPos = GUI.BeginScrollView(scrollRect, _rawLogScrollPos, innerRect, false, true);
			
			GUI.TextArea(innerRect, guiContent.text);
			
			GUI.EndScrollView();
			
			DrawBottomControls();
		}
		
		void MenuWindow(int windowID) {
			GUI.Box(scrollRect, string.Empty);
			
			innerRect.height = innerHeight < scrollRect.height ? scrollRect.height : innerHeight;
			
			//_menuScrollPos = GUI.BeginScrollView(scrollRect, _watchVarsScrollPos, innerRect, false, true);
			
			//GUI.EndScrollView();
			
			DrawBottomControls();
		}
		#endregion
		#region InternalFunctionality
		[Conditional("DEBUG_CONSOLE"),
		 Conditional("UNITY_EDITOR"),
		 Conditional("DEVELOPMENT_BUILD")]
		void LogMessage(Message msg) {
			_messages.Add(msg);
			dirty = true;
		}
		
		//--- Local version. Use the static version above instead.
		void ClearLog() {
			_messages.Clear();
		}
		
		//--- Local version. Use the static version above instead.
		void RegisterCommandCallback(string commandString, DebugCommand commandCallback) {
			#if !UNITY_FLASH
			commandString = commandString.ToLower();
			_cmdTable[commandString] = new DebugCommandData(commandString, commandString, new DebugCommand(commandCallback));
			#endif
		}
		
		//--- Local version. Use the static version above instead.
		void RegisterCommandCallback(string commandString, DebugCommand commandCallback, string helpString) {
			#if !UNITY_FLASH
			commandString = commandString.ToLower();
			_cmdTable[commandString] = new DebugCommandData(commandString, helpString, new DebugCommand(commandCallback));
			#endif
		}
		
		//--- Local version. Use the static version above instead.
		void UnRegisterCommandCallback(string commandString) {
			commandString = commandString.ToLower();
			_cmdTable.Remove(commandString);
		}
		
		string EvalInputString(string inputString) {
			inputString = inputString.Trim();
			
			if (string.IsNullOrEmpty(inputString)) {
				Message msg = Message.Input(string.Empty);
				LogMessage(msg);
				return msg.ToString();
			}
			
			_history.Add(inputString);
			LogMessage(Message.Input(inputString));
			
			var input = new List<string>(inputString.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries));
			
			var cmd = input[0].ToLower();

			if (_cmdTable.ContainsKey(cmd)) {
				object retval = _cmdTable[cmd].callback(input.ToArray());
				Message msg = new Message(retval, MessageType.OUTPUT);
				LogMessage(msg);
				return msg.ToString();
			}

			if(DebugConsole.debugConsoleService != null) {
				IList<ICommandInfo> debugCommands = DebugConsole.debugConsoleService.GetDebugCommands();
				foreach(ICommandInfo debugCommand in debugCommands) {
					if(debugCommand.Command.ToLower() == cmd) {
						DebugConsole.debugConsoleService.ExecDebugCommand(debugCommand.Command, input.ToArray(), s => LogMessage(new Message(s, MessageType.OUTPUT)) );
						return "executing..";
					}
				}
			}

			Message failmsg = Message.Output(string.Format("*** Unknown Command: {0} ***", cmd));
			LogMessage(failmsg);
			return failmsg.ToString();
		
		}
		#endregion
	}

	#else
	public static class DebugConsole {
		public static bool IsOpen;
		public static KeyCode toggleKey;
		public delegate object DebugCommand(params string[] args);
		
		[System.Obsolete("use IDebugConsoleService interface instead of static class")]
		public static object Log(object message) {
			return message;
		}
		
		[System.Obsolete("use IDebugConsoleService interface instead of static class")]
		public static object LogFormat(string format, params object[] args) {
			return string.Format(format, args);
		}
		[System.Obsolete("use IDebugConsoleService interface instead of static class")]
		public static object LogWarning(object message) {
			return message;
		}
		[System.Obsolete("use IDebugConsoleService interface instead of static class")]
		public static object LogError(object message) {
			return message;
		}
		[System.Obsolete("use IDebugConsoleService interface instead of static class")]
		public static object Log(object message, object messageType) {
			return message;
		}
		[System.Obsolete("use IDebugConsoleService interface instead of static class")]
		public static object Log(object message, Color displayColor) {
			return message;
		}
		[System.Obsolete("use IDebugConsoleService interface instead of static class")]
		public static object Log(object message, object messageType, Color displayColor) {
			return message;
		}
		[System.Obsolete("use IDebugConsoleService interface instead of static class")]
		public static void Clear() {
		}
		[System.Obsolete("use IDebugConsoleService interface instead of static class")]
		public static void RegisterCommand(string commandString, DebugCommand commandCallback) {
		}
		[System.Obsolete("use IDebugConsoleService interface instead of static class")]
		public static void UnRegisterCommand(string commandString) {
		}
		[System.Obsolete("use IDebugConsoleService interface instead of static class")]
		public static void RegisterWatchVar(object watchVar) {
		}
		[System.Obsolete("use IDebugConsoleService interface instead of static class")]
		public static void UnRegisterWatchVar(string name) {
		}
		[System.Obsolete("use IDebugConsoleService interface instead of static class")]
		public static void SubscribeToLogs(string category, bool subscribe) {}
	}
	#endif
	
	public class FPSCounter {
		public float current = 0.0f;
		public float updateInterval = 0.5f;
		// FPS accumulated over the interval
		float accum = 0;
		// Frames drawn over the interval
		int frames = 1;
		// Left time for current interval
		float timeleft;
		float delta;
		
		public FPSCounter() {
			timeleft = updateInterval;
		}
		
		public IEnumerator Update() {
			// skip the first frame where everything is initializing.
			yield return null;
			
			while (true) {
				delta = Time.deltaTime;
				
				timeleft -= delta;
				accum += Time.timeScale / delta;
				++frames;
				
				// Interval ended - update GUI text and start new interval
				if (timeleft <= 0.0f) {
					current = accum / frames;
					timeleft = updateInterval;
					accum = 0.0f;
					frames = 0;
				}
				
				yield return null;
			}
		}
	}
	
	namespace UnityEngine {
		public static class Assertion {
			[Conditional("DEBUG_LEVEL_LOG"),
			 Conditional("UNITY_EDITOR"),
			 Conditional("DEVELOPMENT_BUILD")]
			public static void Assert(bool condition) {
				Assert(condition, string.Empty, true);
			}
			
			[Conditional("DEBUG_LEVEL_LOG"),
			 Conditional("UNITY_EDITOR"),
			 Conditional("DEVELOPMENT_BUILD")]
			public static void Assert(bool condition, string assertString) {
				Assert(condition, assertString, false);
			}
			
			[Conditional("DEBUG_LEVEL_LOG"),
			 Conditional("UNITY_EDITOR"),
			 Conditional("DEVELOPMENT_BUILD")]
			public static void Assert(bool condition, string assertString, bool pauseOnFail) {
				if (condition) {
					return;
				}
				
				global::UnityEngine.Debug.LogError(string.Format("Assertion failed!\n{0}", assertString));
				
				if (pauseOnFail) {
					global::UnityEngine.Debug.Break();
				}
			}
		}
	}
	
} // namespace

namespace UnityMock {
	public static class Debug {
		// Methods
		[Conditional("DEBUG_LEVEL_LOG"),
		 Conditional("UNITY_EDITOR"),
		 Conditional("DEVELOPMENT_BUILD")]
		public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration) {
			UnityEngine.Debug.DrawLine(start, end, color, duration);
		}
		
		[Conditional("DEBUG_LEVEL_LOG"),
		 Conditional("UNITY_EDITOR"),
		 Conditional("DEVELOPMENT_BUILD")]
		public static void DrawLine(Vector3 start, Vector3 end, Color color) {
			UnityEngine.Debug.DrawLine(start, end, color);
		}
		
		[Conditional("DEBUG_LEVEL_LOG"),
		 Conditional("UNITY_EDITOR"),
		 Conditional("DEVELOPMENT_BUILD")]
		public static void DrawLine(Vector3 start, Vector3 end) {
			UnityEngine.Debug.DrawLine(start, end);
		}
		
		[Conditional("DEBUG_LEVEL_LOG"),
		 Conditional("UNITY_EDITOR"),
		 Conditional("DEVELOPMENT_BUILD")]
		public static void DrawRay(Vector3 start, Vector3 dir, Color color) {
			UnityEngine.Debug.DrawRay(start, dir, color);
		}
		
		[Conditional("DEBUG_LEVEL_LOG"),
		 Conditional("UNITY_EDITOR"),
		 Conditional("DEVELOPMENT_BUILD")]
		public static void DrawRay(Vector3 start, Vector3 dir) {
			UnityEngine.Debug.DrawRay(start, dir);
		}
		
		[Conditional("DEBUG_LEVEL_LOG"),
		 Conditional("UNITY_EDITOR"),
		 Conditional("DEVELOPMENT_BUILD")]
		public static void DrawRay(Vector3 start, Vector3 dir, Color color, float duration) {
			UnityEngine.Debug.DrawRay(start, dir, color);
		}
		
		[Conditional("DEBUG_LEVEL_LOG"),
		 Conditional("DEBUG_LEVEL_WARN"),
		 Conditional("DEBUG_LEVEL_ERROR"),
		 Conditional("UNITY_EDITOR"),
		 Conditional("DEVELOPMENT_BUILD")]
		public static void Break() {
			UnityEngine.Debug.Break();
		}
		
		[Conditional("DEBUG_LEVEL_LOG"),
		 Conditional("DEBUG_LEVEL_WARN"),
		 Conditional("DEBUG_LEVEL_ERROR"),
		 Conditional("UNITY_EDITOR"),
		 Conditional("DEVELOPMENT_BUILD")]
		public static void DebugBreak() {
			UnityEngine.Debug.DebugBreak();
		}
		
		[Conditional("DEBUG_LEVEL_LOG"),
		 Conditional("UNITY_EDITOR"),
		 Conditional("DEVELOPMENT_BUILD")]
		public static void Log(object message) {
			UnityEngine.Debug.Log(message);
		}
		
		[Conditional("DEBUG_LEVEL_LOG"),
		 Conditional("UNITY_EDITOR"),
		 Conditional("DEVELOPMENT_BUILD")]
		public static void Log(object message, Object context) {
			UnityEngine.Debug.Log(message, context);
		}
		
		[Conditional("DEBUG_LEVEL_LOG"),
		 Conditional("DEBUG_LEVEL_WARN"),
		 Conditional("DEBUG_LEVEL_ERROR"),
		 Conditional("UNITY_EDITOR"),
		 Conditional("DEVELOPMENT_BUILD")]
		public static void LogError(object message) {
			UnityEngine.Debug.LogError(message);
		}
		
		[Conditional("DEBUG_LEVEL_LOG"),
		 Conditional("DEBUG_LEVEL_WARN"),
		 Conditional("DEBUG_LEVEL_ERROR"),
		 Conditional("UNITY_EDITOR"),
		 Conditional("DEVELOPMENT_BUILD")]
		public static void LogError(object message, Object context) {
			UnityEngine.Debug.LogError(message, context);
		}
		
		[Conditional("DEBUG_LEVEL_LOG"),
		 Conditional("DEBUG_LEVEL_WARN"),
		 Conditional("UNITY_EDITOR"),
		 Conditional("DEVELOPMENT_BUILD")]
		public static void LogWarning(object message) {
			UnityEngine.Debug.LogWarning(message);
		}
		
		[Conditional("DEBUG_LEVEL_LOG"),
		 Conditional("DEBUG_LEVEL_WARN"),
		 Conditional("UNITY_EDITOR"),
		 Conditional("DEVELOPMENT_BUILD")]
		public static void LogWarning(object message, Object context) {
			UnityEngine.Debug.LogWarning(message, context);
		}
		
		// Properties
		public static bool isDebugBuild {
			#if QADEBUG
			get { return true; }
			#else
			get { return false; }
			#endif
		}
	}
}

