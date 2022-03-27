using System;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace QuickEat {
    /// <summary>
    /// Entry point for QuickEat mod
    /// </summary>
    public class ModEntry : Mod {
        /// <summary> ID to represent GenericModConfigMenu </summary>
        private const string genericModConfigMenuId = "spacechase0.GenericModConfigMenu";

        /// <summary> The mod configuration </summary>
        private ModConfig Config;

        /// <summary>
        ///     Handles the beginning entry into the mod, loads Config and sets up event handlers
        /// </summary>
        /// <param name="helper"></param>
        /// <exception cref="NotImplementedException"></exception>
        public override void Entry(IModHelper helper) {
            Config = Helper.ReadConfig<ModConfig>();

            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            Monitor.Log("Finished setting up all Event handlers", LogLevel.Info);

            // throw new NotImplementedException();
        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs args) {
            // ignore if player hasn't loaded into the game yet
            if (!Context.IsWorldReady) return;

            // check if mod is disabled
            if (!Config.EnableQuickEat) return;

            if (IsConfirmationShown(out var eatMenu)) {
                // Check if name is coffee or related objects
                var consumedItemName = Game1.player.itemToEat.Name.ToLower();
                if (Config.SkipPromptForCoffee)
                    switch (consumedItemName) {
                        case "coffee":
                        case "triple shot espresso":
                            SkipDialogMenu(eatMenu);
                            return;
                        default:
                            break;
                    }

                if (Config.AlwaysShowPromptWhenHealthEnergyIsFull) {
                    // Check if Player has full health or energy
                    var fullHealth = Game1.player.health == Game1.player.maxHealth;
                    var fullStamina = Game1.player.stamina.CompareTo(Game1.player.maxStamina) == 0;
                    if (!fullHealth || !fullStamina) {
                        SkipDialogMenu(eatMenu);
                        return;
                    }

                    return;
                }

                SkipDialogMenu(eatMenu);
            }
        }

        private void SkipDialogMenu(DialogueBox eatMenu) {
            // When the animation starts, the game shows a yes/no dialogue asking the player to
            // confirm they really want to eat the item. This code answers 'yes' and closes the
            // dialogue.

            var yes = eatMenu.responses[0];
            Game1.currentLocation.answerDialogue(yes);
            eatMenu.closeDialogue();
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e) {
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>(genericModConfigMenuId);

            if (configMenu is null) return;

            PopulateConfigModMenu(configMenu);
        }

        /// <summary>
        ///     Handles the management of the Config's Mod Menu
        /// </summary>
        /// <param name="configMenu"></param>
        private void PopulateConfigModMenu(IGenericModConfigMenuApi configMenu) {
            // Register mod
            configMenu.Register(
                ModManifest,
                () => Config = new ModConfig(),
                () => Helper.WriteConfig(Config)
            );

            configMenu.AddBoolOption(
                ModManifest,
                name: () => "Enable QuickEat",
                tooltip: () => "Enables QuickEat and skips eating prompt when eating food",
                getValue: () => Config.EnableQuickEat,
                setValue: (value) => Config.EnableQuickEat = value
            );

            configMenu.AddBoolOption(
                ModManifest,
                name: () => "Max Energy & Health Prompt",
                tooltip: () => "If enabled, still show the eating prompt if both the player's energy or heath are full",
                getValue: () => Config.AlwaysShowPromptWhenHealthEnergyIsFull,
                setValue: (value) => Config.AlwaysShowPromptWhenHealthEnergyIsFull = value
            );

            configMenu.AddBoolOption(
                ModManifest,
                name: () => "Skip Coffee Prompt",
                tooltip: () => "If enabled, always skip prompt for coffee",
                getValue: () => Config.SkipPromptForCoffee,
                setValue: (value) => Config.SkipPromptForCoffee = value);
        }

        /// <summary>Get whether the eat/drink confirmation is being shown.</summary>
        /// <param name="menu">The confirmation menu.</param>
        private bool IsConfirmationShown(out DialogueBox menu) {
            if (Game1.player.itemToEat != null && Game1.activeClickableMenu is DialogueBox dialogue) {
                var actualLine = dialogue.dialogues.FirstOrDefault();
                var isConfirmation =
                    actualLine == GetString("Strings\\StringsFromCSFiles:Game1.cs.3159",
                        Game1.player.itemToEat.DisplayName) // drink
                    || actualLine == GetString("Strings\\StringsFromCSFiles:Game1.cs.3160",
                        Game1.player.itemToEat.DisplayName); // eat

                if (isConfirmation) {
                    menu = dialogue;
                    return true;
                }
            }

            menu = null;
            return false;
        }


        /// <summary>Get a translation by key.</summary>
        /// <param name="key">The translation key.</param>
        /// <param name="substitutions">The values for placeholders like <c>{0}</c> in the translation text.</param>
        private static string GetString(string key, params object[] substitutions) {
            return Game1.content.LoadString(key, substitutions);
        }
    }
}