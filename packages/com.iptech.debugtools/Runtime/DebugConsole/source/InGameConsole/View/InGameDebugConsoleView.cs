#if !(UNITY_EDITOR || DEVELOPMENT_BUILD || QA_BUILD)
#define CONTAINERDEBUGSERVICE_DISABLED
#endif

#if !CONTAINERDEBUGSERVICE_DISABLED

using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using Object = UnityEngine.Object;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;

namespace IPTech.DebugConsoleService.InGameConsole
{
	public class InGameDebugConsoleView : MonoBehaviour, IInGameDebugConsoleView, IInitializePotentialDragHandler, IEndDragHandler, IDragHandler {

		const int MAX_SCROLLBACK_COUNT = 1000;
		static char[] NEWLINESPLITARRAY = new char[] { '\n' };

        private ScrollbackBuffer scrollBackBuffer;
        private CommandElementsCollection commandElements;

		private float prevConsoleHeight = 500;
		private bool isDragging;
		private IList<string> commandHistory;
		private int commandIterIndex;
		      
        public string Command { get; private set; }

		public InputField commandInputField;
		public Text OutputTextObject;
		public RectTransform sizerHandle;
        public Text currentCategoryText;
		public Button tabButtonTemplate;
        public TabGroupTemplate tabGroupTemplate;

		public UnityEvent ExecuteCommandClicked;
		public UnityEvent WantsUpdatedButtons;
        public UnityEventString RecievedNotification;

		public event Action<string> OnExecuteCommand;
		public event Action OnWantsUpdatedCommands;

		void Awake() {
            InitializeScrollbackBuffer();
            InitializeCommandElementsCollection();
            InitializeCommandHistory();
            InitializeCommandInput();
			HideDefaultItemTemplates();
			MinimizeConsole();
            Object.DontDestroyOnLoad(this.transform.parent.gameObject);
		}

		public void Show(bool value) {
			this.transform.parent.gameObject.SetActive(value);
        }

        private void InitializeScrollbackBuffer() {
            this.scrollBackBuffer = new ScrollbackBuffer(MAX_SCROLLBACK_COUNT);
        }

        private void InitializeCommandElementsCollection() {
            this.commandElements = new CommandElementsCollection();
            this.commandElements.CommandClicked += HandleCommandClicked;
            this.commandElements.CategoryClicked += HandleCategoryClicked;
        }

        private void InitializeCommandHistory() {
            this.commandHistory = new List<string>();
        }

        private void InitializeCommandInput() {
            this.commandInputField.onEndEdit.AddListener(  _ => {
#if ENABLE_LEGACY_INPUT_MANAGER
                if(Input.GetButtonDown("Submit")) {
                    ExecuteCommand();
                }
#else
               if(UnityEngine.InputSystem.Keyboard.current.enterKey.isPressed) {
                    ExecuteCommand();
               }
#endif
            });
        }
        
        private void HandleCommandClicked(string command) {
            this.Command = "\"" + command + "\"";
            this.ExecuteStoredCommand();
        }

        private void HandleCategoryClicked(string category) {
            if(this.currentCategoryText!=null) {
                this.currentCategoryText.text = category;
            }
        }

		private void HideDefaultItemTemplates() {
			if(this.tabButtonTemplate!=null) {
				this.tabButtonTemplate.gameObject.SetActive(false);
			}
            if(this.tabGroupTemplate!=null) {
                this.tabGroupTemplate.gameObject.SetActive(false);
                this.tabGroupTemplate.templatButton.gameObject.SetActive(false);
				this.tabGroupTemplate.templateUIDocument.gameObject.SetActive(false);
            }
		}

		void Update() {
			UpdateCommandInputFieldAutoComplete();
		}

        private void UpdateCommandInputFieldAutoComplete() {
            if(this.commandInputField.isFocused) {
#if ENABLE_LEGACY_INPUT_MANAGER
                if(Input.GetKeyUp(KeyCode.UpArrow)) {
                    prevCommand();
                } else if(Input.GetKeyUp(KeyCode.DownArrow)) {
                    nextCommand();
                }
#else
                if(UnityEngine.InputSystem.Keyboard.current.upArrowKey.wasPressedThisFrame) {
                    prevCommand();
                } else if(UnityEngine.InputSystem.Keyboard.current.downArrowKey.wasPressedThisFrame) {
                    nextCommand();
                }
#endif
            }
        }

		public void ToggleConsole() {
			RectTransform rt = (RectTransform)transform;
			if(rt.sizeDelta.y < 1) {
                RequestUpdateButtons();
				RestoreConsole();
			} else {
				MinimizeConsole();
			}
		}

		public void MinimizeConsole() {
			SetConsoleSize(0, false);
		}

		public void RestoreConsole() {
			SetConsoleSize(this.prevConsoleHeight, false);
		}

		private void SetConsoleSize(float newHeight, bool storePrevious=true) {
			if(newHeight<0) 
				return;

			RectTransform parentRect = (RectTransform)this.transform.parent.transform;
			if(parentRect!=null && parentRect.sizeDelta.y<(newHeight+this.sizerHandle.sizeDelta.y)) {
				return;
			}

			RectTransform rt = (RectTransform)transform;
			Vector2 sizeDelta = rt.sizeDelta;
			sizeDelta.y = newHeight;
			rt.sizeDelta = sizeDelta;
			if(storePrevious) {
				this.prevConsoleHeight = newHeight;
			}
		}

#region IInitializePotentialDragHandler implementation

