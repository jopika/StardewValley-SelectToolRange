using StardewModdingAPI.Utilities;

namespace QuickEat {
    public class ModConfig {
        public KeybindList EnableKey { get; set; } = KeybindList.Parse("F5");
        public KeybindList QuickEatKey { get; set; } = KeybindList.Parse("V");

        public bool EnableQuickEat { get; set; } = true;
        public bool AlwaysShowPromptWhenHealthEnergyIsFull { get; set; } = true;
        public bool SkipPromptForCoffee { get; set; } = false;
    }
}