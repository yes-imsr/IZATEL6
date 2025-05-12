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
            

            foreach (var line in tomlContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string trimmedLine = line.Trim();
                if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith("#"))
                    continue;
                
                System.Diagnostics.Debug.WriteLine($"Parsing TOML line: {trimmedLine}");
                if (trimmedLine.StartsWith("[Skills.Entry.") && trimmedLine.EndsWith("]"))
                {
                    if (isInSkill && !string.IsNullOrEmpty(currentSkill.Name))
                    {
                        if (groupFor != null && groupFor.Count > 0)
                        {
                            currentSkill.GroupFor = groupFor;
                        }
                        skills.Add(currentSkill);
                    }
                    
                    currentSkill = new Skill();
                    isInSkill = true;
                    isInGroupFor = false;
                    groupFor = null;
                    var match = Regex.Match(trimmedLine, @"\[Skills\.Entry\.([^\]\.]+)\]");
                    if (match.Success)
                    {
                        currentSkill.Name = match.Groups[1].Value;
                    }
                    continue;
                }
                
                if (isInSkill && trimmedLine.Contains(".groupFor]"))
                {
                    isInGroupFor = true;
                    groupFor = new Dictionary<string, decimal>();
                    continue;
                }
                
                if (isInSkill && !string.IsNullOrEmpty(trimmedLine) && trimmedLine.Contains("="))
                {
                    string[] parts = trimmedLine.Split('=', 2);
                    if (parts.Length == 2)
                    {
                        string propertyName = parts[0].Trim();
                        string propertyValue = parts[1].Trim();
                        
                        if (isInGroupFor)
                        {
                            if (decimal.TryParse(propertyValue, out decimal value))
                            {
                                groupFor![propertyName] = value;
                            }
                            continue;
                        }
                        
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
