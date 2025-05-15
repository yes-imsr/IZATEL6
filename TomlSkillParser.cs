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
            var lines = tomlContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            
            Skill? currentSkill = null;
            bool inGroupForSection = false;
            
            foreach (var line in lines)
            {
                string trimmedLine = line.Trim();
                
                if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith('#'))
                    continue;
                
                var skillMatch = Regex.Match(trimmedLine, @"\[Skills\.Entry\.([^\]]+)\]");
                if (skillMatch.Success)
                {
                    inGroupForSection = false;
                    
                    if (currentSkill != null)
                        skills.Add(currentSkill);
                    
                    string skillName = skillMatch.Groups[1].Value;
                    currentSkill = new Skill { Name = skillName };
                    continue;
                }
                
                if (trimmedLine.Contains(".groupFor]"))
                {
                    inGroupForSection = true;
                    if (currentSkill != null && currentSkill.GroupFor == null)
                        currentSkill.GroupFor = new Dictionary<string, decimal>();
                    continue;
                                    }
                
                                    if (currentSkill == null)
                    continue;
                                    
                                    if (inGroupForSection)
                                    {
                    var groupForMatch = Regex.Match(trimmedLine, @"([^ =]+)\s*=\s*([0-9.]+)");
                    if (groupForMatch.Success && currentSkill.GroupFor != null)
                    {
                        string subSkillName = groupForMatch.Groups[1].Value.Trim();
                        decimal value = decimal.Parse(groupForMatch.Groups[2].Value);
                        currentSkill.GroupFor[subSkillName] = value;
                    }
                    continue;
                }
                                    
                var propMatch = Regex.Match(trimmedLine, @"([^ =]+)\s*=\s*(.+)");
                if (propMatch.Success)
                {
                    string propName = propMatch.Groups[1].Value.Trim();
                    string propValue = propMatch.Groups[2].Value.Trim();
                    
                    switch (propName)
                    {
                        case "maxLevel":
                            currentSkill.MaxLevel = int.Parse(propValue);
                            break;
                        case "displayGroupName":
                            currentSkill.DisplayGroupName = bool.Parse(propValue);
                            break;
                        case "useTotalLevels":
                            currentSkill.UseTotalLevels = bool.Parse(propValue);
                            break;
                        case "color":
                            currentSkill.Color = int.Parse(propValue);
                            currentSkill.ColorHex = $"#{currentSkill.Color:X6}";
                            break;
                        case "showInList":
                            currentSkill.ShowInList = bool.Parse(propValue);
                            break;
                        case "icon":
                            currentSkill.Icon = propValue.Trim('"');
                            break;
                        case "iconSize":
                            currentSkill.IconSize = int.Parse(propValue);
                            break;
                        case "noAfkPenalty":
                            currentSkill.NoAfkPenalty = bool.Parse(propValue);
                            break;
                    }
                }
            }
            
            if (currentSkill != null)
                skills.Add(currentSkill);
            
            return skills;
        }
    }
}

