using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PMMOEdit
{
    public static class TomlSkillParser
    {
        public static List<Skill> ParseSkills(string tomlContent)
        {
            var skills = new List<Skill>();
            var currentSkill = new Skill();
            bool isInSkill = false;
            bool isInGroupFor = false;
            Dictionary<string, decimal>? groupFor = null;
            
            // Process each line in the TOML
            foreach (var line in tomlContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string trimmedLine = line.Trim();
                
                // Skip empty lines and comments
                if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith("#"))
                    continue;
                    
                // Debug line parsing
                System.Diagnostics.Debug.WriteLine($"Parsing TOML line: {trimmedLine}");
                
                // Check for skill section header [Skills.Entry.skillname]
                if (trimmedLine.StartsWith("[Skills.Entry.") && trimmedLine.EndsWith("]"))
                {
                    // If we were in a skill definition, add the completed skill to the list
                    if (isInSkill && !string.IsNullOrEmpty(currentSkill.Name))
                    {
                        if (groupFor != null && groupFor.Count > 0)
                        {
                            currentSkill.GroupFor = groupFor;
                        }
                        skills.Add(currentSkill);
                    }
                    
                    // Start a new skill
                    currentSkill = new Skill();
                    isInSkill = true;
                    isInGroupFor = false;
                    groupFor = null;
                    
                    // Extract the skill name
                    var match = Regex.Match(trimmedLine, @"\[Skills\.Entry\.([^\]\.]+)\]");
                    if (match.Success)
                    {
                        currentSkill.Name = match.Groups[1].Value;
                    }
                    continue;
                }
                
                // Check for groupFor section
                if (isInSkill && trimmedLine.Contains(".groupFor]"))
                {
                    isInGroupFor = true;
                    groupFor = new Dictionary<string, decimal>();
                    continue;
                }
                
                // Process property assignments
                if (isInSkill && !string.IsNullOrEmpty(trimmedLine) && trimmedLine.Contains("="))
                {
                    string[] parts = trimmedLine.Split('=', 2);
                    if (parts.Length == 2)
                    {
                        string propertyName = parts[0].Trim();
                        string propertyValue = parts[1].Trim();
                        
                        // Handle group skill relationships
                        if (isInGroupFor)
                        {
                            if (decimal.TryParse(propertyValue, out decimal value))
                            {
                                groupFor![propertyName] = value;
                            }
                            continue;
                        }
                        
                        // Handle standard properties
                        switch (propertyName)
                        {
                            case "maxLevel":
                                if (long.TryParse(propertyValue, out long maxLevel))
                                    currentSkill.MaxLevel = maxLevel;
                                break;
                                
                            case "displayGroupName":
                                currentSkill.DisplayGroupName = propertyValue.ToLower() == "true";
                                break;
                                
                            case "useTotalLevels":
                                currentSkill.UseTotalLevels = propertyValue.ToLower() == "true";
                                break;
                                
                            case "color":
                                if (int.TryParse(propertyValue, out int color))
                                    currentSkill.Color = color;
                                break;
                                
                            case "showInList":
                                currentSkill.ShowInList = propertyValue.ToLower() == "true";
                                break;
                                
                            case "icon":
                                // Remove quotes from the string if present
                                currentSkill.Icon = propertyValue.Trim('"');
                                break;
                                
                            case "iconSize":
                                if (int.TryParse(propertyValue, out int iconSize))
                                    currentSkill.IconSize = iconSize;
                                break;
                                
                            case "noAfkPenalty":
                                currentSkill.NoAfkPenalty = propertyValue.ToLower() == "true";
                                break;
                        }
                    }
                }
            }
            
            // Add the last skill if we were processing one
            if (isInSkill && !string.IsNullOrEmpty(currentSkill.Name))
            {
                if (groupFor != null && groupFor.Count > 0)
                {
                    currentSkill.GroupFor = groupFor;
                }
                skills.Add(currentSkill);
            }
            
            return skills;
        }
    }
}
