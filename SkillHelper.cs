using System.Text;

namespace PMMOEdit;

public static class SkillHelper
{
    public static string FormatSkillToToml(Skill skill, int indentationLevel = 2)
    {
        var indent = new string(' ', indentationLevel * 4);
        var sb = new StringBuilder();

        sb.AppendLine($"{indent}[Skills.Entry.{skill.Name}]");
        sb.AppendLine($"{indent}    maxLevel = {skill.MaxLevel}");
        sb.AppendLine($"{indent}    displayGroupName = {skill.DisplayGroupName.ToString().ToLower()}");
        sb.AppendLine($"{indent}    useTotalLevels = {skill.UseTotalLevels.ToString().ToLower()}");
        sb.AppendLine($"{indent}    color = {skill.Color}");
        sb.AppendLine($"{indent}    showInList = {skill.ShowInList.ToString().ToLower()}");
        sb.AppendLine($"{indent}    icon = \"{skill.Icon}\"");
        sb.AppendLine($"{indent}    iconSize = {skill.IconSize}");
        sb.AppendLine($"{indent}    noAfkPenalty = {skill.NoAfkPenalty.ToString().ToLower()}");

        if (skill.IsGroupSkill())
        {
            sb.AppendLine();
            sb.AppendLine($"{indent}    [Skills.Entry.{skill.Name}.groupFor]");
            foreach (var (subSkill, value) in skill.GroupFor!)
            {
                sb.AppendLine($"{indent}        {subSkill} = {value}");
            }
        }

        return sb.ToString();
    }
}
