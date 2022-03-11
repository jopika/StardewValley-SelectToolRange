using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace DefaultToolPowerSelect
{
    public class ModEntry : Mod
    {
        private Dictionary<SButton, Func<bool>> buttonTriggers;

        /// <summary>The mod configuration.</summary>
        private ModConfig Config;

        private bool modActive;
        private bool pulledUseToolKeys;

        private Dictionary<Tool, int> savedPowerLevels;

        /// <summary> They keys to listen to for tool use </summary>
        private HashSet<SButton> useToolKeys;

        /*
         * Public Methods
         */

        /// <summary>
        /// Beginning point of the Mod, loads in config and set up event handlers
        /// </summary>
        /// <param name="helper"></param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            buttonTriggers = new Dictionary<SButton, Func<bool>>();
            savedPowerLevels = new Dictionary<Tool, int>();
            useToolKeys = new HashSet<SButton>();

            pulledUseToolKeys = false;

            LoadConfig();

            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.Input.ButtonReleased += OnButtonRelease;
        }

        /*
         * Private Methods
         */

        /// <summary>
        ///     Loads the configuration file, and some of the buttons in memory
        /// </summary>
        /// <returns></returns>
        private bool LoadConfig()
        {
            var success = true;
            if (!Enum.TryParse(Config.EnableKey, true, out SButton enableButton))
            {
                success = false;
                Monitor.Log($"[Error] Invalid key specified to EnableKey: {Config.EnableKey}", LogLevel.Error);
            }

            if (!Enum.TryParse(Config.ResetPowerLevelKey, true, out SButton resetPowerLevelButton))
            {
                success = false;
                Monitor.Log($"[Error] Invalid key specified to ResetPowerLevelKey: {Config.ResetPowerLevelKey}",
                    LogLevel.Error);
            }

            if (!Enum.TryParse(Config.SavePowerLevelKey, true, out SButton savePowerLevelButton))
            {
                success = false;
                Monitor.Log($"[Error] Invalid key specified to SavePowerLevelKey: {Config.SavePowerLevelKey}",
                    LogLevel.Error);
            }

            buttonTriggers[enableButton] = ToggleModStatus;
            buttonTriggers[resetPowerLevelButton] = ResetPowerLevel;
            buttonTriggers[savePowerLevelButton] = SavePowerLevel;

            modActive = Config.EnabledOnStart;

            return success;
        }

        /// <summary>
        ///     Toggles the mod status, and displays a message to the user that it is enabled or disabled
        /// </summary>
        /// <returns></returns>
        private bool ToggleModStatus()
        {
            modActive = !modActive;
            if (modActive)
                Game1.addHUDMessage(new HUDMessage("[SelectToolRange] Enabled", Color.Green, 800f, false));
            else
                Game1.addHUDMessage(new HUDMessage("[SelectToolRange] Disabled", Color.Red, 800f, false));
            return true;
        }

        /// <summary>
        ///     Saves the power level for the given tool, the the mod is active
        /// </summary>
        /// <returns></returns>
        private bool SavePowerLevel()
        {
            // TODO: Complete this

            if (!modActive) return true;
            var currentTool = Game1.player.CurrentTool;
            var currentToolPower = Game1.player.toolPower;

            if (currentTool == null) return true;
            if (currentToolPower == 0) return true;

            savedPowerLevels[currentTool] = currentToolPower;

            Game1.addHUDMessage(new HUDMessage($"[SelectToolRange] Saved power for {currentTool.Name}", Color.Aqua,
                2000f, true));
            Monitor.Log($"Power level saved for tool: {currentTool.Name} @ level: {currentToolPower}");
            return true; // stub
        }

        /// <summary>
        ///     Resets the saved power level for the given tool, if the mod is active
        /// </summary>
        /// <returns></returns>
        private bool ResetPowerLevel()
        {
            // TODO: Complete this

            if (!modActive) return true;
            var currentTool = Game1.player.CurrentTool;

            if (currentTool == null) return true;

            if (savedPowerLevels.ContainsKey(currentTool))
            {
                Game1.addHUDMessage(new HUDMessage($"[SelectToolRange] Removed power for {currentTool.Name}", Color.Red,
                    2000f, true));
                Monitor.Log($"Power level removed for tool: {currentTool.Name}");
                savedPowerLevels.Remove(currentTool);
            }

            return true; // stub
        }

        /// <summary>
        ///     If the mod is active, attempts to replace the power level with the saved one, if it exists
        ///     If the current power level is non-zero, and the config setting "AlwaysReplacePower" is false, will not modify
        ///     anything
        /// </summary>
        /// <returns></returns>
        private bool UsePowerLevel()
        {
            if (!modActive) return true;
            var currentTool = Game1.player.CurrentTool;
            var originalPowerlevel = Game1.player.toolPower;

            if (currentTool == null) return true;

            if (!Config.AlwaysReplacePower && originalPowerlevel != 0)
            {
                Monitor.Log($"Skipping power override due to non-zero original power level ({originalPowerlevel})");
                return true;
            }

            if (!savedPowerLevels.ContainsKey(currentTool)) return true;
            var powerLevel = savedPowerLevels[currentTool];

            Game1.player.toolPower = powerLevel;
            Monitor.Log($"Overrode Power level: {originalPowerlevel} to: {powerLevel}");
            return true;
        }

        private void OnButtonRelease(object sender, ButtonReleasedEventArgs e)
        {
            // ignore if player hasn't loaded a save yet
            if (!Context.IsWorldReady) return;

            // If the key pressed is one of the "Use tool" keys
            if (useToolKeys.Contains(e.Button)) UsePowerLevel();
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            // ignore if player hasn't loaded a save yet
            if (!Context.IsWorldReady) return;

            if (!pulledUseToolKeys)
            {
                pulledUseToolKeys = true;
                Monitor.Log("Saving useKeys from game options");

                foreach (var toolButton in Game1.options.useToolButton)
                    if (!useToolKeys.Contains(toolButton.ToSButton()))
                    {
                        useToolKeys.Add(toolButton.ToSButton());
                        Monitor.Log($"Added key: {toolButton.ToSButton()}");
                    }
            }

            if (buttonTriggers.ContainsKey(e.Button)) buttonTriggers[e.Button]();
        }
    }
}