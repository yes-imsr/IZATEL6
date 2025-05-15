using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using PMMOEdit.Models;

namespace PMMOEdit.Helpers
{
    public static class TomlServerConfigParser
    {
        public static ServerConfig ParseServerConfig(string tomlContent)
        {
            var config = new ServerConfig();

            // Simple implementation to parse main settings
            // In a production app, you would use a proper TOML parser library
            var lines = tomlContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            string currentSection = "";
            string currentSubSection = "";

            foreach (var line in lines)
            {
                string trimmedLine = line.Trim();
                if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith("#"))
                    continue;

                // Check for section headers
                if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                {
                    string section = trimmedLine.Substring(1, trimmedLine.Length - 2);
                    if (section.Contains("."))
                    {
                        string[] parts = section.Split(new[] { '.' }, 2);
                        currentSection = parts[0];
                        currentSubSection = parts[1];
                    }
                    else
                    {
                        currentSection = section;
                        currentSubSection = "";
                    }

                    continue;
                }

                // Parse key-value pairs
                int equalsPos = trimmedLine.IndexOf('=');
                if (equalsPos > 0)
                {
                    string key = trimmedLine.Substring(0, equalsPos).Trim().Replace("\"", "");
                    string value = trimmedLine.Substring(equalsPos + 1).Trim();

                    // Remove quotes if present
                    if (value.StartsWith("\"") && value.EndsWith("\""))
                        value = value.Substring(1, value.Length - 2);

                    ParseSetting(config, currentSection, currentSubSection, key, value);
                }
            }

            return config;
        }

        private static void ParseSetting(ServerConfig config, string section, string subSection, string key,
            string value)
        {
            // Parse settings based on section
            switch (section)
            {
                case "General":
                    ParseGeneralSetting(config, key, value);
                    break;
                case "Levels":
                    ParseLevelsSetting(config, subSection, key, value);
                    break;
                case "Requirements":
                    ParseRequirementsSetting(config, key, value);
                    break;
                case "XP_Gains":
                    ParseXpGainsSetting(config, subSection, key, value);
                    break;
                case "Party":
                    ParsePartySetting(config, subSection, key, value);
                    break;
                case "Mob_Scaling":
                    ParseMobScalingSetting(config, subSection, key, value);
                    break;
                case "Vein_Miner":
                    ParseVeinMinerSetting(config, key, value);
                    break;
            }
        }

        private static void ParseGeneralSetting(ServerConfig config, string key, string value)
        {
            switch (key)
            {
                case "Creative Reach":
                    if (double.TryParse(value, out double creativeReach))
                        config.CreativeReach = creativeReach;
                    break;
                case "Salvage Block":
                    config.SalvageBlock = value;
                    break;
                case "Treasure Enabled":
                    if (bool.TryParse(value, out bool treasureEnabled))
                        config.TreasureEnabled = treasureEnabled;
                    break;
                case "brewing_tracked":
                    if (bool.TryParse(value, out bool brewingTracked))
                        config.BrewingTracked = brewingTracked;
                    break;
            }
        }

