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
        var pageContent = this.FindControl<ContentControl>("PageContent");
        if (pageContent != null)
        {
            pageContent.Content = new WelcomePage();
        }
        
        var titleBarBorder = this.FindControl<Border>("TitleBarBorder");
        if (titleBarBorder != null)
        {
            titleBarBorder.PointerPressed += (_, e) =>
            {
                BeginMoveDrag(e);
            };
        }
        
        var homeButton = this.FindControl<Button>("HomeButton");
        if (homeButton != null)
        {
            homeButton.Click += (_, _) =>
            {
                var pageContent = this.FindControl<ContentControl>("PageContent");
                if (pageContent != null)
                {
                    pageContent.Content = new WelcomePage();
                }
                
                if (_activeButton != null)
                {
                    _activeButton.Classes.Remove("active");
                    _activeButton = null;
                }
            };
        }
        
        var skillsButton = this.FindControl<Button>("SkillsButton");
        if (skillsButton != null)
        {
            skillsButton.Click += (sender, _) =>
            {
                var pageContent = this.FindControl<ContentControl>("PageContent");
                if (pageContent != null)
                {
                    pageContent.Content = new SkillsPage();
                }
                
                if (_activeButton != null)
                    _activeButton.Classes.Remove("active");
                
                if (sender is Button button)
                {
                    button.Classes.Add("active");
                    _activeButton = button;
                }
            };
        }
        
        var serverButton = this.FindControl<Button>("ServerButton");
        if (serverButton != null)
        {
            serverButton.Click += (sender, _) =>
            {
                var pageContent = this.FindControl<ContentControl>("PageContent");
                if (pageContent != null)
                {
                    pageContent.Content = new ServerPage();
                }
                
                if (_activeButton != null)
                    _activeButton.Classes.Remove("active");
                
                if (sender is Button button)
                {
                    button.Classes.Add("active");
                    _activeButton = button;
                }
            };
        }
        
        var dataButton = this.FindControl<Button>("DataButton");
        if (dataButton != null)
        {
            dataButton.Click += (sender, _) =>
            {
                var pageContent = this.FindControl<ContentControl>("PageContent");
                if (pageContent != null)
                {
                    pageContent.Content = new DataPage();
                }
                
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