using System;
using System.Text;
using System.Collections.Generic;
using System.IO;

namespace PMMOEdit.Models
{
    /// <summary>
    /// Simple parser for PMMO server TOML configuration files
    /// </summary>
    public static class TomlServerConfigParser
    {
        /// <summary>
        /// Parses TOML content into a ServerConfig object
        /// </summary>
        public static ServerConfig ParseServerConfig(string tomlContent)
        {
            // Simplified implementation - just return default config
            // In a real implementation, you would parse the TOML content
            return new ServerConfig();
        }
        
        /// <summary>
        /// Generates TOML configuration content from a ServerConfig object
        /// </summary>
        public static string GenerateTomlConfig(ServerConfig config)
        {
            if (config == null)
                return string.Empty;
                
            StringBuilder sb = new StringBuilder();
            
            // General section
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
            
            // Levels section
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
            sb.AppendLine("\t#Replacing the -1 and adding values to this list will set the xp required to advance for each");
            sb.AppendLine("\t#level manually.  Note that the number of level settings you enter into this list");
            sb.AppendLine("\t#will set your max level.  If you only add 10 entries, your max level will be 10.");
            sb.AppendLine("\t#This setting is intended for players/ops who want fine-tune control over their");
            sb.AppendLine("\t#level growth.  use with caution.  ");
            sb.AppendLine("\t#");
            sb.AppendLine("\t#As a technical note, if you enter values that are not greater than their previous");
            sb.AppendLine("\t#value, the entire list will be ignored and revert back to the selected exponential");
            sb.AppendLine("\t#or linear formulaic calculation");
            sb.AppendLine("\tStatic_Levels = [-1]");
            sb.AppendLine("\t#How much experience should players lose when they die?");
            sb.AppendLine("\t#zero is no loss, one is lose everything");
            sb.AppendLine("\t#Range: 0.0 ~ 1.0");
            sb.AppendLine($"\t\"Loss on death\" = {config.LossOnDeath}");
            sb.AppendLine("\t#should loss of experience cross levels?");
            sb.AppendLine("\t#for example, if true, a player with 1 xp above their current level would lose the");
            sb.AppendLine("\t#[Loss on death] percentage of xp and fall below their current level.  However,");
            sb.AppendLine("\t#if false, the player would lose only 1 xp as that would put them at the base xp of their current level");
            sb.AppendLine($"\t\"Lose Levels On Death\" = {config.LoseLevelsOnDeath.ToString().ToLower()}");
            sb.AppendLine("\t#This setting only matters if [Lose Level On Death] is set to false.");
            sb.AppendLine("\t#If this is true the [Loss On Death] applies only to the experience above the current level");
            sb.AppendLine("\t#for example if level 3 is 1000k xp and the player has 1020 and dies.  the player will only lose");
            sb.AppendLine("\t#the [Loss On Death] of the 20 xp above the level's base.");
            sb.AppendLine($"\t\"Lose Only Excess\" = {config.LoseOnlyExcess.ToString().ToLower()}");
            sb.AppendLine("\t#Modifies how much xp is earned.  This is multiplicative to the XP.");
            sb.AppendLine("\t#(Mutually Exclusive to [Skill Modifiers])");
            sb.AppendLine($"\t\"Global Modifier\" = {config.GlobalModifier}");
            sb.AppendLine();
            
            // Skill Modifiers subsection
            sb.AppendLine("\t#Modifies xp gains for specific skills.  This is multiplicative to the XP.");
            sb.AppendLine("\t#(Mutually Exclusive to [Global Modifier])");
            sb.AppendLine("\t[Levels.\"Skill Modifiers\"]");
            if (config.SkillModifiers != null && config.SkillModifiers.Count > 0)
            {
                foreach (var kvp in config.SkillModifiers)
                {
                    sb.AppendLine($"\t\t{kvp.Key} = {kvp.Value}");
                }
            }
            else
            {
                sb.AppendLine("\t\texample_skill = 1.0");
            }
            sb.AppendLine();
            
            // Linear Levels subsection
            sb.AppendLine("\t#Settings for Linear XP configuration");
            sb.AppendLine("\t[Levels.\"LINEAR LEVELS\"]");
            sb.AppendLine("\t\t#what is the base xp to reach level 2 (this + level * xpPerLevel)");
            sb.AppendLine("\t\t#Range: 0 ~ 9223372036854775807");
            sb.AppendLine($"\t\t\"Base XP\" = {config.LinearBaseXP}");
            sb.AppendLine("\t\t#What is the xp increase per level (baseXp + level * this)");
            sb.AppendLine("\t\t#Range: 0.0 ~ 1.7976931348623157E308");
            sb.AppendLine($"\t\t\"Per Level\" = {config.LinearPerLevel}");
            sb.AppendLine();
            
            // Exponential Levels subsection
            sb.AppendLine("\t#Settings for Exponential XP configuration");
            sb.AppendLine("\t[Levels.\"EXPONENTIAL LEVELS\"]");
            sb.AppendLine("\t\t#What is the x in: x * ([Power Base]^([Per Level] * level))");
            sb.AppendLine("\t\t#Range: > 1");
            sb.AppendLine($"\t\t\"Base XP\" = {config.ExpBaseXP}");
            sb.AppendLine("\t\t#What is the x in: [Base XP] * (x^([Per Level] * level))");
            sb.AppendLine("\t\t#Range: 0.0 ~ 1.7976931348623157E308");
            sb.AppendLine($"\t\t\"Power Base\" = {config.ExpPowerBase}");
            sb.AppendLine("\t\t#What is the x in: [Base XP] * ([Power Base]^(x * level))");
            sb.AppendLine("\t\t#Range: 0.0 ~ 1.7976931348623157E308");
            sb.AppendLine($"\t\t\"Per Level\" = {config.ExpPerLevel}");
            sb.AppendLine();
            
            // Requirements section
            sb.AppendLine("#Should requirements apply for the applicable action type");
            sb.AppendLine("[Requirements]");
            sb.AppendLine($"\t\"WEAR Req Enabled\" = {config.WearReqEnabled.ToString().ToLower()}");
            sb.AppendLine($"\t\"USE_ENCHANTMENT Req Enabled\" = {config.UseEnchantmentReqEnabled.ToString().ToLower()}");
            sb.AppendLine($"\t\"TOOL Req Enabled\" = {config.ToolReqEnabled.ToString().ToLower()}");
            sb.AppendLine($"\t\"WEAPON Req Enabled\" = {config.WeaponReqEnabled.ToString().ToLower()}");
            sb.AppendLine($"\t\"USE Req Enabled\" = {config.UseReqEnabled.ToString().ToLower()}");
            sb.AppendLine($"\t\"PLACE Req Enabled\" = {config.PlaceReqEnabled.ToString().ToLower()}");
            sb.AppendLine($"\t\"BREAK Req Enabled\" = {config.BreakReqEnabled.ToString().ToLower()}");
            sb.AppendLine($"\t\"KILL Req Enabled\" = {config.KillReqEnabled.ToString().ToLower()}");
            sb.AppendLine($"\t\"TRAVEL Req Enabled\" = {config.TravelReqEnabled.ToString().ToLower()}");
            sb.AppendLine($"\t\"RIDE Req Enabled\" = {config.RideReqEnabled.ToString().ToLower()}");
            sb.AppendLine($"\t\"TAME Req Enabled\" = {config.TameReqEnabled.ToString().ToLower()}");
            sb.AppendLine($"\t\"BREED Req Enabled\" = {config.BreedReqEnabled.ToString().ToLower()}");
            sb.AppendLine($"\t\"INTERACT Req Enabled\" = {config.InteractReqEnabled.ToString().ToLower()}");
            sb.AppendLine($"\t\"ENTITY_INTERACT Req Enabled\" = {config.EntityInteractReqEnabled.ToString().ToLower()}");
            sb.AppendLine();
            
            // XP_Gains section
            sb.AppendLine("#All settings related to the gain of experience");
            sb.AppendLine("[XP_Gains]");
            sb.AppendLine("\t#how much of the original XP should be awarded when a player breaks a block they placed");
            sb.AppendLine("\t#Range: 0.0 ~ 1.7976931348623157E308");
            sb.AppendLine($"\t\"Reuse Penalty\" = {config.ReusePenalty}");
            sb.AppendLine("\t#Should xp Gains from perks be added onto by configured xp values");
            sb.AppendLine($"\t\"Perks Plus Configs\" = {config.PerksPlusConfigs.ToString().ToLower()}");
            sb.AppendLine();
            
            // XP Event specifics
            sb.AppendLine("\t#Settings related to certain default event XP awards.");
            sb.AppendLine("\t[XP_Gains.Event_XP_Specifics]");
            sb.AppendLine();
            sb.AppendLine("\t\t[XP_Gains.Event_XP_Specifics.Damage]");
            sb.AppendLine();
            sb.AppendLine("\t\t\t#damage dealt and received is defined by the damage type");
            sb.AppendLine("\t\t\t#or damage type tag preceding it.  xp is awarded based on");
            sb.AppendLine("\t\t\t#the value below multiplied by the damage applied.");
            sb.AppendLine("\t\t\t[XP_Gains.Event_XP_Specifics.Damage.DEAL_DAMAGE]");
            sb.AppendLine();
            sb.AppendLine("\t\t\t\t[XP_Gains.Event_XP_Specifics.Damage.DEAL_DAMAGE.\"minecraft:generic_kill\"]");
            sb.AppendLine("\t\t\t\t\tcombat = 1");
            sb.AppendLine();
            sb.AppendLine("\t\t\t\t[XP_Gains.Event_XP_Specifics.Damage.DEAL_DAMAGE.\"#pmmo:magic\"]");
            sb.AppendLine("\t\t\t\t\tmagic = 15");
            sb.AppendLine();
            sb.AppendLine("\t\t\t\t[XP_Gains.Event_XP_Specifics.Damage.DEAL_DAMAGE.\"#pmmo:gun\"]");
            sb.AppendLine("\t\t\t\t\tgunslinging = 1");
            sb.AppendLine();
            sb.AppendLine("\t\t\t\t[XP_Gains.Event_XP_Specifics.Damage.DEAL_DAMAGE.\"minecraft:player_attack\"]");
            sb.AppendLine("\t\t\t\t\tcombat = 1");
            sb.AppendLine();
            sb.AppendLine("\t\t\t\t[XP_Gains.Event_XP_Specifics.Damage.DEAL_DAMAGE.\"#minecraft:is_projectile\"]");
            sb.AppendLine("\t\t\t\t\tarchery = 1");
            sb.AppendLine();
            sb.AppendLine("\t\t\t[XP_Gains.Event_XP_Specifics.Damage.RECEIVE_DAMAGE]");
            sb.AppendLine();
            sb.AppendLine("\t\t\t\t[XP_Gains.Event_XP_Specifics.Damage.RECEIVE_DAMAGE.\"minecraft:generic_kill\"]");
            sb.AppendLine("\t\t\t\t\tendurance = 1");
            sb.AppendLine();
            sb.AppendLine("\t\t\t\t[XP_Gains.Event_XP_Specifics.Damage.RECEIVE_DAMAGE.\"#pmmo:impact\"]");
            sb.AppendLine("\t\t\t\t\tendurance = 15");
            sb.AppendLine();
            sb.AppendLine("\t\t\t\t[XP_Gains.Event_XP_Specifics.Damage.RECEIVE_DAMAGE.\"#pmmo:magic\"]");
            sb.AppendLine("\t\t\t\t\tmagic = 15");
            sb.AppendLine();
            sb.AppendLine("\t\t\t\t[XP_Gains.Event_XP_Specifics.Damage.RECEIVE_DAMAGE.\"#pmmo:environment\"]");
            sb.AppendLine("\t\t\t\t\tendurance = 10");
            sb.AppendLine();
            sb.AppendLine("\t\t\t\t[XP_Gains.Event_XP_Specifics.Damage.RECEIVE_DAMAGE.\"#minecraft:is_projectile\"]");
            sb.AppendLine("\t\t\t\t\tendurance = 15");
            sb.AppendLine();
            sb.AppendLine("\t\t[XP_Gains.Event_XP_Specifics.Jumps]");
            sb.AppendLine();
            sb.AppendLine("\t\t\t[XP_Gains.Event_XP_Specifics.Jumps.\"JUMP Skills and Ratios\"]");
            sb.AppendLine("\t\t\t\tagility = 2.5");
            sb.AppendLine();
            sb.AppendLine("\t\t\t[XP_Gains.Event_XP_Specifics.Jumps.\"SPRINT_JUMP Skills and Ratios\"]");
            sb.AppendLine("\t\t\t\tagility = 2.5");
            sb.AppendLine();
            sb.AppendLine("\t\t\t[XP_Gains.Event_XP_Specifics.Jumps.\"CROUCH_JUMP Skills and Ratios\"]");
            sb.AppendLine("\t\t\t\tagility = 2.5");
            sb.AppendLine();
            sb.AppendLine("\t\t[XP_Gains.Event_XP_Specifics.Player_Actions]");
            sb.AppendLine();
            sb.AppendLine("\t\t\t[XP_Gains.Event_XP_Specifics.Player_Actions.\"BREATH_CHANGE Skills and Ratios\"]");
            sb.AppendLine("\t\t\t\tswimming = 1.0");
            sb.AppendLine();
            sb.AppendLine("\t\t\t[XP_Gains.Event_XP_Specifics.Player_Actions.\"HEALTH_CHANGE Skills and Ratios\"]");
            sb.AppendLine("\t\t\t\tendurance = 0.0");
            sb.AppendLine();
            sb.AppendLine("\t\t\t[XP_Gains.Event_XP_Specifics.Player_Actions.\"HEALTH_INCREASE Skills and Ratios\"]");
            sb.AppendLine("\t\t\t\tendurance = 1.0");
            sb.AppendLine();
            sb.AppendLine("\t\t\t[XP_Gains.Event_XP_Specifics.Player_Actions.\"HEALTH_DECREASE Skills and Ratios\"]");
            sb.AppendLine("\t\t\t\tendurance = 1.0");
            sb.AppendLine();
            sb.AppendLine("\t\t\t[XP_Gains.Event_XP_Specifics.Player_Actions.\"SPRINTING Skills and Ratios\"]");
            sb.AppendLine("\t\t\t\tagility = 100.0");
            sb.AppendLine();
            sb.AppendLine("\t\t\t[XP_Gains.Event_XP_Specifics.Player_Actions.\"SUBMERGED Skills and Ratios\"]");
            sb.AppendLine("\t\t\t\tswimming = 1.0");
            sb.AppendLine();
            sb.AppendLine("\t\t\t[XP_Gains.Event_XP_Specifics.Player_Actions.\"SWIMMING Skills and Ratios\"]");
            sb.AppendLine("\t\t\t\tswimming = 100.0");
            sb.AppendLine();
            sb.AppendLine("\t\t\t[XP_Gains.Event_XP_Specifics.Player_Actions.\"DIVING Skills and Ratios\"]");
            sb.AppendLine("\t\t\t\tswimming = 150.0");
            sb.AppendLine();
            sb.AppendLine("\t\t\t[XP_Gains.Event_XP_Specifics.Player_Actions.\"SURFACING Skills and Ratios\"]");
            sb.AppendLine("\t\t\t\tswimming = 50.0");
            sb.AppendLine();
            sb.AppendLine("\t\t\t[XP_Gains.Event_XP_Specifics.Player_Actions.\"SWIM_SPRINTING Skills and Ratios\"]");
            sb.AppendLine("\t\t\t\tswimming = 200.0");
            sb.AppendLine();
            
            // Party section
            sb.AppendLine("#All settings governing party behavior");
            sb.AppendLine("[Party]");
            sb.AppendLine("\t#How close do party members have to be to share experience.");
            sb.AppendLine("\t#Range: > 0");
            sb.AppendLine($"\t\"Party Range\" = {config.PartyRange}");
            sb.AppendLine();
            
            // Party Bonus subsection
            sb.AppendLine("\t#How much bonus xp should parties earn by skill.");
            sb.AppendLine("\t#This value is multiplied by the party size.");
            sb.AppendLine("\t[Party.\"Party Bonus\"]");
            if (config.PartyBonus != null && config.PartyBonus.Count > 0)
            {
                foreach (var kvp in config.PartyBonus)
                {
                    sb.AppendLine($"\t\t{kvp.Key} = {kvp.Value}");
                }
            }
            else
            {
                sb.AppendLine("\t\tcombat = 1.05");
                sb.AppendLine("\t\tendurance = 1.1");
            }
            sb.AppendLine();
            
            // Mob_Scaling section
            sb.AppendLine("#settings related to how strong mobs get based on player level.");
            sb.AppendLine("[Mob_Scaling]");
            sb.AppendLine("\t#Should mob scaling be turned on.");
            sb.AppendLine($"\t\"Enable Mob Scaling\" = {config.EnableMobScaling.ToString().ToLower()}");
            sb.AppendLine("\t#How far should players be from spawning mobs to affect scaling?");
            sb.AppendLine("\t#Range: > 0");
            sb.AppendLine($"\t\"Scaling AOE\" = {config.ScalingAOE}");
            sb.AppendLine("\t#what is the minimum level for scaling to kick in");
            sb.AppendLine("\t#Range: > 0");
            sb.AppendLine($"\t\"Base Level\" = {config.BaseLevel}");
            sb.AppendLine("\t#a multiplier on top of final scaling values that");
            sb.AppendLine("\t#applies only to entities in the forge:bosses tag.");
            sb.AppendLine($"\tboss_scaling = {config.BossScaling}");
            sb.AppendLine();
            
            // Mob Scaling Formula
            sb.AppendLine("\t#How should mob attributes be calculated with respect to the player's level.");
            sb.AppendLine("\t[Mob_Scaling.Formula]");
            sb.AppendLine("\t\t#should levels be determined using an exponential formula?");
            sb.AppendLine($"\t\t\"Use Exponential Formula\" = {config.MobScalingUseExponentialFormula.ToString().ToLower()}");
            sb.AppendLine();
            
            // Linear scaling configuration
            sb.AppendLine("\t\t#Settings for Linear scaling configuration");
            sb.AppendLine("\t\t[Mob_Scaling.Formula.LINEAR_LEVELS]");
            sb.AppendLine("\t\t\t#What is the xp increase per level ((level - base_level) * this)");
            sb.AppendLine("\t\t\t#Range: 0.0 ~ 1.7976931348623157E308");
            sb.AppendLine($"\t\t\t\"Per Level\" = {config.MobScalingLinearPerLevel}");
            sb.AppendLine();
            
            // Exponential scaling configuration
            sb.AppendLine("\t\t#Settings for Exponential scaling configuration");
            sb.AppendLine("\t\t[Mob_Scaling.Formula.EXPONENTIAL_LEVELS]");
            sb.AppendLine("\t\t\t#What is the x in: (x^([Per Level] * level))");
            sb.AppendLine("\t\t\t#Range: 0.0 ~ 1.7976931348623157E308");
            sb.AppendLine($"\t\t\t\"Power Base\" = {config.MobScalingExpPowerBase}");
            sb.AppendLine("\t\t\t#What is the x in: ([Power Base]^(x * level))");
            sb.AppendLine("\t\t\t#Range: 0.0 ~ 1.7976931348623157E308");
            sb.AppendLine($"\t\t\t\"Per Level\" = {config.MobScalingExpPerLevel}");
            sb.AppendLine();
            
            // Mob Scaling Settings
            sb.AppendLine("\t#These settings control which skills affect scaling and the ratio for each skill");
            sb.AppendLine("\t#minecraft:generic.max_health: 1 = half a heart, or 1 hitpoint");
            sb.AppendLine("\t#minecraft:generic.movement_speed: 0.7 is base for most mobs.  this is added to that. so 0.7 from scaling is double speed");
            sb.AppendLine("\t#minecraft:generic.attack_damage: is a multiplier of their base damage.  1 = no change, 2 = double damage");
            sb.AppendLine("\t#negative values are possible and you can use this to create counterbalance skills");
            sb.AppendLine("\t#");
            sb.AppendLine("\t#NOTE: TOML WILL MOVE THE QUOTATIONS OF YOUR ATTRIBUTE ID AND BREAK YOUR CONFIG.");
            sb.AppendLine("\t#ENSURE YOU HAVE FORCIBLY PUT YOUR QUOTES AROUND YOUR ATTRIBUTE ID BEFORE SAVING.");
            sb.AppendLine("\t[Mob_Scaling.Scaling_Settings]");
            sb.AppendLine();
            sb.AppendLine("\t\t[Mob_Scaling.Scaling_Settings.\"Mob Scaling IDs and Ratios\"]");
            sb.AppendLine();
            sb.AppendLine("\t\t\t[Mob_Scaling.Scaling_Settings.\"Mob Scaling IDs and Ratios\".\"minecraft:generic\"]");
            sb.AppendLine();
            sb.AppendLine("\t\t\t\t[Mob_Scaling.Scaling_Settings.\"Mob Scaling IDs and Ratios\".\"minecraft:generic\".max_health]");
            sb.AppendLine("\t\t\t\t\tcombat = 0.001");
            sb.AppendLine();
            sb.AppendLine("\t\t\t\t[Mob_Scaling.Scaling_Settings.\"Mob Scaling IDs and Ratios\".\"minecraft:generic\".attack_damage]");
            sb.AppendLine("\t\t\t\t\tcombat = 1.0E-4");
            sb.AppendLine();
            sb.AppendLine("\t\t\t\t[Mob_Scaling.Scaling_Settings.\"Mob Scaling IDs and Ratios\".\"minecraft:generic\".movement_speed]");
            sb.AppendLine("\t\t\t\t\tcombat = 1.0E-6");
            sb.AppendLine();
            
            // Vein_Miner section
            sb.AppendLine("#Settings related to the Vein Miner");
            sb.AppendLine("[Vein_Miner]");
            sb.AppendLine("\t#setting to false disables all vein features");
            sb.AppendLine($"\t\"vein enabled\" = {config.veinEnabled.ToString().ToLower()}");
            sb.AppendLine("\t#If true, default consume will be ignored in favor of only allowing");
            sb.AppendLine("\t#veining blocks with declared values.");
            sb.AppendLine($"\t\"Require Settings\" = {config.RequireSettings.ToString().ToLower()}");
            sb.AppendLine("\t#how much a block should consume if no setting is defined.");
            sb.AppendLine($"\t\"Vein Mine Default Consume\" = {config.VeinMineDefaultConsume}");
            sb.AppendLine("\t#a multiplier to all vein charge rates.");
            sb.AppendLine("\t#Range: 0.0 ~ 1.7976931348623157E308");
            sb.AppendLine($"\t\"Vein Charge Modifier\" = {config.VeinChargeModifier}");
            
            // Vein Blacklist
            sb.AppendLine("\t#Tools in this list do not cause the vein miner to trigger");
            sb.Append("\tVein_Blacklist = [");
            if (config.VeinBlacklist != null && config.VeinBlacklist.Count > 0)
            {
                for (int i = 0; i < config.VeinBlacklist.Count; i++)
                {
                    if (i > 0) sb.Append(", ");
                    sb.Append($"\"{config.VeinBlacklist[i]}\"");
                }
            }
            else
            {
                sb.Append("\"silentgear:saw\"");
            }
            sb.AppendLine("]");
            
            sb.AppendLine("\t#A constant charge rate given to all players regardless of equipment.");
            sb.AppendLine("\t#Items worn will add to this amount, not replace it.");
            sb.AppendLine("\t#Range: 0.0 ~ 1.7976931348623157E308");
            sb.AppendLine($"\t\"base charge rate\" = {config.BaseChargeRate}");
            sb.AppendLine("\t#A minimum capacity given to all players regardless of equipment.");
            sb.AppendLine("\t#Items worn will add to this amount, not replace it.");
            sb.AppendLine("\t#Range: > 0");
            sb.AppendLine($"\t\"base vein capacity\" = {config.BaseVeinCapacity}");
            
            return sb.ToString();
        }
    }
}