        private static void ParseLevelsSetting(ServerConfig config, string subSection, string key, string value)
        {
            if (string.IsNullOrEmpty(subSection))
            {
                switch (key)
                {
                    case "Max Level":
                        if (int.TryParse(value, out int maxLevel))
                            config.MaxLevel = maxLevel;
                        break;
                    case "Use Exponential Formula":
                        if (bool.TryParse(value, out bool useExpFormula))
                            config.UseExponentialFormula = useExpFormula;
                        break;
                    case "Loss on death":
                        if (double.TryParse(value, out double lossOnDeath))
                            config.LossOnDeath = lossOnDeath;
                        break;
                    case "Lose Levels On Death":
                        if (bool.TryParse(value, out bool loseLevelsOnDeath))
                            config.LoseLevelsOnDeath = loseLevelsOnDeath;
                        break;
                    case "Lose Only Excess":
                        if (bool.TryParse(value, out bool loseOnlyExcess))
                            config.LoseOnlyExcess = loseOnlyExcess;
                        break;
                    case "Global Modifier":
                        if (double.TryParse(value, out double globalMod))
                            config.GlobalModifier = globalMod;
                        break;
                }
            }
            else if (subSection == "LINEAR LEVELS")
            {
                switch (key)
                {
                    case "Base XP":
                        if (int.TryParse(value, out int baseXp))
                            config.LinearBaseXP = baseXp;
                        break;
                    case "Per Level":
                        if (double.TryParse(value, out double perLevel))
                            config.LinearPerLevel = perLevel;
                        break;
                }
            }
            else if (subSection == "EXPONENTIAL LEVELS")
            {
                switch (key)
                {
                    case "Base XP":
                        if (int.TryParse(value, out int baseXp))
                            config.ExpBaseXP = baseXp;
                        break;
                    case "Power Base":
                        if (double.TryParse(value, out double powerBase))
                            config.ExpPowerBase = powerBase;
                        break;
                    case "Per Level":
                        if (double.TryParse(value, out double perLevel))
                            config.ExpPerLevel = perLevel;
                        break;
                }
            }
            else if (subSection == "Skill Modifiers")
            {
                if (double.TryParse(value, out double modifier))
                    config.SkillModifiers[key] = modifier;
            }
        }

        private static void ParseRequirementsSetting(ServerConfig config, string key, string value)
        {
            if (bool.TryParse(value, out bool reqEnabled))
            {
                switch (key)
                {
                    case "WEAR Req Enabled":
                        config.WearReqEnabled = reqEnabled;
                        break;
                    case "USE_ENCHANTMENT Req Enabled":
                        config.UseEnchantmentReqEnabled = reqEnabled;
                        break;
                    case "TOOL Req Enabled":
                        config.ToolReqEnabled = reqEnabled;
                        break;
                    case "WEAPON Req Enabled":
                        config.WeaponReqEnabled = reqEnabled;
                        break;
                    case "USE Req Enabled":
                        config.UseReqEnabled = reqEnabled;
                        break;
                    case "PLACE Req Enabled":
                        config.PlaceReqEnabled = reqEnabled;
                        break;
                    case "BREAK Req Enabled":
                        config.BreakReqEnabled = reqEnabled;
                        break;
                    case "KILL Req Enabled":
                        config.KillReqEnabled = reqEnabled;
                        break;
                    case "TRAVEL Req Enabled":
                        config.TravelReqEnabled = reqEnabled;
                        break;
                    case "RIDE Req Enabled":
                        config.RideReqEnabled = reqEnabled;
                        break;
                    case "TAME Req Enabled":
                        config.TameReqEnabled = reqEnabled;
                        break;
                    case "BREED Req Enabled":
                        config.BreedReqEnabled = reqEnabled;
                        break;
                    case "INTERACT Req Enabled":
                        config.InteractReqEnabled = reqEnabled;
                        break;
                    case "ENTITY_INTERACT Req Enabled":
                        config.EntityInteractReqEnabled = reqEnabled;
                        break;
                }
            }
        }

        private static void ParseVeinMinerSetting(ServerConfig config, string key, string value)
        {
            switch (key)
            {
                case "vein enabled":
                    if (bool.TryParse(value, out bool veinEnabled))
                        config.veinEnabled = veinEnabled;
                    break;
                case "Require Settings":
                    if (bool.TryParse(value, out bool requireSettings))
                        config.RequireSettings = requireSettings;
                    break;
                case "Vein Mine Default Consume":
                    if (int.TryParse(value, out int defaultConsume))
                        config.VeinMineDefaultConsume = defaultConsume;
                    break;
                case "Vein Charge Modifier":
                    if (double.TryParse(value, out double chargeModifier))
                        config.VeinChargeModifier = chargeModifier;
                    break;
                case "base charge rate":
                    if (double.TryParse(value, out double baseChargeRate))
                        config.BaseChargeRate = baseChargeRate;
                    break;
                case "base vein capacity":
                    if (int.TryParse(value, out int baseVeinCapacity))
                        config.BaseVeinCapacity = baseVeinCapacity;
                    break;
                case "Vein_Blacklist":
                    // Parse the blacklist array
                    if (value.StartsWith("[") && value.EndsWith("]"))
                    {
                        string listContent = value.Substring(1, value.Length - 2);
                        string[] items = listContent.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        config.VeinBlacklist.Clear();
                        foreach (var item in items)
                        {
                            string cleanedItem = item.Trim().Trim('"');
                            if (!string.IsNullOrEmpty(cleanedItem))
                                config.VeinBlacklist.Add(cleanedItem);
                        }
                    }

                    break;
            }
        }

