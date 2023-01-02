// Jotepad
// A Valheim mod that adds a To-Do task/checklist.
// 
// File:    Jotepad.cs
// Project: Jotepad

using BepInEx;
using Jotunn.GUI;
using Jotunn.Managers;
using UnityEngine.UI;
using UnityEngine;
using Jotunn.Configs;
using BepInEx.Configuration;
using static ItemSets;

namespace Jotepad
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    internal class Jotepad : BaseUnityPlugin
    {
        public const string PluginGUID = "com.jotunn.Jotepad";
        public const string PluginName = "Jotepad";
        public const string PluginVersion = "0.7.6";
        private GameObject JotepadPanel;
        private ButtonConfig ShowGUIButton;
        private ConfigEntry<string> JotepadStringConfig;
        private ConfigEntry<KeyCode> ToggleJotepadConfig;
        private string[] JotepadArray = new string[MAX_ITEMS];
        private int numItems = 0;
        private const int MAX_ITEMS = 10;
        private const int MAX_LENGTH = 50;
        private const float ITEM_TEXT_WIDTH = 700f;
        private const float ITEM_TEXT_HEIGHT = 30f;
        private const float START_TEXT_Y_POS = 200f;
        private const float MOVE_TEXT_Y = 45f;
        private const float TEXT_X_POS = 25f;
        private const float DELETE_SIZE = 30f;
        private const float DELETE_X_POS = -350f;
        private const string SERIAL_TOKEN = "``]]JOTE[[``";

        private void SaveList()
        {
            string serializedList = "";
            for (int i = 0; i < numItems; i++)            
            {
                serializedList += JotepadArray[i];
                if (i < numItems - 1)
                {
                    serializedList += SERIAL_TOKEN;
                }
            }
            JotepadStringConfig.Value = serializedList;
        }
        private void ReadList()
        {
            if (JotepadStringConfig == null) { return; }
            string serializedList = JotepadStringConfig.Value;
            if (serializedList.IsNullOrWhiteSpace())
            {
                return;
            }
            string[] tempArray = serializedList.Split(new string[] { SERIAL_TOKEN }, System.StringSplitOptions.RemoveEmptyEntries);            
            foreach (string item in tempArray)
            {
                JotepadArray[numItems] = item;
                numItems++;
            }
        }
        private void AddTextItem()
        {
            var inputField = JotepadPanel.GetComponentInChildren<InputField>();

            string newItem = inputField.text.Trim();
            // Don't add empty items or just whitespace, or if it contains the serializer token
            if (newItem.IsNullOrWhiteSpace() || newItem.Contains(SERIAL_TOKEN))
            {
                return;
            }
            // Check if max. items reached.
            if (numItems >= MAX_ITEMS)
            {
                inputField.text = "Max reached.";
                numItems = MAX_ITEMS;
                return;
            }
            // If it's longer than the max length, cut the end off.
            if (newItem.Length > MAX_LENGTH)
            {
                newItem = newItem.Substring(0, MAX_LENGTH-2) + "..";
            }
            JotepadArray[numItems] = newItem;
            numItems++;
            inputField.text = "";
            ReloadThenSaveList();
        }
        private void ReloadList()
        {
            // Loop through all Text objects
            Text[] textFields = JotepadPanel.GetComponentsInChildren<Text>();            
            foreach (var field in textFields)
            {
                // Skip any text fields with names that don't start with ItemText
                if (field.name.StartsWith("ItemText"))
                {
                    // Remove "ItemText" from the name, then parse that into an integer
                    int fieldNum = int.Parse(field.name.Replace("ItemText", ""));
                    field.text = (fieldNum + 1).ToString() + ". " + JotepadArray[fieldNum];
                }
            }
        }
        private void Awake()
        {
            numItems = 0;
            CreateConfigValues();
            ReadList();
            AddInputs();            
        }
        private void Update()
        {
            // Since our Update function in our BepInEx mod class will load BEFORE Valheim loads,
            // we need to check that ZInput is ready to use first.
            if (ZInput.instance != null)
            {
                // Check if our button is pressed. This will only return true ONCE, right after our button is pressed.
                // If we hold the button down, it won't spam toggle our menu.
                if (ZInput.GetButtonDown(ShowGUIButton.Name))
                {
                    ToggleJotepad();
                }

            }
        }
        private void ClearJotepad()
        {
            for (int i = 0; i < MAX_ITEMS; i++)
            {
                JotepadArray[i] = "";
            }
            numItems = 0;
            ReloadThenSaveList();
        }
        private GameObject NewItemText(float yPos, int num, string itemString)
        {
            GameObject itemText = GUIManager.Instance.CreateText(
                    text: (num + 1).ToString() + ". " + itemString,
                    parent: JotepadPanel.transform,
                    anchorMin: new Vector2(0.5f, 0.5f),
                    anchorMax: new Vector2(0.5f, 0.5f),
                    position: new Vector2(TEXT_X_POS, yPos),
                    font: GUIManager.Instance.AveriaSerifBold,
                    fontSize: 24,
                    color: GUIManager.Instance.ValheimOrange,
                    outline: true,
                    outlineColor: Color.black,
                    width: ITEM_TEXT_WIDTH,
                    height: ITEM_TEXT_HEIGHT,
                    addContentSizeFitter: false);
            itemText.name = "ItemText" + num.ToString();
            return itemText;
        }
        private GameObject NewDeleteButton(float yPos)
        {
            GameObject buttonObject = GUIManager.Instance.CreateButton(
                    text: "X",
                    parent: JotepadPanel.transform,
                    anchorMin: new Vector2(0.5f, 0.5f),
                    anchorMax: new Vector2(0.5f, 0.5f),
                    position: new Vector2(DELETE_X_POS, yPos),
                    width: DELETE_SIZE,
                    height: DELETE_SIZE);
            buttonObject.SetActive(true);
            return buttonObject;
        }
        private void ReloadThenSaveList()
        {
            ReloadList();
            SaveList();
            GUI.FocusControl("InputField");
        }
        private void ToggleJotepad()
        {
            // Create the panel if it does not exist
            if (!JotepadPanel)
            {
                CreateJotepadPanel();
            }

            // Switch the current state
            bool state = !JotepadPanel.activeSelf;

            // Set the active state of the panel
            JotepadPanel.SetActive(state);

            // Toggle input for the player and camera while displaying the GUI
            GUIManager.BlockInput(state);
        }

        private void CreateJotepadPanel()
        {
            #region Panel and Title
            if (GUIManager.Instance == null)
            {
                return;
            }
            if (!GUIManager.CustomGUIFront)
            {
                return;
            }
            // Wooden Jotepad panel
            JotepadPanel = GUIManager.Instance.CreateWoodpanel(
                parent: GUIManager.CustomGUIFront.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(0, 0),
                width: 850,
                height: 700,
                draggable: false);
            JotepadPanel.SetActive(false);
            JotepadPanel.AddComponent<DragWindowCntrl>();

            // Jotepad label
            GameObject itemText = GUIManager.Instance.CreateText(
                text: "Jotepad",
                parent: JotepadPanel.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(5f, 265f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 48,
                color: GUIManager.Instance.ValheimOrange,
                outline: true,
                outlineColor: Color.black,
                width: 200f,
                height: 80f,
                addContentSizeFitter: false);
            #endregion

            #region To Do list items and buttons
            // To do list text items
            float yPos = START_TEXT_Y_POS;
            GameObject[] JotepadTextItems = new GameObject[MAX_ITEMS];
            for (int i = 0; i < MAX_ITEMS; i++)
            {
                if (i < JotepadArray.Length)
                {
                    string val = "";
                    if (!JotepadArray[i].IsNullOrWhiteSpace()) { val = JotepadArray[i]; }
                    JotepadTextItems[i] = NewItemText(yPos, i, val);
                }
                else
                {
                    JotepadTextItems[i] = NewItemText(yPos, i, "");
                }
                yPos -= MOVE_TEXT_Y;
            }

            // To do list delete buttons.
            yPos = START_TEXT_Y_POS;
            GameObject[] DeleteButtons = new GameObject[MAX_ITEMS];
            for (int i = 0; i < MAX_ITEMS; i++)
            {
                DeleteButtons[i] = NewDeleteButton(yPos);
                yPos -= MOVE_TEXT_Y;
            }
            // There is definitely a more elegant way to do this, but I was very lazy... and it's only 10 buttons! :)
            UnityEngine.UI.Button delButton1 = DeleteButtons[0].GetComponent<UnityEngine.UI.Button>();
            delButton1.onClick.AddListener(Del1);
            UnityEngine.UI.Button delButton2 = DeleteButtons[1].GetComponent<UnityEngine.UI.Button>();
            delButton2.onClick.AddListener(Del2);
            UnityEngine.UI.Button delButton3 = DeleteButtons[2].GetComponent<UnityEngine.UI.Button>();
            delButton3.onClick.AddListener(Del3);
            UnityEngine.UI.Button delButton4 = DeleteButtons[3].GetComponent<UnityEngine.UI.Button>();
            delButton4.onClick.AddListener(Del4);
            UnityEngine.UI.Button delButton5 = DeleteButtons[4].GetComponent<UnityEngine.UI.Button>();
            delButton5.onClick.AddListener(Del5);
            UnityEngine.UI.Button delButton6 = DeleteButtons[5].GetComponent<UnityEngine.UI.Button>();
            delButton6.onClick.AddListener(Del6);
            UnityEngine.UI.Button delButton7 = DeleteButtons[6].GetComponent<UnityEngine.UI.Button>();
            delButton7.onClick.AddListener(Del7);
            UnityEngine.UI.Button delButton8 = DeleteButtons[7].GetComponent<UnityEngine.UI.Button>();
            delButton8.onClick.AddListener(Del8);
            UnityEngine.UI.Button delButton9 = DeleteButtons[8].GetComponent<UnityEngine.UI.Button>();
            delButton9.onClick.AddListener(Del9);
            UnityEngine.UI.Button delButton10 = DeleteButtons[9].GetComponent<UnityEngine.UI.Button>();
            delButton10.onClick.AddListener(Del10);
            #endregion

            #region Bottom buttons and input field
            // Close button
            GameObject closeButtonObject = GUIManager.Instance.CreateButton(
                text: "Close",
                parent: JotepadPanel.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(300, -275f),
                width: 100f,
                height: 60f);
            closeButtonObject.SetActive(true);

            // Add a listener to the button to close the panel again
            UnityEngine.UI.Button closeButton = closeButtonObject.GetComponent<UnityEngine.UI.Button>();
            closeButton.onClick.AddListener(ToggleJotepad);

            // Clear button
            GameObject clearButtonObject = GUIManager.Instance.CreateButton(
                text: "Clear List",
                parent: JotepadPanel.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(125, -275f),
                width: 100f,
                height: 60f);
            clearButtonObject.SetActive(true);

            // Add a listener to the button to close the panel again
            UnityEngine.UI.Button clearButton = clearButtonObject.GetComponent<UnityEngine.UI.Button>();
            clearButton.onClick.AddListener(ClearJotepad);

            // Add custom item input field
            GameObject inputField = GUIManager.Instance.CreateInputField(
                parent: JotepadPanel.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(-265f, -275f),
                contentType: InputField.ContentType.Standard,
                placeholderText: "dream big...",
                fontSize: 16,
                width: 200f,
                height: 40f);
            inputField.name = "InputField";

            // Add custom button
            GameObject addButtonObject = GUIManager.Instance.CreateButton(
                text: "Add",
                parent: JotepadPanel.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(-100f, -275f),
                width: 80f,
                height: 60f);
            addButtonObject.SetActive(true);
            UnityEngine.UI.Button addButton = addButtonObject.GetComponent<UnityEngine.UI.Button>();
            addButton.onClick.AddListener(AddTextItem);
            GUI.FocusControl("InputField"); // Doesn't seem to work :~(
            #endregion
        }
        private void RemoveArrayItem(int index)
        {
            // Don't remove the item if there is no item to remove
            if (index > numItems - 1)
            {
                return;
            }
            JotepadArray = RemoveElementAndCopyArray(JotepadArray, index);
            numItems--;
            var inputField = JotepadPanel.GetComponentInChildren<InputField>();
            if (inputField.text == "Max reached.")
            {
                inputField.text = "";
            }
            
            ReloadThenSaveList();
        }
        public string[] RemoveElementAndCopyArray(string[] array, int index)
        {
            string[] newArray = new string[array.Length];
            int j = 0;
            for (int i = 0; i < array.Length; i++)
            {
                if (i != index)
                {
                    newArray[j] = array[i];
                    j++;
                }
            }
            newArray[array.Length - 1] = "";
            return newArray;
        }
        private void Del1() { RemoveArrayItem(0); }
        private void Del2() { RemoveArrayItem(1); }
        private void Del3() { RemoveArrayItem(2); }
        private void Del4() { RemoveArrayItem(3); }
        private void Del5() { RemoveArrayItem(4); }
        private void Del6() { RemoveArrayItem(5); }
        private void Del7() { RemoveArrayItem(6); }
        private void Del8() { RemoveArrayItem(7); }
        private void Del9() { RemoveArrayItem(8); }
        private void Del10() { RemoveArrayItem(9); }
        private void AddInputs()
        {
            ShowGUIButton = new ButtonConfig
            {
                Name = "ToggleJotepadButton",
                Config = ToggleJotepadConfig,
                ActiveInCustomGUI = true  // Enable this button in custom GUI
            };
            InputManager.Instance.AddButton(PluginGUID, ShowGUIButton);
        }
        private void CreateConfigValues()
        {
            Config.SaveOnConfigSet = true;
            JotepadStringConfig = Config.Bind("Client config", "JotepadString", "", "Jotepad list");
            ToggleJotepadConfig = Config.Bind("Client config", "Toggle Jotepad", KeyCode.F4, new ConfigDescription("Show/hide the Jotepad panel."));
        }

    }
}




