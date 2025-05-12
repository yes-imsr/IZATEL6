using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;

namespace PMMOEdit;

public partial class MainWindow : Window
{
    private Button? _activeButton;

    public MainWindow()
    {
        InitializeComponent();
        
        if (OperatingSystem.IsWindows())
        {
            TransparencyLevelHint = [WindowTransparencyLevel.AcrylicBlur];
            Background = null;
        }
        
        var titleBarBorder = this.FindControl<Border>("TitleBarBorder");
        if (titleBarBorder != null)
        {
            titleBarBorder.PointerPressed += (_, e) =>
            {
                BeginMoveDrag(e);
            };
        }
        
        var skillsButton = this.FindControl<Button>("SkillsButton");
        if (skillsButton != null)
        {
            skillsButton.Click += (sender, _) =>
            {
                var pageContent = this.FindControl<ContentControl>("PageContent");
                pageContent.Content = new SkillsPage();
                
                if (_activeButton != null)
                    _activeButton.Classes.Remove("active");
                
                if (sender is Button button)
                {
                    button.Classes.Add("active");
                    _activeButton = button;
                }
            };
        }
    }
}