        public static string GenerateTomlConfig(ServerConfig config)
        {
            StringBuilder sb = new StringBuilder();

            // Add empty line at the beginning (per original format)
            sb.AppendLine();

            // Add General Section
            sb.AppendLine("#General settings on the server");
            sb.AppendLine("[General]");
            sb.AppendLine("\t#how much extra reach should a player get in creative mode");
            sb.AppendLine("\t#Range: 4.0 ~ 1.7976931348623157E308");
            sb.AppendLine($"\t\"Creative Reach\" = {config.CreativeReach}");
            sb.AppendLine("\t#Which block should be used for salvaging");
            sb.AppendLine($"\t\"Salvage Block\" = \"{config.SalvageBlock}\"");
            sb.AppendLine("\t#if false, all pmmo loot conditions will be turned off");
            sb.AppendLine($"\t\"Treasure Enabled\" = {config.TreasureEnabled.ToString().ToLower()}");
            sb.AppendLine("\t#If false, pmmo will not track if a potion was previously brewed.");
            sb.AppendLine("\t#this helps with stacking potions from other mods, but ");
            sb.AppendLine("\t#does not prevent users from pick-placing potions in the");
            sb.AppendLine("\t#brewing stand for free XP. Toggle at your discretion.");
            sb.AppendLine($"\tbrewing_tracked = {config.BrewingTracked.ToString().ToLower()}");
            sb.AppendLine();

            // Add Levels Section
            sb.AppendLine("#Settings related level gain");
            sb.AppendLine("[Levels]");
            sb.AppendLine("\t#The highest level a player can achieve in any skill.");
            sb.AppendLine("\t#NOTE: if this is changing on you to a lower value, that's intentional");
            sb.AppendLine("\t#If your formula makes the required xp to get max level greater than");
            sb.AppendLine("\t#pmmo can store, pmmo will replace your value with the actual max.");
            sb.AppendLine("\t#Range: > 1");
            sb.AppendLine($"\t\"Max Level\" = {config.MaxLevel}");
            sb.AppendLine("\t#should levels be determined using an exponential formula?");
            sb.AppendLine($"\t\"Use Exponential Formula\" = {config.UseExponentialFormula.ToString().ToLower()}");
            sb.AppendLine("\t#=====LEAVE -1 VALUE UNLESS YOU WANT STATIC LEVELS=====");
            sb.AppendLine(
                "\t#Replacing the -1 and adding values to this list will set the xp required to advance for each");
            sb.AppendLine("\t#level manually.  Note that the number of level settings you enter into this list");
            sb.AppendLine("\t#will set your max level.  If you only add 10 entries, your max level will be 10.");
            sb.AppendLine("\t#This setting is intended for players/ops who want fine-tune control over their");
            sb.AppendLine("\t#level growth.  use with caution.  ");
            sb.AppendLine("\t#");
            sb.AppendLine("\t#As a technical note, if you enter values that are not greater than their previous");
            sb.AppendLine("\t#value, the entire list will be ignored and revert back to the selected exponential");
            sb.AppendLine("\t#or linear formulaic calculation");

            // Format the static levels list
            sb.Append("\tStatic_Levels = [");
            for (int i = 0; i < config.StaticLevels.Count; i++)
            {
                sb.Append(config.StaticLevels[i]);
                if (i < config.StaticLevels.Count - 1)
                    sb.Append(", ");
            }

            sb.AppendLine("]");

            sb.AppendLine("\t#How much experience should players lose when they die?");
            sb.AppendLine("\t#zero is no loss, one is lose everything");
            sb.AppendLine("\t#Range: 0.0 ~ 1.0");
            sb.AppendLine($"\t\"Loss on death\" = {config.LossOnDeath}");
            sb.AppendLine("\t#should loss of experience cross levels?");
            sb.AppendLine("\t#for example, if true, a player with 1 xp above their current level would lose the");
            sb.AppendLine("\t#[Loss on death] percentage of xp and fall below their current level.  However,");
            sb.AppendLine(
                "\t#if false, the player would lose only 1 xp as that would put them at the base xp of their current level");
            sb.AppendLine($"\t\"Lose Levels On Death\" = {config.LoseLevelsOnDeath.ToString().ToLower()}");
            sb.AppendLine("\t#This setting only matters if [Lose Level On Death] is set to false.");
            sb.AppendLine(
                "\t#If this is true the [Loss On Death] applies only to the experience above the current level");
            sb.AppendLine(
                "\t#for example if level 3 is 1000k xp and the player has 1020 and dies.  the player will only lose");
            sb.AppendLine("\t#the [Loss On Death] of the 20 xp above the level's base.");
            sb.AppendLine($"\t\"Lose Only Excess\" = {config.LoseOnlyExcess.ToString().ToLower()}");
            sb.AppendLine("\t#Modifies how much xp is earned.  This is multiplicative to the XP.");
            sb.AppendLine("\t#(Mutually Exclusive to [Skill Modifiers])");
            sb.AppendLine($"\t\"Global Modifier\" = {config.GlobalModifier}");
            sb.AppendLine();

            return sb.ToString();
                    }
                    
