#if !(UNITY_EDITOR || DEVELOPMENT_BUILD || QA_BUILD)
#define CONTAINERDEBUGSERVICE_DISABLED
#endif

#if !CONTAINERDEBUGSERVICE_DISABLED

using System;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
using static IPTech.DebugConsoleService.InGameConsole.InGameDebugConsoleView;

namespace IPTech.DebugConsoleService.InGameConsole
{
    public class CommandElementsCollection {

        private IDictionary<string, CategoryButton> createdButtons;

        public event Action<string> CommandClicked;
        public event Action<string> CategoryClicked;

        public CommandElementsCollection() {
            this.createdButtons = new Dictionary<string, CategoryButton>();

        }

        public void ClearNonExistantCommands(IEnumerable<KeyValuePair<string, string>> categoryCommandPairs) {
            string[] keys = null;
            if(this.createdButtons.Count > 0) {
                keys = this.createdButtons.Keys.ToArray();
                foreach(string category in keys) {
                    if(categoryCommandPairs == null || categoryCommandPairs.All(cat => cat.Key != category)) {
                        UnityEngine.Object.Destroy(this.createdButtons[category].categoryGroupObject);
                        UnityEngine.Object.Destroy(this.createdButtons[category].button);
                        this.createdButtons.Remove(category);
                    } else {
                        CategoryButton catButton = this.createdButtons[category];
                        string[] commands = catButton.commandToGameObjects.Keys.ToArray();
                        foreach(string command in commands) {
                            if(categoryCommandPairs.All(kvp => kvp.Value != command)) {
                                UnityEngine.Object.Destroy(catButton.commandToGameObjects[command]);
                                catButton.commandToGameObjects.Remove(command);
                            }
                        }
                    }
                }
            }
        }

        public void AddCategoryCommand(CommandData cmd, Button tabButtonTemplate, TabGroupTemplate tabGroupTemplate) {
            var category = cmd.Category;
            var shortName = cmd.ShortName;
            var command = cmd.Command;

            if(!HasCategory(category)) {
                AddCategory(category, tabButtonTemplate, tabGroupTemplate);
            }

            if(HasCategoryCommand(category, command)) {
                return;
            }

            CategoryButton catButton = this.createdButtons[category];

            if(command.StartsWith("DEBUGPANEL")) {
                UIDocument templateDoc = catButton.categoryGroupObject.templateUIDocument;
                templateDoc = UnityEngine.Object.Instantiate(templateDoc);
                catButton.commandToVisualElement.Add(command, templateDoc);
                templateDoc.gameObject.name = command;
                templateDoc.transform.SetParent(catButton.categoryGroupObject.templateUIDocument.transform.parent);
                templateDoc.transform.localScale = catButton.categoryGroupObject.templateUIDocument.transform.localScale;
                var d = templateDoc.gameObject.AddComponent<TemplateUIDoc>();
                d.visualElement = cmd.visualElementFactory();
                templateDoc.gameObject.SetActive(true);
            } else {
                Button buttonObject = catButton.categoryGroupObject.templatButton;

                Button btn = UnityEngine.Object.Instantiate(buttonObject);
                catButton.commandToGameObjects.Add(command, btn.gameObject);
                btn.gameObject.name = command;
                btn.transform.SetParent(buttonObject.transform.parent);
                btn.transform.localScale = buttonObject.transform.localScale;
                btn.onClick.AddListener(() => {
                    OnCommandButtonClicked(command);
                });
                Text tx = btn.GetComponentInChildren<Text>();
                if(tx != null) {
                    tx.text = shortName;
                }
                btn.gameObject.SetActive(true);
            }
        }

        private bool HasCategory(string category) {
            if(this.createdButtons.Count == 0)
                return false;

            return this.createdButtons.ContainsKey(category);
        }

        private bool HasCategoryCommand(string category, string command) {
            if(this.createdButtons.Count == 0) {
                return false;
            }

            return this.createdButtons.Any(kvp => kvp.Key == category && kvp.Value.ContainsCommand(command));
        }

        private void OnCommandButtonClicked(string command) {
            if(this.CommandClicked!=null) {
                try {
                    this.CommandClicked(command);
                } catch {}
            }
        }

        private void CategoryButtonClicked(string category) {
            foreach(KeyValuePair<string, CategoryButton> kvp in this.createdButtons) {
                kvp.Value.categoryGroupObject.gameObject.SetActive(kvp.Key==category);
            }
            if(this.CategoryClicked!=null) {
                try {
                    this.CategoryClicked(category);
                } catch {}
            }
        }

        private void AddCategory(string category, Button tabButtonTemplate, TabGroupTemplate tabGroupTemplate) {
            Button btn = UnityEngine.Object.Instantiate(tabButtonTemplate);
            btn.transform.SetParent(tabButtonTemplate.transform.parent);
            btn.transform.localScale = tabButtonTemplate.transform.localScale;
            btn.onClick.AddListener(() => CategoryButtonClicked(category));
            Text tx = btn.GetComponentInChildren<Text>();
            if(tx!=null) {
                tx.text = category;
            }

            TabGroupTemplate groupContainer = CreateGroupContainer(category, tabGroupTemplate);
            this.createdButtons.Add(category, new CategoryButton(btn, groupContainer));
            btn.gameObject.SetActive(true);
        }

        private TabGroupTemplate CreateGroupContainer(string category, TabGroupTemplate tabGroupTemplate) {
            var newGroupContainer = UnityEngine.Object.Instantiate<TabGroupTemplate>(tabGroupTemplate);
            newGroupContainer.transform.SetParent(tabGroupTemplate.transform.parent);
            newGroupContainer.transform.localScale = tabGroupTemplate.transform.localScale;
            newGroupContainer.name = "CategoryGroup " + category.ToString();
            return newGroupContainer;
        }

        private class CategoryButton {
            public readonly Button button;
            public readonly TabGroupTemplate categoryGroupObject;
            public IDictionary<string, GameObject> commandToGameObjects;
            public IDictionary<string, UIDocument> commandToVisualElement;

            public CategoryButton(Button button, TabGroupTemplate categoryGroup) {
                this.button = button;
                this.categoryGroupObject = categoryGroup;
                this.commandToGameObjects = new Dictionary<string, GameObject>();
                this.commandToVisualElement = new Dictionary<string, UIDocument>();
            }

            public bool ContainsCommand(string command) {
                return this.commandToGameObjects.ContainsKey(command) || commandToVisualElement.ContainsKey(command);
            }
        }
    }
}

#endif

