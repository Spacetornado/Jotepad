// Jotepad
// A Valheim mod that adds a To-Do task/checklist.
// 
// File:    Jotepad.cs
// Project: Jotepad

using BepInEx;
using Jotunn.Entities;
using Jotunn.GUI;
using Jotunn.Managers;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Jotunn.Configs;
using BepInEx.Configuration;
using static ItemSets;
using System.Reflection.Emit;

namespace Jotepad
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    internal class Jotepad : BaseUnityPlugin
    {
        public const string PluginGUID = "com.jotunn.Jotepad";
        public const string PluginName = "Jotepad";
        public const string PluginVersion = "0.1.0";

        private GameObject JotepadPanel;
        private ButtonConfig ShowGUIButton;

        private string JotepadString;

        private void AddTextItem()
        {
            if (!JotepadPanel) { Jotunn.Logger.LogInfo("JotepadPanel is null."); }
            var inputField = JotepadPanel.GetComponentInChildren<InputField>();
            if (!inputField) { Jotunn.Logger.LogInfo("Could not find InputField"); }
            Jotunn.Logger.LogInfo("Adding " + inputField.text);
            var textField = JotepadPanel.GetComponentInChildren<Text>();
            JotepadString += "\n" + inputField.text;
            textField.text = JotepadString;
        }
        private void AddDropdownItem()
        {
            if (!JotepadPanel) { Jotunn.Logger.LogInfo("JotepadPanel is null."); }
            var dropdown = JotepadPanel.GetComponentInChildren<Dropdown>();
            if (!dropdown) { Jotunn.Logger.LogInfo("Could not find Dropdown"); }            
            var textField = JotepadPanel.GetComponentInChildren<Text>();
            JotepadString += "\n" + dropdown.GetComponentInChildren<Text>().text;
            textField.text = JotepadString;
        }

        // Use this class to add your own localization to the game
        // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
        //public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        private void Awake()
        {
            AddInputs();
        }

        // Called every frame
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
            var textField = JotepadPanel.GetComponentInChildren<Text>();
            textField.text = "";
        }
        private void ToggleJotepad()
        {
            // Create the panel if it does not exist
            if (!JotepadPanel)
            {
                if (GUIManager.Instance == null)
                {
                    Logger.LogError("GUIManager instance is null");
                    return;
                }

                if (!GUIManager.CustomGUIFront)
                {
                    Logger.LogError("GUIManager CustomGUI is null");
                    return;
                }

                // Create the panel object
                JotepadPanel = GUIManager.Instance.CreateWoodpanel(
                    parent: GUIManager.CustomGUIFront.transform,
                    anchorMin: new Vector2(0.5f, 0.5f),
                    anchorMax: new Vector2(0.5f, 0.5f),
                    position: new Vector2(0, 0),
                    width: 850,
                    height: 700,
                    draggable: false);
                JotepadPanel.SetActive(false);

                // Add the Jötunn draggable Component to the panel
                // Note: This is normally automatically added when using CreateWoodpanel()
                JotepadPanel.AddComponent<DragWindowCntrl>();

                // Jotepad label
                /*GameObject textObject = GUIManager.Instance.CreateText(
                    text: "Jotepad",
                    parent: JotepadPanel.transform,
                    anchorMin: new Vector2(0.5f, 1f),
                    anchorMax: new Vector2(0.5f, 1f),
                    position: new Vector2(0f, -100f),
                    font: GUIManager.Instance.AveriaSerifBold,
                    fontSize: 30,
                    color: GUIManager.Instance.ValheimOrange,
                    outline: true,
                    outlineColor: Color.black,
                    width: 350f,
                    height: 40f,
                    addContentSizeFitter: false);*/

                // To do list
                GameObject JotepadText = GUIManager.Instance.CreateText(
                    text: JotepadString,
                    parent: JotepadPanel.transform,
                    anchorMin: new Vector2(0.5f, 1f),
                    anchorMax: new Vector2(0.5f, 1f),
                    position: new Vector2(0f, 150f),
                    font: GUIManager.Instance.AveriaSerifBold,
                    fontSize: 18,
                    color: GUIManager.Instance.ValheimOrange,
                    outline: true,
                    outlineColor: Color.black,
                    width: 750f,
                    height: 400f,
                    addContentSizeFitter: false);

                // Close button
                GameObject closeButtonObject = GUIManager.Instance.CreateButton(
                    text: "Close",
                    parent: JotepadPanel.transform,
                    anchorMin: new Vector2(0.5f, 0.5f),
                    anchorMax: new Vector2(0.5f, 0.5f),
                    position: new Vector2(300, -250f),
                    width: 100f,
                    height: 60f);
                closeButtonObject.SetActive(true);

                // Add a listener to the button to close the panel again
                Button closeButton = closeButtonObject.GetComponent<Button>();
                closeButton.onClick.AddListener(ToggleJotepad);

                // Clear button
                GameObject clearButtonObject = GUIManager.Instance.CreateButton(
                    text: "Clear",
                    parent: JotepadPanel.transform,
                    anchorMin: new Vector2(0.5f, 0.5f),
                    anchorMax: new Vector2(0.5f, 0.5f),
                    position: new Vector2(150, -250f),
                    width: 100f,
                    height: 60f);
                clearButtonObject.SetActive(true);

                // Add a listener to the button to close the panel again
                Button clearButton = clearButtonObject.GetComponent<Button>();
                clearButton.onClick.AddListener(ClearJotepad);


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


                // Add custom item input field
                GameObject inputField = GUIManager.Instance.CreateInputField(
                    parent: JotepadPanel.transform,
                    anchorMin: new Vector2(0.5f, 0.5f),
                    anchorMax: new Vector2(0.5f, 0.5f),
                    position: new Vector2(-250f, -300f),
                    contentType: InputField.ContentType.Standard,
                    placeholderText: "whatevs...",
                    fontSize: 16,
                    width: 200f,
                    height: 40f);

                // Add custom button
                GameObject addButtonObject = GUIManager.Instance.CreateButton(
                    text: "Add",
                    parent: JotepadPanel.transform,
                    anchorMin: new Vector2(0.5f, 0.5f),
                    anchorMax: new Vector2(0.5f, 0.5f),
                    position: new Vector2(-100f, -300f),
                    width: 80f,
                    height: 60f);
                addButtonObject.SetActive(true);

                // Add a listener to the button to close the panel again
                Button addButton = addButtonObject.GetComponent<Button>();
                addButton.onClick.AddListener(AddTextItem);


            }

            // Switch the current state
            bool state = !JotepadPanel.activeSelf;

            // Set the active state of the panel
            JotepadPanel.SetActive(state);

            // Toggle input for the player and camera while displaying the GUI
            GUIManager.BlockInput(state);
        }


        // Add custom key bindings
        private void AddInputs()
        {
            // Add key bindings on the fly
            ShowGUIButton = new ButtonConfig
            {
                Name = "Jotepad_Toggle",
                Key = KeyCode.Insert,
                ActiveInCustomGUI = true  // Enable this button in custom GUI
            };
            InputManager.Instance.AddButton(PluginGUID, ShowGUIButton);

        }

    }
}