		public void OnInitializePotentialDrag(PointerEventData eventData) {
			eventData.useDragThreshold = false;
		}

#endregion

#region IDragHandler implementation

		public void OnDrag(PointerEventData eventData) {
			if(!this.MayDrag(eventData)) {
				return;
			}
			this.UpdateDrag(eventData, eventData.pressEventCamera);
		}

#endregion

#region IEndDragHandler implementation

		public void OnEndDrag(PointerEventData eventData) {
			this.isDragging = false;
		}

#endregion

		private bool MayDrag(PointerEventData eventData) {
			return this.isActiveAndEnabled && eventData.button == PointerEventData.InputButton.Left;
		}
           

		private void UpdateDrag(PointerEventData eventData, Camera cam) {
			if(IsSizerHandleValid()) {
            	Vector2 vector;
                if(CalculateRelativePositionInSizerHandleRect(eventData.position, cam, out vector)) {
                    if(!this.isDragging) {
                        if(!IsPointInSizerRect(vector)) {
                            return;
                        }
                        RequestUpdateButtons();
                    }
                    this.isDragging = true;

                    DragTranslateConsoleHeight(vector.y);
                    DragTranslateSizerHandleX(vector.x);
                }
			}
		}

        private bool IsSizerHandleValid() {
            return this.sizerHandle != null && this.sizerHandle.rect.size.x > 0;
        }

        private bool IsPointInSizerRect(Vector2 relativePoint) {
            return this.sizerHandle.rect.Contains(relativePoint);
        }

        private bool CalculateRelativePositionInSizerHandleRect(Vector2 screenSpacePoint, Camera cam, out Vector2 relativePoint) {
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(this.sizerHandle, screenSpacePoint, cam, out relativePoint);
        }

        private void DragTranslateConsoleHeight(float deltaHeight) {
            RectTransform thisRectTransform = (RectTransform)this.transform;

            float newConsoleHeight = thisRectTransform.sizeDelta.y + deltaHeight;
            SetConsoleSize(newConsoleHeight);
        }

        private void DragTranslateSizerHandleX(float deltaX) {
            RectTransform thisRectTransform = (RectTransform)this.transform;

            Vector3 prevLocalPosition = this.sizerHandle.localPosition;
            Vector3 newLocalPosition = this.sizerHandle.localPosition + new Vector3(deltaX, 0, 0);
            this.sizerHandle.localPosition = newLocalPosition;

            Vector3[] parentWorldCorners = new Vector3[4];
            thisRectTransform.GetWorldCorners(parentWorldCorners);

            if(!(this.sizerHandle.position.x > parentWorldCorners[1].x && this.sizerHandle.position.x < parentWorldCorners[2].x)) {
                this.sizerHandle.localPosition = prevLocalPosition;   
            }
        }

		public void ExecuteCommand() {
			this.Command = this.commandInputField.text;
            ExecuteStoredCommand();
        }

        private void ExecuteStoredCommand() {
			AddCommandToHistory(this.Command);
			resetCommandIter();

			this.ExecuteCommandClicked.Invoke();
			OnExecuteCommand?.Invoke(Command);

			this.commandInputField.text = "";
			this.commandInputField.ActivateInputField();
			this.commandInputField.Select();
		}

		public void Output(string message) {
			if(this.OutputTextObject!=null) {
				string[] lines = message.Split(NEWLINESPLITARRAY);
				foreach(string line in lines) {
					Text newText = Instantiate(this.OutputTextObject);
					newText.transform.SetParent(this.OutputTextObject.transform.parent);
					newText.transform.localScale = this.OutputTextObject.transform.localScale;
					newText.text = line;
					this.scrollBackBuffer.AppendScrollBackText(newText);
				}
			}
		}
        
        public void Notify(string message) {
           if(this.RecievedNotification!=null) {
               this.RecievedNotification.Invoke(message);
           } 
        }

		public void RequestUpdateButtons() {
			this.WantsUpdatedButtons.Invoke();
			OnWantsUpdatedCommands?.Invoke();
		}

		public void UpdateButtons(IEnumerable<CommandData> commands) {
			if(this.tabButtonTemplate!=null) {
                this.commandElements.ClearNonExistantCommands(commands.Select(cmd => new KeyValuePair<string,string>(cmd.Category, cmd.Command)));

                if(commands!=null) {
    				foreach(CommandData cmd in commands) {
    					this.commandElements.AddCategoryCommand(cmd, this.tabButtonTemplate, this.tabGroupTemplate);
    				}
                }
			}
		}
  
		private void AddCommandToHistory(string command) {
			if(this.commandHistory.Count>0) {
				if(this.commandHistory[this.commandHistory.Count-1]==command) {
					return;
				}
			}
			this.commandHistory.Add(command);
		}

		private void resetCommandIter() {
			this.commandIterIndex = 0;
		}

		private void prevCommand() {
			int commandIndex = this.commandHistory.Count - (++this.commandIterIndex);
			if(commandIndex>=0) {
				string command = this.commandHistory[commandIndex];
				this.commandInputField.text = command;
			}
		}

		private void nextCommand() {
			int commandIndex = this.commandHistory.Count - (--this.commandIterIndex);
			if(commandIndex<this.commandHistory.Count) {
				string command = this.commandHistory[commandIndex];
				this.commandInputField.text = command;
			}
		}

        public class CommandData {
            public string Category;
            public string Command;
            public string ShortName;
			public Func<VisualElement> visualElementFactory;
		}

        [Serializable]
        public class UnityEventString : UnityEvent<String> {}
	}
}
#endif