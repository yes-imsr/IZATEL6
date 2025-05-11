using Avalonia.Controls;
using Avalonia.Media;
using System;


namespace PMMOEdit;

public partial class SkillsPage : UserControl
{
    private SkillsPageViewModel ViewModel { get; }

    public SkillsPage()
    {
        ViewModel = new SkillsPageViewModel();
        DataContext = ViewModel;
    
        InitializeComponent();

        // Add some test data
        ViewModel.Skills.Add(new Skill { Name = "Test Skill", Color = 16711680 }); // Red
        ViewModel.Skills.Add(new Skill { Name = "Another Skill", Color = 65280 }); // Green

        var skillsList = this.FindControl<ListBox>("SkillsList");
        if (skillsList != null)
        {
            skillsList.ItemsSource = ViewModel.Skills;
            skillsList.SelectionChanged += SkillsList_SelectionChanged;
            
            skillsList.DoubleTapped += (s, e) => 
            {
                if (skillsList.SelectedItem is Skill selectedSkill)
                {
                    ViewModel.SelectedSkill = selectedSkill;
                    
                    var skillEditorGrid = this.FindControl<Grid>("SkillEditorGrid");
                    if (skillEditorGrid != null)
                        skillEditorGrid.IsVisible = true;
                }
            };
        }
        
        var newFileButton = this.FindControl<Button>("NewFileButton");
        if (newFileButton != null)
            newFileButton.Click += NewFile_Click;

        var editFileButton = this.FindControl<Button>("EditFileButton");
        if (editFileButton != null)
            editFileButton.Click += EditFile_Click;

        var newExportButton = this.FindControl<Button>("NewExportButton");
        if (newExportButton != null)
            newExportButton.Click += NewExport_Click;
        
        var addSkillButton = this.FindControl<Button>("AddSkillButton");
        if (addSkillButton != null)
            addSkillButton.Click += AddSkill_Click;
            
        var saveSkillButton = this.FindControl<Button>("SaveSkill");
        if (saveSkillButton != null)
            saveSkillButton.Click += SaveSkill_Click;
            
        var deleteSkillButton = this.FindControl<Button>("DeleteSkillButton");
        if (deleteSkillButton != null)
            deleteSkillButton.Click += DeleteSkill_Click;
            
        var browseIconButton = this.FindControl<Button>("BrowseIcon");
        if (browseIconButton != null)
            browseIconButton.Click += BrowseIcon_Click;
        
        var colorPicker = this.FindControl<Avalonia.Controls.ColorPicker>("ColorPicker");
        if (colorPicker != null)
            colorPicker.ColorChanged += ColorPicker_ColorChanged;
        
        var popup = this.FindControl<Avalonia.Controls.Primitives.Popup>("ColorPickerPopup");
        if (popup != null)
        {
            this.PointerPressed += (s, e) => 
            {
                if (popup.IsOpen && !e.Handled)
                {
                    var point = e.GetPosition(this);
                    var colorSwatch = this.FindControl<Border>("ColorSwatch");
                    var popupBounds = popup.Bounds;
                    
                    if (colorSwatch != null && !colorSwatch.Bounds.Contains(point) && 
                        !popupBounds.Contains(point))
                    {
                        popup.IsOpen = false;
                    }
                }
            };
        }
    }
            
            private void ColorPicker_ColorChanged(object? sender, Avalonia.Controls.ColorChangedEventArgs e)
            {
        if (ViewModel.SelectedSkill != null && sender is Avalonia.Controls.ColorPicker)
        {
            // Convert Avalonia color to integer RGB color value
            var color = e.NewColor;
            int colorValue = (color.R << 16) | (color.G << 8) | color.B;
            
            // Update the skill's color property
            ViewModel.SelectedSkill.Color = colorValue;
        }
    }
    
    private void NewFile_Click(object? sender, EventArgs e)
    {
        // TODO: Implement new file creation logic
        var skillEditorGrid = this.FindControl<Grid>("SkillEditorGrid");
        if (skillEditorGrid != null)
            skillEditorGrid.IsVisible = true;
        
        var headerText = this.FindControl<TextBlock>("HeaderText");
        if (headerText != null)
            headerText.Text = "Editing File";
    }
    
    private void EditFile_Click(object? sender, EventArgs e)
    {
        // TODO: Implement file editing logic
        var skillEditorGrid = this.FindControl<Grid>("SkillEditorGrid");
        if (skillEditorGrid != null)
            skillEditorGrid.IsVisible = true;
        
        var headerText = this.FindControl<TextBlock>("HeaderText");
        if (headerText != null)
            headerText.Text = "Editing File";
    }
    
    private void NewExport_Click(object? sender, EventArgs e)
    {
        // TODO: Implement export creation logic
    }
    