/*
// Dropdown list
GameObject dropdownList = GUIManager.Instance.CreateDropDown(
    parent: JotepadPanel.transform,
    anchorMin: new Vector2(0.5f, 0.5f),
    anchorMax: new Vector2(0.5f, 0.5f),
    position: new Vector2(-250f, -250f),
    fontSize: 16,
    width: 200f,
    height: 60f);
dropdownList.GetComponent<Dropdown>().AddOptions(new List<string>
{
    "Red Mushrooms", "Yellow Mushrooms", "Raspberries", "Blueberries", "Cloudberries",
    "Boar Meat", "Deer meat", "Neck tails", "Go fishing", "Wolf meat", "Lox meat",
    "Deer hide", "Leather scraps", "Troll hides", "Wolf hides", "Lox hides",
    "Core wood", "Fine wood", "Yggdrasil wood",
    "Copper ore", "Tin ore", "Iron scraps", "Silver ore", "Blackmetal",
    "Find Haldor", "Find Elder", "Kill Elder", "Find Bonemass", "Kill Bonemass",
    "Find Moder", "Kill Moder", "Find Yagluth", "Kill Yagluth", "Find The Queen", "Kill The Queen",

});

// Add dropdown button
GameObject dropdownButtonObject = GUIManager.Instance.CreateButton(
    text: "Add",
    parent: JotepadPanel.transform,
    anchorMin: new Vector2(0.5f, 0.5f),
    anchorMax: new Vector2(0.5f, 0.5f),
    position: new Vector2(-100f, -250f),
    width: 80f,
    height: 40f);
dropdownButtonObject.SetActive(true);

// Add a listener to the button to close the panel again
Button dropdownButton = dropdownButtonObject.GetComponent<Button>();
dropdownButton.onClick.AddListener(AddDropdownItem);

*/