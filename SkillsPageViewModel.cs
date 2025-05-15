using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PMMOEdit
{
    public class SkillsPageViewModel : INotifyPropertyChanged
    {
        private Skill? _selectedSkill;
        private string? _currentFilePath;
        
        public ObservableCollection<Skill> Skills { get; } = new();
        
        public string? CurrentFilePath
        {
            get => _currentFilePath;
            set
            {
                if (_currentFilePath != value)
                {
                    _currentFilePath = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(HasOpenedFile));
                }
            }
        }
        
        public bool HasOpenedFile => !string.IsNullOrEmpty(CurrentFilePath);
        
        public Skill? SelectedSkill
        {
            get => _selectedSkill;
            set
            {
                if (_selectedSkill != value)
                {
                    _selectedSkill = value;
                    OnPropertyChanged();
                }
            }
        }

        public SkillsPageViewModel()
        {
            LoadDefaultSkills();
        }

        public void LoadDefaultSkills()
        {
            Skills.Clear();
            string defaultSkillsToml = @"
[Skills.Entry.magic]
    maxLevel = 2147483647
    displayGroupName = false
    useTotalLevels = false
    color = 255
    showInList = true
    icon = ""minecraft:textures/particle/enchanted_hit.png""
    iconSize = 8
    noAfkPenalty = false

[Skills.Entry.slayer]
    maxLevel = 2147483647
    displayGroupName = false
    useTotalLevels = false
    color = 16777215
    showInList = true
    icon = ""minecraft:textures/item/netherite_sword.png""
    iconSize = 16
    noAfkPenalty = false

[Skills.Entry.fishing]
    maxLevel = 2147483647
    displayGroupName = false
    useTotalLevels = false
    color = 52479
    showInList = true
    icon = ""minecraft:textures/item/fishing_rod.png""
    iconSize = 16
    noAfkPenalty = false

[Skills.Entry.fightgroup]
    maxLevel = 2147483647
    displayGroupName = false
    useTotalLevels = false
    color = 16777215
    showInList = true
    icon = ""pmmo:textures/skills/missing_icon.png""
    iconSize = 18
    noAfkPenalty = false

    [Skills.Entry.fightgroup.groupFor]
        combat = 0.5
        endurance = 0.3
        archery = 0.2

[Skills.Entry.combat]
    maxLevel = 2147483647
    displayGroupName = false
    useTotalLevels = false
    color = 16724736
    showInList = true
    icon = ""minecraft:textures/mob_effect/strength.png""
    iconSize = 18
    noAfkPenalty = false

[Skills.Entry.alchemy]
    maxLevel = 2147483647
    displayGroupName = false
    useTotalLevels = false
    color = 15112448
    showInList = true
    icon = ""minecraft:textures/item/potion.png""
    iconSize = 16
    noAfkPenalty = true

[Skills.Entry.mining]
    maxLevel = 2147483647
    displayGroupName = false
    useTotalLevels = false
    color = 65535
    showInList = true
    icon = ""minecraft:textures/mob_effect/haste.png""
    iconSize = 18
    noAfkPenalty = false

[Skills.Entry.engineering]
    maxLevel = 100
    displayGroupName = false
    useTotalLevels = false
    color = 16777215
    showInList = true
    icon = ""minecraft:textures/item/redstone.png""
    iconSize = 16
    noAfkPenalty = false

[Skills.Entry.endurance]
    maxLevel = 2147483647
    displayGroupName = false
    useTotalLevels = false
    color = 13369344
    showInList = true
    icon = ""minecraft:textures/mob_effect/absorption.png""
    iconSize = 18
    noAfkPenalty = false

[Skills.Entry.building]
    maxLevel = 2147483647
    displayGroupName = false
    useTotalLevels = false
    color = 65535
    showInList = true
    icon = ""pmmo:textures/skills/building.png""
    iconSize = 32
    noAfkPenalty = false

[Skills.Entry.smithing]
    maxLevel = 2147483647
    displayGroupName = false
    useTotalLevels = false
    color = 15790320
    showInList = true
    icon = ""pmmo:textures/skills/smithing.png""
    iconSize = 32
    noAfkPenalty = true

[Skills.Entry.swimming]
    maxLevel = 2147483647
    displayGroupName = false
    useTotalLevels = false
    color = 3368703
    showInList = true
    icon = ""minecraft:textures/mob_effect/dolphins_grace.png""
    iconSize = 18
    noAfkPenalty = false

[Skills.Entry.woodcutting]
    maxLevel = 2147483647
    displayGroupName = false
    useTotalLevels = false
    color = 16753434
    showInList = true
    icon = ""minecraft:textures/item/iron_axe.png""
    iconSize = 16
    noAfkPenalty = false

[Skills.Entry.gunslinging]
    maxLevel = 2147483647
    displayGroupName = false
    useTotalLevels = false
    color = 13877667
    showInList = true
    icon = ""pmmo:textures/skills/missing_icon.png""
    iconSize = 18
    noAfkPenalty = false

[Skills.Entry.crafting]
    maxLevel = 2147483647
    displayGroupName = false
    useTotalLevels = false
    color = 16750848
    showInList = true
    icon = ""pmmo:textures/skills/crafting.png""
    iconSize = 32
    noAfkPenalty = false

[Skills.Entry.excavation]
    maxLevel = 2147483647
    displayGroupName = false
    useTotalLevels = false
    color = 15112448
    showInList = true
    icon = ""minecraft:textures/item/iron_shovel.png""
    iconSize = 16
    noAfkPenalty = false

[Skills.Entry.farming]
    maxLevel = 2147483647
    displayGroupName = false
    useTotalLevels = false
    color = 58880
    showInList = true
    icon = ""minecraft:textures/item/wheat.png""
    iconSize = 16
    noAfkPenalty = false

[Skills.Entry.flying]
    maxLevel = 2147483647
    displayGroupName = false
    useTotalLevels = false
    color = 13421823
    showInList = true
    icon = ""minecraft:textures/item/elytra.png""
    iconSize = 16
    noAfkPenalty = false

[Skills.Entry.cooking]
    maxLevel = 2147483647
    displayGroupName = false
    useTotalLevels = false
    color = 15112448
    showInList = true
    icon = ""minecraft:textures/item/cooked_mutton.png""
    iconSize = 16
    noAfkPenalty = true

[Skills.Entry.agility]
    maxLevel = 2147483647
    displayGroupName = false
    useTotalLevels = false
    color = 6736998
    showInList = true
    icon = ""minecraft:textures/mob_effect/speed.png""
    iconSize = 18
    noAfkPenalty = false

[Skills.Entry.sailing]
    maxLevel = 2147483647
    displayGroupName = false
    useTotalLevels = false
    color = 10073087
    showInList = true
    icon = ""minecraft:textures/item/oak_boat.png""
    iconSize = 16
    noAfkPenalty = false

[Skills.Entry.hunter]
    maxLevel = 2147483647
    displayGroupName = false
    useTotalLevels = false
    color = 13596693
    showInList = true
    icon = ""minecraft:textures/item/diamond_sword.png""
    iconSize = 16
    noAfkPenalty = false

[Skills.Entry.archery]
    maxLevel = 2147483647
    displayGroupName = false
    useTotalLevels = false
    color = 16776960
    showInList = true
    icon = ""minecraft:textures/item/bow.png""
    iconSize = 16
    noAfkPenalty = false

[Skills.Entry.taming]
    maxLevel = 2147483647
    displayGroupName = false
    useTotalLevels = false
    color = 16777215
    showInList = true
    icon = ""minecraft:textures/item/lead.png""
    iconSize = 16
    noAfkPenalty = false
";
            
            var defaultSkills = TomlSkillParser.ParseSkills(defaultSkillsToml);
            foreach (var skill in defaultSkills)
            {
                Skills.Add(skill);
            }
            if (Skills.Count > 0)
                SelectedSkill = Skills[0];
        }

        public bool LoadSkillsFromFile(string filePath)
        {
            try
            {
                string fileContent = System.IO.File.ReadAllText(filePath);
                var skills = TomlSkillParser.ParseSkills(fileContent);
                
                if (skills.Count == 0)
                {
                    return false;
                }
                Skills.Clear();
                foreach (var skill in skills)
                {
                    Skills.Add(skill);
                }
                if (Skills.Count > 0)
                    SelectedSkill = Skills[0];
                CurrentFilePath = filePath;
                
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        
        public bool SaveToFile(string? filePath = null)
        {
            filePath = filePath ?? CurrentFilePath;
            if (string.IsNullOrEmpty(filePath))
                return false;
                
            try
            {
                var sb = new System.Text.StringBuilder();

                sb.AppendLine();
                
                sb.AppendLine("#========================================================================");
                sb.AppendLine("#");
                sb.AppendLine("# All skills in pmmo are defined when they are used anywhere in PMMO.");
                sb.AppendLine("# You do not need to define a skill here to use it. However, defining");
                sb.AppendLine("# a skills attributes here will give you amore rounded skill list and");
                sb.AppendLine("# a cleaner looking mod. Note that all the defaults here can be replaced");
                sb.AppendLine("# if you wish. Also note that when using custom icon sizes they must be");
                sb.AppendLine("# a square image(eg. 16x16, 24x24, 32x32) and they default to 18x18.");
                sb.AppendLine("#");
                sb.AppendLine("#========================================================================");
                sb.AppendLine("[Skills]");
                sb.AppendLine();
                sb.AppendLine("\t[Skills.Entry]");
                sb.AppendLine();
                
                foreach (var skill in Skills)
                {
                    sb.AppendLine(SkillHelper.FormatSkillToToml(skill, 2));
                    sb.AppendLine();
                }
                
                System.IO.File.WriteAllText(filePath, sb.ToString());
                if (filePath != CurrentFilePath)
                    CurrentFilePath = filePath;
                    
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}