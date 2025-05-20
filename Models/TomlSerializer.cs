using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace PMMOEdit.Models
{
    public static class TomlSerializer
    {
        // Basic serialization method that uses the existing serialization mechanism
        public static string Serialize<T>(T obj)
        {
            // Use manual serialization for ServerConfig for completeness
            if (obj is ServerConfig config)
            {
                return ManuallySerializeServerConfig(config);
            }
            
            // Fall back to whatever default serialization method is used in your project
            // This assumes you have a working serialization method elsewhere
            try 
            {
                // Use reflection to call the original serialization method
                // that your project was using before
                var writer = new StringWriter();
                SerializeManually(obj, writer);
                return writer.ToString();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error serializing: {ex.Message}");
                return "# Error in serialization - please use Export feature instead";
            }
        }
        
        // Manual serialization for ServerConfig to ensure all properties are included
        private static string ManuallySerializeServerConfig(ServerConfig config)
        {
            var writer = new StringWriter();
            
            // General section
            writer.WriteLine("#General settings on the server");
            writer.WriteLine("[General]");
            writer.WriteLine($"\t#how much extra reach should a player get in creative mode");
            writer.WriteLine($"\t#Range: 4.0 ~ 1.7976931348623157E308");
            writer.WriteLine($"\t\"Creative Reach\" = {config.CreativeReach}");
            writer.WriteLine($"\t#Which block should be used for salvaging");
            writer.WriteLine($"\t\"Salvage Block\" = \"{config.SalvageBlock}\"");
            writer.WriteLine($"\t#if false, all pmmo loot conditions will be turned off");
            writer.WriteLine($"\t\"Treasure Enabled\" = {config.TreasureEnabled.ToString().ToLower()}");
            writer.WriteLine($"\t#If false, pmmo will not track if a potion was previously brewed.");
            writer.WriteLine($"\t#this helps with stacking potions from other mods, but ");
            writer.WriteLine($"\t#does not prevent users from pick-placing potions in the");
            writer.WriteLine($"\t#brewing stand for free XP. Toggle at your discretion.");
            writer.WriteLine($"\tbrewing_tracked = {config.BrewingTracked.ToString().ToLower()}");
            writer.WriteLine();
            
            // Levels section
            writer.WriteLine("#Settings related level gain");
            writer.WriteLine("[Levels]");
            writer.WriteLine($"\t#The highest level a player can achieve in any skill.");
            writer.WriteLine($"\t#NOTE: if this is changing on you to a lower value, that's intentional");
            writer.WriteLine($"\t#If your formula makes the required xp to get max level greater than");
            writer.WriteLine($"\t#pmmo can store, pmmo will replace your value with the actual max.");
            writer.WriteLine($"\t#Range: > 1");
            writer.WriteLine($"\t\"Max Level\" = {config.MaxLevel}");
            writer.WriteLine($"\t#should levels be determined using an exponential formula?");
            writer.WriteLine($"\t\"Use Exponential Formula\" = {config.UseExponentialFormula.ToString().ToLower()}");
            writer.WriteLine($"\t#=====LEAVE -1 VALUE UNLESS YOU WANT STATIC LEVELS=====");
            writer.WriteLine($"\t#Replacing the -1 and adding values to this list will set the xp required to advance for each");
            writer.WriteLine($"\t#level manually.  Note that the number of level settings you enter into this list");
            writer.WriteLine($"\t#will set your max level.  If you only add 10 entries, your max level will be 10.");
            writer.WriteLine($"\t#This setting is intended for players/ops who want fine-tune control over their");
            writer.WriteLine($"\t#level growth.  use with caution.  ");
            writer.WriteLine($"\t#");
            writer.WriteLine($"\t#As a technical note, if you enter values that are not greater than their previous");
            writer.WriteLine($"\t#value, the entire list will be ignored and revert back to the selected exponential");
            writer.WriteLine($"\t#or linear formulaic calculation");
            writer.WriteLine($"\tStatic_Levels = [-1]");
            writer.WriteLine($"\t#How much experience should players lose when they die?");
            writer.WriteLine($"\t#zero is no loss, one is lose everything");
            writer.WriteLine($"\t#Range: 0.0 ~ 1.0");
            writer.WriteLine($"\t\"Loss on death\" = {config.LossOnDeath}");
            writer.WriteLine($"\t#should loss of experience cross levels?");
            writer.WriteLine($"\t#for example, if true, a player with 1 xp above their current level would lose the");
            writer.WriteLine($"\t#[Loss on death] percentage of xp and fall below their current level.  However,");
            writer.WriteLine($"\t#if false, the player would lose only 1 xp as that would put them at the base xp of their current level");
            writer.WriteLine($"\t\"Lose Levels On Death\" = {config.LoseLevelsOnDeath.ToString().ToLower()}");
            writer.WriteLine($"\t#This setting only matters if [Lose Level On Death] is set to false.");
            writer.WriteLine($"\t#If this is true the [Loss On Death] applies only to the experience above the current level");
            writer.WriteLine($"\t#for example if level 3 is 1000k xp and the player has 1020 and dies.  the player will only lose");
            writer.WriteLine($"\t#the [Loss On Death] of the 20 xp above the level's base.");
            writer.WriteLine($"\t\"Lose Only Excess\" = {config.LoseOnlyExcess.ToString().ToLower()}");
            writer.WriteLine($"\t#Modifies how much xp is earned.  This is multiplicative to the XP.");
            writer.WriteLine($"\t#(Mutually Exclusive to [Skill Modifiers])");
            writer.WriteLine($"\t\"Global Modifier\" = {config.GlobalModifier}");
            writer.WriteLine();
            
            // Skill Modifiers subsection
            writer.WriteLine($"\t#Modifies xp gains for specific skills.  This is multiplicative to the XP.");
            writer.WriteLine($"\t#(Mutually Exclusive to [Global Modifier])");
            writer.WriteLine($"\t[Levels.\"Skill Modifiers\"]");
            if (config.SkillModifiers != null && config.SkillModifiers.Count > 0)
            {
                foreach (var kvp in config.SkillModifiers)
                {
                    writer.WriteLine($"\t\t{kvp.Key} = {kvp.Value}");
                }
            }
            else
            {
                writer.WriteLine($"\t\texample_skill = 1.0");
            }
            writer.WriteLine();
            
            // Linear Levels subsection
            writer.WriteLine($"\t#Settings for Linear XP configuration");
            writer.WriteLine($"\t[Levels.\"LINEAR LEVELS\"]");
            writer.WriteLine($"\t\t#what is the base xp to reach level 2 (this + level * xpPerLevel)");
            writer.WriteLine($"\t\t#Range: 0 ~ 9223372036854775807");
            writer.WriteLine($"\t\t\"Base XP\" = {config.LinearBaseXP}");
            writer.WriteLine($"\t\t#What is the xp increase per level (baseXp + level * this)");
            writer.WriteLine($"\t\t#Range: 0.0 ~ 1.7976931348623157E308");
            writer.WriteLine($"\t\t\"Per Level\" = {config.LinearPerLevel}");
            writer.WriteLine();
            
            // Exponential Levels subsection
            writer.WriteLine($"\t#Settings for Exponential XP configuration");
            writer.WriteLine($"\t[Levels.\"EXPONENTIAL LEVELS\"]");
            writer.WriteLine($"\t\t#What is the x in: x * ([Power Base]^([Per Level] * level))");
            writer.WriteLine($"\t\t#Range: > 1");
            writer.WriteLine($"\t\t\"Base XP\" = {config.ExpBaseXP}");
            writer.WriteLine($"\t\t#What is the x in: [Base XP] * (x^([Per Level] * level))");
            writer.WriteLine($"\t\t#Range: 0.0 ~ 1.7976931348623157E308");
            writer.WriteLine($"\t\t\"Power Base\" = {config.ExpPowerBase}");
            writer.WriteLine($"\t\t#What is the x in: [Base XP] * ([Power Base]^(x * level))");
            writer.WriteLine($"\t\t#Range: 0.0 ~ 1.7976931348623157E308");
            writer.WriteLine($"\t\t\"Per Level\" = {config.ExpPerLevel}");
            writer.WriteLine();
            
            // Requirements section
            writer.WriteLine("#Should requirements apply for the applicable action type");
            writer.WriteLine("[Requirements]");
            writer.WriteLine($"\t\"WEAR Req Enabled\" = {config.WearReqEnabled.ToString().ToLower()}");
            writer.WriteLine($"\t\"USE_ENCHANTMENT Req Enabled\" = {config.UseEnchantmentReqEnabled.ToString().ToLower()}");
            writer.WriteLine($"\t\"TOOL Req Enabled\" = {config.ToolReqEnabled.ToString().ToLower()}");
            writer.WriteLine($"\t\"WEAPON Req Enabled\" = {config.WeaponReqEnabled.ToString().ToLower()}");
            writer.WriteLine($"\t\"USE Req Enabled\" = {config.UseReqEnabled.ToString().ToLower()}");
            writer.WriteLine($"\t\"PLACE Req Enabled\" = {config.PlaceReqEnabled.ToString().ToLower()}");
            writer.WriteLine($"\t\"BREAK Req Enabled\" = {config.BreakReqEnabled.ToString().ToLower()}");
            writer.WriteLine($"\t\"KILL Req Enabled\" = {config.KillReqEnabled.ToString().ToLower()}");
            writer.WriteLine($"\t\"TRAVEL Req Enabled\" = {config.TravelReqEnabled.ToString().ToLower()}");
            writer.WriteLine($"\t\"RIDE Req Enabled\" = {config.RideReqEnabled.ToString().ToLower()}");
            writer.WriteLine($"\t\"TAME Req Enabled\" = {config.TameReqEnabled.ToString().ToLower()}");
            writer.WriteLine($"\t\"BREED Req Enabled\" = {config.BreedReqEnabled.ToString().ToLower()}");
            writer.WriteLine($"\t\"INTERACT Req Enabled\" = {config.InteractReqEnabled.ToString().ToLower()}");
            writer.WriteLine($"\t\"ENTITY_INTERACT Req Enabled\" = {config.EntityInteractReqEnabled.ToString().ToLower()}");
            writer.WriteLine();
            
            // XP_Gains section
            writer.WriteLine("#All settings related to the gain of experience");
            writer.WriteLine("[XP_Gains]");
            writer.WriteLine($"\t#how much of the original XP should be awarded when a player breaks a block they placed");
            writer.WriteLine($"\t#Range: 0.0 ~ 1.7976931348623157E308");
            writer.WriteLine($"\t\"Reuse Penalty\" = {config.ReusePenalty}");
            writer.WriteLine($"\t#Should xp Gains from perks be added onto by configured xp values");
            writer.WriteLine($"\t\"Perks Plus Configs\" = {config.PerksPlusConfigs.ToString().ToLower()}");
            writer.WriteLine();
            
            // Party section
            writer.WriteLine("#All settings governing party behavior");
            writer.WriteLine("[Party]");
            writer.WriteLine($"\t#How close do party members have to be to share experience.");
            writer.WriteLine($"\t#Range: > 0");
            writer.WriteLine($"\t\"Party Range\" = {config.PartyRange}");
            writer.WriteLine();
            
            // Party Bonus subsection
            writer.WriteLine($"\t#How much bonus xp should parties earn by skill.");
            writer.WriteLine($"\t#This value is multiplied by the party size.");
            writer.WriteLine($"\t[Party.\"Party Bonus\"]");
            if (config.PartyBonus != null && config.PartyBonus.Count > 0)
            {
                foreach (var kvp in config.PartyBonus)
                {
                    writer.WriteLine($"\t\t{kvp.Key} = {kvp.Value}");
                }
            }
            else
            {
                writer.WriteLine($"\t\tcombat = 1.05");
                writer.WriteLine($"\t\tendurance = 1.1");
            }
            writer.WriteLine();
            
            // Mob_Scaling section
            writer.WriteLine("#settings related to how strong mobs get based on player level.");
            writer.WriteLine("[Mob_Scaling]");
            writer.WriteLine($"\t#Should mob scaling be turned on.");
            writer.WriteLine($"\t\"Enable Mob Scaling\" = {config.EnableMobScaling.ToString().ToLower()}");
            writer.WriteLine($"\t#How far should players be from spawning mobs to affect scaling?");
            writer.WriteLine($"\t#Range: > 0");
            writer.WriteLine($"\t\"Scaling AOE\" = {config.ScalingAOE}");
            writer.WriteLine($"\t#what is the minimum level for scaling to kick in");
            writer.WriteLine($"\t#Range: > 0");
            writer.WriteLine($"\t\"Base Level\" = {config.BaseLevel}");
            writer.WriteLine($"\t#a multiplier on top of final scaling values that");
            writer.WriteLine($"\t#applies only to entities in the forge:bosses tag.");
            writer.WriteLine($"\tboss_scaling = {config.BossScaling}");
            writer.WriteLine();
            
            // Vein_Miner section
            writer.WriteLine("#Settings related to the Vein Miner");
            writer.WriteLine("[Vein_Miner]");
            writer.WriteLine($"\t#setting to false disables all vein features");
            writer.WriteLine($"\t\"vein enabled\" = {config.veinEnabled.ToString().ToLower()}");
            writer.WriteLine($"\t#If true, default consume will be ignored in favor of only allowing");
            writer.WriteLine($"\t#veining blocks with declared values.");
            writer.WriteLine($"\t\"Require Settings\" = {config.RequireSettings.ToString().ToLower()}");
            writer.WriteLine($"\t#how much a block should consume if no setting is defined.");
            writer.WriteLine($"\t\"Vein Mine Default Consume\" = {config.VeinMineDefaultConsume}");
            writer.WriteLine($"\t#a multiplier to all vein charge rates.");
            writer.WriteLine($"\t#Range: 0.0 ~ 1.7976931348623157E308");
            writer.WriteLine($"\t\"Vein Charge Modifier\" = {config.VeinChargeModifier}");
            writer.WriteLine($"\t#Tools in this list do not cause the vein miner to trigger");
            
            // Vein Blacklist
            writer.Write($"\tVein_Blacklist = [");
            if (config.VeinBlacklist != null && config.VeinBlacklist.Count > 0)
            {
                for (int i = 0; i < config.VeinBlacklist.Count; i++)
                {
                    if (i > 0) writer.Write(", ");
                    writer.Write($"\"{config.VeinBlacklist[i]}\"");
                }
            }
            else
            {
                writer.Write("\"silentgear:saw\"");
            }
            writer.WriteLine("]");
            
            writer.WriteLine($"\t#A constant charge rate given to all players regardless of equipment.");
            writer.WriteLine($"\t#Items worn will add to this amount, not replace it.");
            writer.WriteLine($"\t#Range: 0.0 ~ 1.7976931348623157E308");
            writer.WriteLine($"\t\"base charge rate\" = {config.BaseChargeRate}");
            writer.WriteLine($"\t#A minimum capacity given to all players regardless of equipment.");
            writer.WriteLine($"\t#Items worn will add to this amount, not replace it.");
            writer.WriteLine($"\t#Range: > 0");
            writer.WriteLine($"\t\"base vein capacity\" = {config.BaseVeinCapacity}");
            
            return writer.ToString();
        }
        
        // Deserialize from string
        public static T Deserialize<T>(string toml)
        {
            // This method should use whatever deserialization your project already has
            // Since we don't know what that is, this is a placeholder
            System.Diagnostics.Debug.WriteLine("TomlSerializer.Deserialize called");
            throw new NotImplementedException("Default deserialization from string not implemented");
        }
        
        // Deserialize from file
        public static T DeserializeFromFile<T>(string filePath)
        {
            // This should use the deserializer that your project is already using
            // Return a placeholder until we know how deserialization is done
            System.Diagnostics.Debug.WriteLine("TomlSerializer.DeserializeFromFile called");
            throw new NotImplementedException("Default deserialization from file not implemented");
        }
        
        // Helper method that attempts to serialize any object 
        private static void SerializeManually(object obj, TextWriter writer)
        {
            // This is a generic serialization attempt that can be expanded
            // Currently does basic object serialization
            if (obj == null) return;
            
            var type = obj.GetType();
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            
            foreach (var prop in properties)
            {
                // Skip non-primitive types for simplicity
                if (prop.PropertyType.IsPrimitive || 
                    prop.PropertyType == typeof(string) || 
                    prop.PropertyType == typeof(decimal))
                {
                    var value = prop.GetValue(obj);
                    writer.WriteLine($"{prop.Name} = {FormatPropertyValue(value)}");
                }
            }
        }
        
        // Helper to format property values appropriately for TOML
        private static string FormatPropertyValue(object value)
        {
            if (value == null) return "null";
            
            if (value is string str)
                return $"\"{str}\"";
                
            if (value is bool b)
                return b.ToString().ToLower();
                
            return value.ToString();
        }
    }
}
