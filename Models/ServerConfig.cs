using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PMMOEdit.Models
{
    public class ServerConfig
    {
        // General Settings
        public double CreativeReach { get; set; } = 50.0;
        public string SalvageBlock { get; set; } = "minecraft:smithing_table";
        public bool TreasureEnabled { get; set; } = true;
        public bool BrewingTracked { get; set; } = true;

        // Levels Settings
        public int MaxLevel { get; set; } = 329;
        public bool UseExponentialFormula { get; set; } = true;
        public List<int> StaticLevels { get; set; } = new List<int> { -1 };
        public double LossOnDeath { get; set; } = 0.05;
        public bool LoseLevelsOnDeath { get; set; } = false;
        public bool LoseOnlyExcess { get; set; } = true;
        public double GlobalModifier { get; set; } = 1.0;
        public Dictionary<string, double> SkillModifiers { get; set; } = new Dictionary<string, double>();

        // Linear Levels Settings
        public int LinearBaseXP { get; set; } = 250;
        public double LinearPerLevel { get; set; } = 500.0;

        // Exponential Levels Settings
        public int ExpBaseXP { get; set; } = 250;
        public double ExpPowerBase { get; set; } = 1.104088404342588;
        public double ExpPerLevel { get; set; } = 1.1;

        // Requirements Settings
        public bool WearReqEnabled { get; set; } = true;
        public bool UseEnchantmentReqEnabled { get; set; } = true;
        public bool ToolReqEnabled { get; set; } = true;
        public bool WeaponReqEnabled { get; set; } = true;
        public bool UseReqEnabled { get; set; } = true;
        public bool PlaceReqEnabled { get; set; } = true;
        public bool BreakReqEnabled { get; set; } = true;
        public bool KillReqEnabled { get; set; } = true;
        public bool TravelReqEnabled { get; set; } = true;
        public bool RideReqEnabled { get; set; } = true;
        public bool TameReqEnabled { get; set; } = true;
        public bool BreedReqEnabled { get; set; } = true;
        public bool InteractReqEnabled { get; set; } = true;
        public bool EntityInteractReqEnabled { get; set; } = true;

        // XP Gains Settings
        public double ReusePenalty { get; set; } = 0.0;
        public bool PerksPlusConfigs { get; set; } = false;

        // Party Settings
        public int PartyRange { get; set; } = 50;
        public Dictionary<string, double> PartyBonus { get; set; } = new Dictionary<string, double>();

        // Mob Scaling Settings
        public bool EnableMobScaling { get; set; } = true;
        public int ScalingAOE { get; set; } = 150;
        public int BaseLevel { get; set; } = 0;
        public double BossScaling { get; set; } = 1.1;
        public bool MobScalingUseExponentialFormula { get; set; } = true;
        public double MobScalingLinearPerLevel { get; set; } = 1.0;
        public double MobScalingExpPowerBase { get; set; } = 1.104088404342588;
        public double MobScalingExpPerLevel { get; set; } = 1.0;

        // Vein Miner Settings
        public bool veinEnabled { get; set; } = true;  // Lowercase to match parser
        public bool RequireSettings { get; set; } = true;
        public int VeinMineDefaultConsume { get; set; } = 1;
        public double VeinChargeModifier { get; set; } = 0.05;
        public double BaseChargeRate { get; set; } = 1.0;
        public int BaseVeinCapacity { get; set; } = 64;
        public List<string> VeinBlacklist { get; set; } = new List<string>();

        // Default event XP awards nested dictionaries
        public Dictionary<string, Dictionary<string, Dictionary<string, double>>> DamageXP { get; set; } = new Dictionary<string, Dictionary<string, Dictionary<string, double>>>();
        public Dictionary<string, Dictionary<string, double>> JumpsXP { get; set; } = new Dictionary<string, Dictionary<string, double>>();
        public Dictionary<string, Dictionary<string, double>> PlayerActionsXP { get; set; } = new Dictionary<string, Dictionary<string, double>>();
        public Dictionary<string, Dictionary<string, Dictionary<string, double>>> MobScalingSettings { get; set; } = new Dictionary<string, Dictionary<string, Dictionary<string, double>>>();

        public ServerConfig()
        {
            // Initialize default values
            PartyBonus["mining"] = 0.05;
            PartyBonus["combat"] = 0.05;
            PartyBonus["building"] = 0.05;
            
            // Initialize collections
            SkillModifiers = new Dictionary<string, double>();
            PartyBonus = new Dictionary<string, double>();
            StaticLevels = new List<int> { -1 };
            VeinBlacklist = new List<string>();
            
            // Initialize XP dictionaries with default values
            InitializeDefaultXpSettings();
        }
        
        private void InitializeDefaultXpSettings()
        {
            // These are simplified - in a real implementation, you would need to build the full nested structure
            var dealDamage = new Dictionary<string, Dictionary<string, double>>();
            var receiveDamage = new Dictionary<string, Dictionary<string, double>>();
            
            var genericKill = new Dictionary<string, double>();
            genericKill["combat"] = 1;
            dealDamage["minecraft:generic_kill"] = genericKill;
            
            var magicDamage = new Dictionary<string, double>();
            magicDamage["magic"] = 15;
            dealDamage["#pmmo:magic"] = magicDamage;
            
            DamageXP["DEAL_DAMAGE"] = dealDamage;
            DamageXP["RECEIVE_DAMAGE"] = receiveDamage;
            
            // Similarly initialize other XP dictionaries
            JumpsXP["JUMP Skills and Ratios"] = new Dictionary<string, double> { ["agility"] = 2.5 };
            JumpsXP["SPRINT_JUMP Skills and Ratios"] = new Dictionary<string, double> { ["agility"] = 2.5 };
            JumpsXP["CROUCH_JUMP Skills and Ratios"] = new Dictionary<string, double> { ["agility"] = 2.5 };
            
            PlayerActionsXP["SWIMMING Skills and Ratios"] = new Dictionary<string, double> { ["swimming"] = 100.0 };
            PlayerActionsXP["SPRINTING Skills and Ratios"] = new Dictionary<string, double> { ["agility"] = 100.0 };
        }
    }
}