    private void AddSkill_Click(object? sender, EventArgs e)
    {
        // Create a new skill with default values
        var newSkill = new Skill
        {
            Name = "New Skill",
            MaxLevel = 100,
            Color = 3355443, // Dark gray
            ShowInList = true,
            IconSize = 32
        };
        
        // Add it to the collection
        ViewModel.Skills.Add(newSkill);
        
        // Select the new skill to edit it
        ViewModel.SelectedSkill = newSkill;
    }
    
    private void SaveSkill_Click(object? sender, EventArgs e)
    {
        // Currently, the skill is already saved in the collection via two-way binding
        // In a real application, you would save to a file or database here
        
        // Just for feedback that the save worked:
        var skillName = ViewModel.SelectedSkill?.Name ?? "skill";
        _ = MessageBox.Show($"Saved {skillName} successfully!", "Save Complete");
    }
    
    private void DeleteSkill_Click(object? sender, EventArgs e)
    {
        if (ViewModel.SelectedSkill == null)
            return;
            
        // Get the skill info before removal
        var skillName = ViewModel.SelectedSkill.Name;
        var skillToDelete = ViewModel.SelectedSkill;
        
        // Remove the skill from the collection
        ViewModel.Skills.Remove(skillToDelete);
        
        // Select another skill if available, otherwise clear selection
        if (ViewModel.Skills.Count > 0)
        {
            ViewModel.SelectedSkill = ViewModel.Skills[0];
        }
        else
        {
            ViewModel.SelectedSkill = null;
        }
        
        // Notify the user that deletion happened
        _ = MessageBox.Show($"Deleted skill '{skillName}'", "Deletion Complete");
    }
    
    private void SkillsList_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is ListBox listBox && listBox.SelectedItem is Skill selectedSkill)
        {
            // Update the selected skill in the view model
            ViewModel.SelectedSkill = selectedSkill;
            
            // The color hex input will be automatically updated through binding
            // to the Skill.ColorHex property
        }
    }
    
    private async void BrowseIcon_Click(object? sender, EventArgs e)
    {
        if (ViewModel.SelectedSkill == null)
            return;
            
        // Create OpenFileDialog
        var dialog = new OpenFileDialog
        {
            Title = "Select Skill Icon",
            AllowMultiple = false
        };
        
        // Set up file filters for common image formats
        dialog.Filters.Add(new FileDialogFilter
        {
            Name = "Image Files",
            Extensions = { "png", "jpg", "jpeg", "gif", "bmp" }
        });
        
        dialog.Filters.Add(new FileDialogFilter
        {
            Name = "All Files",
            Extensions = { "*" }
        });
        
        try
        {
            // Show the dialog and get the selected file
            var result = await dialog.ShowAsync(this.VisualRoot as Window);
            
            if (result != null && result.Length > 0)
            {
                // Update the skill's icon property with the selected file path
                ViewModel.SelectedSkill.Icon = result[0];
                
                // Optionally, you can update the UI explicitly if needed
                var iconTextBox = this.FindControl<TextBox>("Icon");
                if (iconTextBox != null)
                {
                    iconTextBox.Text = result[0];
                }
            }
        }
        catch (Exception ex)
        {
            await MessageBox.Show($"Error selecting file: {ex.Message}", "Error");
        }
    }
    
    private void HexColorInput_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (ViewModel.SelectedSkill == null || sender is not TextBox textBox)
            return;
            
        string hexColor = textBox.Text?.Trim() ?? "";
        
        // Add # prefix if not present
        if (!hexColor.StartsWith("#") && hexColor.Length > 0)
        {
            hexColor = "#" + hexColor;
            textBox.Text = hexColor;
            return; // This will trigger another TextChanged event
        }
        
        // Ensure the format is valid (must be #RRGGBB)
        if (hexColor.StartsWith("#") && hexColor.Length == 7 && 
            System.Text.RegularExpressions.Regex.IsMatch(hexColor.Substring(1), "^[0-9A-Fa-f]{6}$"))
        {
            // Manually set the ColorHex property on the skill
            ViewModel.SelectedSkill.ColorHex = hexColor;
            
            // Parse the color value
            string hexValue = hexColor.Substring(1);
            int colorValue = int.Parse(hexValue, System.Globalization.NumberStyles.HexNumber);
            
            byte r = (byte)((colorValue >> 16) & 0xFF);
            byte g = (byte)((colorValue >> 8) & 0xFF);
            byte b = (byte)(colorValue & 0xFF);
            
            var newColor = new SolidColorBrush(Color.FromRgb(r, g, b));
            
            // Force UI update for preview square
            var colorPreview = this.FindControl<Border>("ColorPreview");
            if (colorPreview != null)
            {
                colorPreview.Background = newColor;
            }
            
            // Force UI update for the summary title
            var summaryTitle = this.FindControl<TextBlock>("SummaryTitle");
            if (summaryTitle != null)
            {
                summaryTitle.Foreground = newColor;
            }
        }
    }
}