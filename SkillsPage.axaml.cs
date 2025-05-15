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

        var saveFileButton = this.FindControl<Button>("SaveFileButton");
        if (saveFileButton != null)
            saveFileButton.Click += SaveFile_Click;
            
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
            var color = e.NewColor;
            int colorValue = (color.R << 16) | (color.G << 8) | color.B;
            
            ViewModel.SelectedSkill.Color = colorValue;
        }
    }
    
    private void NewFile_Click(object? sender, EventArgs e)
    {
        // Clear any existing file path
        ViewModel.CurrentFilePath = null;
        
        // Load default skills
        ViewModel.LoadDefaultSkills();
        
        // Update skills list
        var skillsList = this.FindControl<ListBox>("SkillsList");
        if (skillsList != null)
        {
            skillsList.ItemsSource = null;
            skillsList.ItemsSource = ViewModel.Skills;
        }
        
        // Show the editor grid
        var skillEditorGrid = this.FindControl<Grid>("SkillEditorGrid");
        if (skillEditorGrid != null)
            skillEditorGrid.IsVisible = true;
        
        // Update header text
        var headerText = this.FindControl<TextBlock>("HeaderText");
        if (headerText != null)
            headerText.Text = "New Skills File";
    }
    
    private async void EditFile_Click(object? sender, EventArgs e)
    {
        try
        {
            var dialog = new OpenFileDialog
            {
                Title = "Open PMMO Skills File",
                AllowMultiple = false
            };
            
            dialog.Filters.Add(new FileDialogFilter
            {
                Name = "PMMO Skills Files",
                Extensions = { "toml" }
            });
            
            var result = await dialog.ShowAsync(this.VisualRoot as Window);
            
            if (result != null && result.Length > 0)
            {
                string filePath = result[0];
                string fileName = System.IO.Path.GetFileName(filePath);
                
                // Validate that the file is named pmmo-Skills.toml
                if (!fileName.Equals("pmmo-Skills.toml", StringComparison.OrdinalIgnoreCase))
                {
                    await MessageBox.Show("Please select a valid pmmo-Skills.toml file.", "Invalid File");
                    return;
                }
                
                // Load skills from the file
                bool success = ViewModel.LoadSkillsFromFile(filePath);
                
                if (success)
                {
                    // Show the skill editor grid
                    var skillEditorGrid = this.FindControl<Grid>("SkillEditorGrid");
                    if (skillEditorGrid != null)
                        skillEditorGrid.IsVisible = true;
                    
                    // Update header text
                    var headerText = this.FindControl<TextBlock>("HeaderText");
                    if (headerText != null)
                        headerText.Text = $"Editing {fileName}";
                        
                    // Update the skill list with the loaded skills
                    var skillsList = this.FindControl<ListBox>("SkillsList");
                    if (skillsList != null)
                    {
                        skillsList.ItemsSource = null;
                        skillsList.ItemsSource = ViewModel.Skills;
                    }
                }
                else
                {
                    await MessageBox.Show("Failed to load skills from the selected file. The file might be corrupted or have an invalid format.", "Load Error");
                }
            }
        }
        catch (Exception ex)
        {
            await MessageBox.Show($"Error loading file: {ex.Message}", "Error");
        }
    }
    
    private async void NewExport_Click(object? sender, EventArgs e)
    {
        // Show Grid if it's not visible
        var skillEditorGrid = this.FindControl<Grid>("SkillEditorGrid");
        if (skillEditorGrid != null && !skillEditorGrid.IsVisible)
            skillEditorGrid.IsVisible = true;
        
        // If there are no skills to export, show a message
        if (ViewModel.Skills.Count == 0)
        {
            await MessageBox.Show("No skills to export. Please create or load skills first.", "Export Error");
            return;
        }
        
        var dialog = new SaveFileDialog
        {
            Title = "Export Skills to TOML",
            InitialFileName = "pmmo-Skills.toml",
            DefaultExtension = ".toml"
        };
        
        dialog.Filters.Add(new FileDialogFilter
        {
            Name = "TOML Files",
            Extensions = { "toml" }
        });
        
        dialog.Filters.Add(new FileDialogFilter
        {
            Name = "All Files",
            Extensions = { "*" }
        });
        
        try
        {
            var filePath = await dialog.ShowAsync(this.VisualRoot as Window);
            
            if (!string.IsNullOrEmpty(filePath))
            {
                // Use the ViewModel's SaveToFile method to save with the correct format
                bool success = ViewModel.SaveToFile(filePath);
                
                if (success)
                {
                    await MessageBox.Show($"Skills successfully exported to {System.IO.Path.GetFileName(filePath)}", "Export Complete");
                }
                else
                {
                    await MessageBox.Show("Failed to export skills to file.", "Export Error");
                }
            }
        }
        catch (Exception ex)
        {
            await MessageBox.Show($"Error exporting skills: {ex.Message}", "Export Error");
        }
    }
    
    private void AddSkill_Click(object? sender, EventArgs e)
    {
        var newSkill = new Skill
        {
            Name = "New Skill",
            MaxLevel = 100,
            Color = 3355443,
            ShowInList = true,
            IconSize = 32
        };
        
        ViewModel.Skills.Add(newSkill);
        ViewModel.SelectedSkill = newSkill;
    }
    
    private void SaveSkill_Click(object? sender, EventArgs e)
    {
        var skillName = ViewModel.SelectedSkill?.Name ?? "skill";
        _ = MessageBox.Show($"Saved {skillName} successfully!", "Save Complete");
    }
    
    private async void SaveFile_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(ViewModel.CurrentFilePath))
        {
            await MessageBox.Show("No file is currently open. Please use 'Export' to save to a new file.", "Save Error");
            return;
        }
        
        try
        {
            bool success = ViewModel.SaveToFile();
            
            if (success)
            {
                await MessageBox.Show($"Skills successfully saved to {System.IO.Path.GetFileName(ViewModel.CurrentFilePath)}", "Save Complete");
            }
            else
            {
                await MessageBox.Show("Failed to save skills to file. Please try using 'Export' instead.", "Save Error");
            }
        }
        catch (Exception ex)
        {
            await MessageBox.Show($"Error saving file: {ex.Message}", "Save Error");
        }
    }
    
    private void DeleteSkill_Click(object? sender, EventArgs e)
    {
        if (ViewModel.SelectedSkill == null)
            return;
        
        var skillName = ViewModel.SelectedSkill.Name;
        var skillToDelete = ViewModel.SelectedSkill;
        
        ViewModel.Skills.Remove(skillToDelete);
        
        if (ViewModel.Skills.Count > 0)
        {
            ViewModel.SelectedSkill = ViewModel.Skills[0];
        }
        else
        {
            ViewModel.SelectedSkill = null;
        }
        
        _ = MessageBox.Show($"Deleted skill '{skillName}'", "Deletion Complete");
    }
    
    private void SkillsList_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is ListBox listBox && listBox.SelectedItem is Skill selectedSkill)
        {
            ViewModel.SelectedSkill = selectedSkill;
        }
    }
    
    private async void BrowseIcon_Click(object? sender, EventArgs e)
    {
        if (ViewModel.SelectedSkill == null)
            return;
        
        var dialog = new OpenFileDialog
        {
            Title = "Select Skill Icon",
            AllowMultiple = false
        };
        
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
            var result = await dialog.ShowAsync(this.VisualRoot as Window);
            
            if (result != null && result.Length > 0)
            {
                ViewModel.SelectedSkill.Icon = result[0];
                
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
        if (!hexColor.StartsWith("#") && hexColor.Length > 0)
        {
            hexColor = "#" + hexColor;
            textBox.Text = hexColor;
            return;
        }
        
        if (hexColor.StartsWith("#") && hexColor.Length == 7 && 
            System.Text.RegularExpressions.Regex.IsMatch(hexColor.Substring(1), "^[0-9A-Fa-f]{6}$"))
        {
            ViewModel.SelectedSkill.ColorHex = hexColor;
            string hexValue = hexColor.Substring(1);
            int colorValue = int.Parse(hexValue, System.Globalization.NumberStyles.HexNumber);
            
            byte r = (byte)((colorValue >> 16) & 0xFF);
            byte g = (byte)((colorValue >> 8) & 0xFF);
            byte b = (byte)(colorValue & 0xFF);
            
            var newColor = new SolidColorBrush(Color.FromRgb(r, g, b));
            
            var colorPreview = this.FindControl<Border>("ColorPreview");
            if (colorPreview != null)
            {
                colorPreview.Background = newColor;
            }
             
            var summaryTitle = this.FindControl<TextBlock>("SummaryTitle");
            if (summaryTitle != null)
            {
                summaryTitle.Foreground = newColor;
            }
        }
    }
}