                    private static void ParseXpGainsSetting(ServerConfig config, string subSection, string key, string value)
                    {
            if (string.IsNullOrEmpty(subSection))
            {
                switch (key)
                {
                    case "Reuse Penalty":
                        if (double.TryParse(value, out double reusePenalty))
                            config.ReusePenalty = reusePenalty;
                        break;
                    case "Perks Plus Configs":
                        if (bool.TryParse(value, out bool perksPlusConfigs))
                            config.PerksPlusConfigs = perksPlusConfigs;
                        break;
                }
            }
            // XP Event-specific parsing would be more complex
            // For advanced implementation, would need deeper nested parsing
                    }

            private static void ParsePartySetting(ServerConfig config, string subSection, string key, string value)
            {
                if (string.IsNullOrEmpty(subSection))
                {
                    if (key == "Party Range" && int.TryParse(value, out int partyRange))
                        config.PartyRange = partyRange;
                }
                else if (subSection == "Party Bonus")
                {
                    if (double.TryParse(value, out double bonus))
                        config.PartyBonus[key] = bonus;
                }
            }

            private static void ParseMobScalingSetting(ServerConfig config, string subSection, string key, string value)
            {
                if (string.IsNullOrEmpty(subSection))
                {
                    switch (key)
                    {
                        case "Enable Mob Scaling":
                            if (bool.TryParse(value, out bool enableMobScaling))
                                config.EnableMobScaling = enableMobScaling;
                            break;
                        case "Scaling AOE":
                            if (int.TryParse(value, out int scalingAOE))
                                config.ScalingAOE = scalingAOE;
                            break;
                        case "Base Level":
                            if (int.TryParse(value, out int baseLevel))
                                config.BaseLevel = baseLevel;
                            break;
                        case "boss_scaling":
                            if (double.TryParse(value, out double bossScaling))
                                config.BossScaling = bossScaling;
                            break;
                    }
                }
                else if (subSection == "Formula")
                {
                    if (key == "Use Exponential Formula" && bool.TryParse(value, out bool useExpFormula))
                        config.MobScalingUseExponentialFormula = useExpFormula;
                }
                else if (subSection.StartsWith("Formula.LINEAR_LEVELS"))
                {
                    if (key == "Per Level" && double.TryParse(value, out double perLevel))
                        config.MobScalingLinearPerLevel = perLevel;
                }
                else if (subSection.StartsWith("Formula.EXPONENTIAL_LEVELS"))
                {
                    switch (key)
                    {
                        case "Power Base":
                            if (double.TryParse(value, out double powerBase))
                                config.MobScalingExpPowerBase = powerBase;
                            break;
                        case "Per Level":
                            if (double.TryParse(value, out double perLevel))
                                config.MobScalingExpPerLevel = perLevel;
                            break;
                    }
                }
            }
        }
    }
