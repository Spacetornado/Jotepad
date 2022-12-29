// Jotepad
// a Valheim mod skeleton using Jötunn
// 
// File:    Jotepad.cs
// Project: Jotepad

using BepInEx;
using Jotunn.Entities;
using Jotunn.Managers;

namespace Jotepad
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    //[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class Jotepad : BaseUnityPlugin
    {
        public const string PluginGUID = "com.jotunn.Jotepad";
        public const string PluginName = "Jotepad";
        public const string PluginVersion = "0.1.0";
        
        // Use this class to add your own localization to the game
        // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
        public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        private void Awake()
        {
            Jotunn.Logger.LogInfo("Jotepad loaded");


        }
    }
}

