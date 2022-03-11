namespace DefaultToolPowerSelect
{
    public class ModConfig
    {
        public ModConfig()
        {
            EnabledOnStart = true;
            EnableKey = "OemTilde";
            SavePowerLevelKey = "Tab";
            ResetPowerLevelKey = "R";
            AlwaysReplacePower = false;
        }

        public bool EnabledOnStart { get; set; }
        public string EnableKey { get; set; }
        public string SavePowerLevelKey { get; set; }
        public string ResetPowerLevelKey { get; set; }
        public bool AlwaysReplacePower { get; set; }
    }
}