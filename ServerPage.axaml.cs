using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using PMMOEdit.Models;

namespace PMMOEdit
{
    public partial class ServerPage : UserControl
    {
        private ServerPageViewModel ViewModel { get; }
        private string _newSkillName = string.Empty;
        private string _newVeinBlacklistItem = string.Empty;
        private string _newPartyBonusSkill = string.Empty;

        public ServerPage()
        {
            ViewModel = new ServerPageViewModel();
            DataContext = ViewModel;
            
            InitializeComponent();
            
            // Debug output to confirm constructor is called
            System.Diagnostics.Debug.WriteLine("ServerPage constructor called");

            // Wire up button events
            var newFileButton = this.FindControl<Button>("NewFileButton");
            if (newFileButton != null)
                newFileButton.Click += NewFile_Click;
                
            // Wire up enable/disable all requirements buttons
            var enableAllRequirementsButton = this.FindControl<Button>("EnableAllRequirementsButton");
            if (enableAllRequirementsButton != null)
                enableAllRequirementsButton.Click += EnableAllRequirements_Click;
                
            var disableAllRequirementsButton = this.FindControl<Button>("DisableAllRequirementsButton");
            if (disableAllRequirementsButton != null)
                disableAllRequirementsButton.Click += DisableAllRequirements_Click;
                
            // Wire up party bonus controls
            var addPartyBonusButton = this.FindControl<Button>("AddPartyBonusButton");
            if (addPartyBonusButton != null)
                addPartyBonusButton.Click += AddPartyBonus_Click;
                
            var partyBonusListBox = this.FindControl<ListBox>("PartyBonusListBox");
            if (partyBonusListBox != null)
            {
                partyBonusListBox.AddHandler(Button.ClickEvent, HandlePartyBonusListBoxButtonClick, Avalonia.Interactivity.RoutingStrategies.Bubble);
                // Initialize with current items
                UpdatePartyBonusListBox();
            }
                
            var newPartyBonusSkillTextBox = this.FindControl<TextBox>("NewPartyBonusSkillTextBox");
            if (newPartyBonusSkillTextBox != null)
            {
                // Add TextChanged event handler
                newPartyBonusSkillTextBox.TextChanged += (s, e) => 
                {
                    _newPartyBonusSkill = newPartyBonusSkillTextBox.Text ?? string.Empty;
                };
            }

            var editFileButton = this.FindControl<Button>("EditFileButton");
            if (editFileButton != null)
                editFileButton.Click += EditFile_Click;

            var saveFileButton = this.FindControl<Button>("SaveFileButton");
            if (saveFileButton != null)
                saveFileButton.Click += SaveFile_Click;
                
            var newExportButton = this.FindControl<Button>("NewExportButton");
            if (newExportButton != null)
                newExportButton.Click += NewExport_Click;
                
            var serverConfigButton = this.FindControl<Button>("ServerConfigButton");
            if (serverConfigButton != null)
                serverConfigButton.Click += ServerConfigButton_Click;
                
            var advancedConfigButton = this.FindControl<Button>("AdvancedConfigButton");
            if (advancedConfigButton != null)
                advancedConfigButton.Click += AdvancedConfigButton_Click;
                
            var applyChangesButton = this.FindControl<Button>("ApplyChangesButton");
            if (applyChangesButton != null)
                applyChangesButton.Click += ApplyChanges_Click;
                
            var resetDefaultsButton = this.FindControl<Button>("ResetDefaultsButton");
            if (resetDefaultsButton != null)
                resetDefaultsButton.Click += ResetDefaults_Click;
            
            // Wire up Skill Modifiers controls
            var addSkillModifierButton = this.FindControl<Button>("AddSkillModifierButton");
            if (addSkillModifierButton != null)
                addSkillModifierButton.Click += AddSkillModifier_Click;
            
            var newSkillNameTextBox = this.FindControl<TextBox>("NewSkillNameTextBox");
            if (newSkillNameTextBox != null)
            {
                // Add TextChanged event handler
                newSkillNameTextBox.TextChanged += (s, e) => 
                {
                    _newSkillName = newSkillNameTextBox.Text ?? string.Empty;
                };
            }
            
            // Wire up Vein Blacklist controls
            var addVeinBlacklistButton = this.FindControl<Button>("AddVeinBlacklistButton");
            if (addVeinBlacklistButton != null)
                addVeinBlacklistButton.Click += AddVeinBlacklistItem_Click;
            
            var newVeinBlacklistItemTextBox = this.FindControl<TextBox>("NewVeinBlacklistItemTextBox");
            if (newVeinBlacklistItemTextBox != null)
            {
                // Add TextChanged event handler
                newVeinBlacklistItemTextBox.TextChanged += (s, e) => 
                {
                    _newVeinBlacklistItem = newVeinBlacklistItemTextBox.Text ?? string.Empty;
                };
            }
            
            // Wire up tab control selection changes
            var configTabControl = this.FindControl<TabControl>("ConfigTabControl");
            if (configTabControl != null)
            {
                configTabControl.SelectionChanged += (s, e) => 
                {
                    // Update header text based on selected tab
                    var headerText = this.FindControl<TextBlock>("HeaderText");
                    if (headerText != null && configTabControl.SelectedItem is TabItem selectedTab)
                    {
                        string tabHeader = selectedTab.Header?.ToString() ?? "";
                        headerText.Text = $"{tabHeader} Configuration";
                    }
                };
            }
            
            // Add event handler for the skill modifiers remove button
            var skillModifiersListBox = this.FindControl<ListBox>("SkillModifiersListBox");
            if (skillModifiersListBox != null)
            {
                skillModifiersListBox.AddHandler(Button.ClickEvent, HandleSkillModifiersListBoxButtonClick, Avalonia.Interactivity.RoutingStrategies.Bubble);
            }
            
            // Add event handler for the vein blacklist remove button
            var veinBlacklistBox = this.FindControl<ListBox>("VeinBlacklistBox");
            if (veinBlacklistBox != null)
            {
                veinBlacklistBox.AddHandler(Button.ClickEvent, HandleVeinBlacklistBoxButtonClick, Avalonia.Interactivity.RoutingStrategies.Bubble);
            }
        }
        
        private void HandleSkillModifiersListBoxButtonClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (e.Source is Button button && button.Name == "RemoveSkillModifierButton" && button.Tag is string skillName)
            {
                RemoveSkillModifierButton_Click(button, EventArgs.Empty);
                e.Handled = true;
            }
        }
        
        private void HandleVeinBlacklistBoxButtonClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (e.Source is Button button && button.Name == "RemoveVeinBlacklistButton" && button.Tag is string item)
            {
                RemoveVeinBlacklistButton_Click(button, EventArgs.Empty);
                e.Handled = true;
            }
        }

        private void NewFile_Click(object? sender, EventArgs e)
        {
            // Debug output to verify method is being called
            System.Diagnostics.Debug.WriteLine("NewFile_Click called");
            
            if (ViewModel == null)
            {
                System.Diagnostics.Debug.WriteLine("ViewModel is null");
                return;
            }
            
            ViewModel.CreateNewServerConfig();
            
            var headerText = this.FindControl<TextBlock>("HeaderText");
            if (headerText != null)
                headerText.Text = "New Server Config File";
            else
                System.Diagnostics.Debug.WriteLine("HeaderText not found");
                
            var serverContentGrid = this.FindControl<Grid>("ServerContentGrid");
            if (serverContentGrid != null)
                serverContentGrid.IsVisible = true;
            else
                System.Diagnostics.Debug.WriteLine("ServerContentGrid not found");
                
            // Force UI update
            this.InvalidateVisual();
        }
        
        private async void EditFile_Click(object? sender, EventArgs e)
        {
            try
            {
                var dialog = new OpenFileDialog
                {
                    Title = "Open PMMO Server Config File",
                    AllowMultiple = false
                };
                
                dialog.Filters.Add(new FileDialogFilter
                {
                    Name = "PMMO Server Config Files",
                    Extensions = { "toml" }
                });
                
                var dialogParent = this.VisualRoot as Window;
                var result = await dialog.ShowAsync(dialogParent);
                
                if (result != null && result.Length > 0)
                {
                    string filePath = result[0];
                    string fileName = System.IO.Path.GetFileName(filePath);
                    if (!fileName.Equals("pmmo-Server.toml", StringComparison.OrdinalIgnoreCase))
                    {
                        var messageBox = new CustomConfirmDialog
                        {
                            Title = "Invalid File",
                            Message = "Please select a valid pmmo-Server.toml file.",
                            ConfirmButtonText = "OK",
                            ShowCancelButton = false
                        };
                        await messageBox.ShowDialog(this.VisualRoot as Window);
                        return;
                    }
                    
                    bool success = ViewModel.LoadServerConfigFromFile(filePath);
                    
                    if (success)
                    {
                        var serverContentGrid = this.FindControl<Grid>("ServerContentGrid");
                        if (serverContentGrid != null)
                            serverContentGrid.IsVisible = true;
                            
                        var headerText = this.FindControl<TextBlock>("HeaderText");
                        if (headerText != null)
                            headerText.Text = $"Editing {fileName}";
                    }
                    else
                    {
                        var messageBox = new CustomConfirmDialog
                        {
                            Title = "Load Error",
                            Message = "Failed to load server config from the selected file. The file might be corrupted or have an invalid format.",
                            ConfirmButtonText = "OK",
                            ShowCancelButton = false
                        };
                        var dialogOwner = this.VisualRoot as Window;
                        if (dialogOwner != null)
                            await messageBox.ShowDialog(dialogOwner);
                        else
                            await messageBox.ShowDialog(null);
                    }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    var messageBox = new CustomConfirmDialog
                                    {
                    Title = "Error",
                    Message = $"Error loading file: {ex.Message}",
                    ConfirmButtonText = "OK",
                    ShowCancelButton = false
                                    };
                                    await messageBox.ShowDialog(this.VisualRoot as Window);
            }
        }
        
        private async void SaveFile_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(ViewModel.CurrentFilePath))
            {
                var messageBox = new CustomConfirmDialog
                {
                    Title = "Save Error",
                    Message = "No file is currently open. Please use 'Export new file' to save to a new file.",
                    ConfirmButtonText = "OK",
                    ShowCancelButton = false
                };
                await messageBox.ShowDialog(this.VisualRoot as Window);
                return;
            }
            
            try
            {
                // Ensure the directory exists before saving
                string directory = System.IO.Path.GetDirectoryName(ViewModel.CurrentFilePath);
                if (!string.IsNullOrEmpty(directory) && !System.IO.Directory.Exists(directory))
                {
                    System.IO.Directory.CreateDirectory(directory);
                }
                
                bool success = ViewModel.SaveToFile();
                
                if (success)
                {
                    // Verify file was saved completely by checking file size
                    var fileInfo = new System.IO.FileInfo(ViewModel.CurrentFilePath);
                    if (fileInfo.Length < 100) // If file is suspiciously small
                    {
                        var messageBox = new CustomConfirmDialog
                        {
                            Title = "Save Warning",
                            Message = $"File was saved but appears to be incomplete. The file size is only {fileInfo.Length} bytes. Try using 'Export new file' instead.",
                            ConfirmButtonText = "OK",
                            ShowCancelButton = false
                        };
                        await messageBox.ShowDialog(this.VisualRoot as Window);
                    }
                    else
                    {
                        var messageBox = new CustomConfirmDialog
                        {
                            Title = "Save Complete",
                            Message = $"Server config successfully saved to {System.IO.Path.GetFileName(ViewModel.CurrentFilePath)}",
                            ConfirmButtonText = "OK",
                            ShowCancelButton = false
                        };
                        await messageBox.ShowDialog(this.VisualRoot as Window);
                    }
                }
                else
                {
                    var messageBox = new CustomConfirmDialog
                    {
                        Title = "Save Error",
                        Message = "Failed to save server config to file. Please try using 'Export new file' instead.",
                        ConfirmButtonText = "OK",
                        ShowCancelButton = false
                    };
                    await messageBox.ShowDialog(this.VisualRoot as Window);
                }
            }
            catch (Exception ex)
            {
                // Provide more detailed error information
                var messageBox = new CustomConfirmDialog
                {
                    Title = "Save Error",
                    Message = $"Error saving file: {ex.Message}\n\nDetails: {ex.GetType().Name}\n{ex.StackTrace?.Split('\n')[0]}",
                    ConfirmButtonText = "OK",
                    ShowCancelButton = false
                };
                await messageBox.ShowDialog(this.VisualRoot as Window);
                
                // Log full details to debug console
                System.Diagnostics.Debug.WriteLine($"Error saving file: {ex}");
            }
        }
        
        private async void NewExport_Click(object? sender, EventArgs e)
        {
            var serverContentGrid = this.FindControl<Grid>("ServerContentGrid");
            if (serverContentGrid != null && !serverContentGrid.IsVisible)
                serverContentGrid.IsVisible = true;
                
            var dialog = new SaveFileDialog
            {
                Title = "Export Server Config to TOML",
                InitialFileName = "pmmo-Server.toml",
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
                    bool success = ViewModel.SaveToFile(filePath);
                    
                    if (success)
                    {
                        var messageBox = new CustomConfirmDialog
                        {
                            Title = "Export Complete",
                            Message = $"Server config successfully exported to {System.IO.Path.GetFileName(filePath)}",
                            ConfirmButtonText = "OK",
                            ShowCancelButton = false
                        };
                        await messageBox.ShowDialog(this.VisualRoot as Window);
                    }
                    else
                    {
                        var messageBox = new CustomConfirmDialog
                        {
                            Title = "Export Error",
                            Message = "Failed to export server config to file.",
                            ConfirmButtonText = "OK",
                            ShowCancelButton = false
                        };
                        await messageBox.ShowDialog(this.VisualRoot as Window);
                    }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    var messageBox = new CustomConfirmDialog
                                    {
                    Title = "Export Error",
                    Message = $"Error exporting server config: {ex.Message}",
                    ConfirmButtonText = "OK",
                    ShowCancelButton = false
                                    };
                                    await messageBox.ShowDialog(this.VisualRoot as Window);
            }
        }
        
        private void ServerConfigButton_Click(object? sender, EventArgs e)
        {
            // Toggle visibility of the server content grid
            var serverContentGrid = this.FindControl<Grid>("ServerContentGrid");
            if (serverContentGrid != null)
                serverContentGrid.IsVisible = true;
            
            // Change header text
            var headerText = this.FindControl<TextBlock>("HeaderText");
            if (headerText != null)
                headerText.Text = "Server Configuration";
        }
        
        private void AdvancedConfigButton_Click(object? sender, EventArgs e)
        {
            // Toggle visibility of the server content grid
            var serverContentGrid = this.FindControl<Grid>("ServerContentGrid");
            if (serverContentGrid != null)
                serverContentGrid.IsVisible = true;
            
            // Change header text
            var headerText = this.FindControl<TextBlock>("HeaderText");
            if (headerText != null)
                headerText.Text = "Advanced Server Configuration";
        }
        
        private async void ApplyChanges_Click(object? sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("ApplyChanges_Click called");
            
            if (ViewModel.HasOpenedFile)
            {
                // If we already have a file open, save to that file
                bool success = ViewModel.SaveToFile();
                
                if (success)
                {
                    var messageBox = new CustomConfirmDialog
                    {
                        Title = "Save Complete",
                        Message = $"Server config successfully saved to {System.IO.Path.GetFileName(ViewModel.CurrentFilePath)}",
                        ConfirmButtonText = "OK",
                        ShowCancelButton = false
                    };
                    await messageBox.ShowDialog(this.VisualRoot as Window);
                }
                else
                {
                    var messageBox = new CustomConfirmDialog
                    {
                        Title = "Save Error",
                        Message = "Failed to save server config to file.",
                        ConfirmButtonText = "OK",
                        ShowCancelButton = false
                    };
                    await messageBox.ShowDialog(this.VisualRoot as Window);
                }
            }
            else
            {
                // If no file is open, ask the user to export to a new file
                var dialog = new SaveFileDialog
                {
                    Title = "Export Server Config to TOML",
                    InitialFileName = "pmmo-Server.toml",
                    DefaultExtension = ".toml"
                };
                
                dialog.Filters.Add(new FileDialogFilter
                {
                    Name = "TOML Files",
                    Extensions = { "toml" }
                });
                
                var filePath = await dialog.ShowAsync(this.VisualRoot as Window);
                
                if (!string.IsNullOrEmpty(filePath))
                {
                    bool success = ViewModel.SaveToFile(filePath);
                    
                    if (success)
                    {
                        var messageBox = new CustomConfirmDialog
                        {
                            Title = "Export Complete",
                            Message = $"Server config successfully exported to {System.IO.Path.GetFileName(filePath)}",
                            ConfirmButtonText = "OK",
                            ShowCancelButton = false
                        };
                        await messageBox.ShowDialog(this.VisualRoot as Window);
                    }
                    else
                    {
                        var messageBox = new CustomConfirmDialog
                        {
                            Title = "Export Error",
                            Message = "Failed to export server config to file.",
                            ConfirmButtonText = "OK",
                            ShowCancelButton = false
                        };
                        await messageBox.ShowDialog(this.VisualRoot as Window);
                    }
                }
            }
        }
        
        private async void ResetDefaults_Click(object? sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("ResetDefaults_Click called");
            
            var confirmDialog = new CustomConfirmDialog
            {
                Title = "Confirm Reset",
                Message = "Are you sure you want to reset all settings to default values?",
                ConfirmButtonText = "Yes",
                CancelButtonText = "No"
            };
            
            var result = await confirmDialog.ShowDialog(this.VisualRoot as Window);
                
            if (result)
            {
                ViewModel.CreateNewServerConfig();
                
                var headerText = this.FindControl<TextBlock>("HeaderText");
                if (headerText != null)
                    headerText.Text = "New Server Config File";
                
                // Ensure all UI elements are updated
                SyncUIWithViewModel();
                
                // Show confirmation message
                var messageBox = new CustomConfirmDialog
                {
                    Title = "Reset Complete",
                    Message = "All settings have been reset to their default values.",
                    ConfirmButtonText = "OK",
                    ShowCancelButton = false
                };
                await messageBox.ShowDialog(this.VisualRoot as Window);
                
                // Update lists and UI components with fresh data
                SyncUIWithViewModel();
                            }
                        }
                        
                        // We need to create an ObservableCollection to hold our skill modifiers data
                        private ObservableCollection<KeyValuePairViewModel> _skillModifiersCollection = new ObservableCollection<KeyValuePairViewModel>();
                        
                        private void SyncUIWithViewModel()
                        {
                            // Ensure the ViewModel has skill modifiers and vein blacklist collections
                            if (ViewModel.ServerConfig.SkillModifiers == null)
                                ViewModel.ServerConfig.SkillModifiers = new Dictionary<string, double>();
                                
                            if (ViewModel.ServerConfig.VeinBlacklist == null)
                                ViewModel.ServerConfig.VeinBlacklist = new List<string>();
                            
                            // Update the skill modifiers collection
                            _skillModifiersCollection.Clear();
                            foreach (var kvp in ViewModel.ServerConfig.SkillModifiers)
                            {
                                _skillModifiersCollection.Add(new KeyValuePairViewModel(kvp.Key, kvp.Value));
                            }
                            
                            // Set the ItemsSource of the skill modifiers list box
                            var skillModifiersListBox = this.FindControl<ListBox>("SkillModifiersListBox");
                            if (skillModifiersListBox != null)
                            {
                                skillModifiersListBox.ItemsSource = _skillModifiersCollection;
                            }
                            
                            // Sync vein blacklist - Use existing binding in XAML
                            
                            // Clear text boxes
                            var newSkillNameTextBox = this.FindControl<TextBox>("NewSkillNameTextBox");
                            if (newSkillNameTextBox != null)
                                newSkillNameTextBox.Text = string.Empty;
                                
                            var newVeinBlacklistItemTextBox = this.FindControl<TextBox>("NewVeinBlacklistItemTextBox");
                            if (newVeinBlacklistItemTextBox != null)
                                newVeinBlacklistItemTextBox.Text = string.Empty;
                                
                            // Reset fields
                            _newSkillName = string.Empty;
                            _newVeinBlacklistItem = string.Empty;
        }
                        
                        // Event handlers for skill modifiers
                        private void AddSkillModifier_Click(object? sender, EventArgs e)
                        {
                            if (string.IsNullOrWhiteSpace(_newSkillName) || ViewModel.ServerConfig.SkillModifiers == null)
                                return;
                                
                            if (!ViewModel.ServerConfig.SkillModifiers.ContainsKey(_newSkillName))
                            {
                                // Add to the server config dictionary
                                ViewModel.ServerConfig.SkillModifiers[_newSkillName] = 1.0;
                                
                                // Add to the observable collection
                                _skillModifiersCollection.Add(new KeyValuePairViewModel(_newSkillName, 1.0));
                                
                                ViewModel.HasFileChanges = true;
                                
                                // Clear input field
                                var newSkillNameTextBox = this.FindControl<TextBox>("NewSkillNameTextBox");
                                if (newSkillNameTextBox != null)
                                    newSkillNameTextBox.Text = string.Empty;
                                
                                _newSkillName = string.Empty;
                            }
                        }
                        
                        private void RemoveSkillModifierButton_Click(object? sender, EventArgs e)
                        {
                            if (sender is Button button && button.Tag is string skillName)
                            {
                                if (ViewModel.ServerConfig.SkillModifiers.ContainsKey(skillName))
                                {
                                    // Remove from the server config dictionary
                                    ViewModel.ServerConfig.SkillModifiers.Remove(skillName);
                                    
                                    // Remove from the observable collection
                                    var itemToRemove = _skillModifiersCollection.FirstOrDefault(x => x.Key == skillName);
                                    if (itemToRemove != null)
                                        _skillModifiersCollection.Remove(itemToRemove);
                                    
                                    ViewModel.HasFileChanges = true;
                                }
                            }
                        }
                        
                        // Event handlers for vein blacklist
                        private void AddVeinBlacklistItem_Click(object? sender, EventArgs e)
                        {
                            if (string.IsNullOrWhiteSpace(_newVeinBlacklistItem) || ViewModel.ServerConfig.VeinBlacklist == null)
                                return;
                                
                            if (!ViewModel.ServerConfig.VeinBlacklist.Contains(_newVeinBlacklistItem))
                            {
                                ViewModel.ServerConfig.VeinBlacklist.Add(_newVeinBlacklistItem);
                                ViewModel.HasFileChanges = true;
                                SyncUIWithViewModel();
                            }
                        }
                        
                        private void RemoveVeinBlacklistButton_Click(object? sender, EventArgs e)
                        {
                            if (sender is Button button && button.Tag is string item)
                            {
                                if (ViewModel.ServerConfig.VeinBlacklist.Contains(item))
                                {
                                    ViewModel.ServerConfig.VeinBlacklist.Remove(item);
                                    ViewModel.HasFileChanges = true;
                                    SyncUIWithViewModel();
                                }
                            }
                        }
                        
                        // Enable all requirements
                        private void EnableAllRequirements_Click(object? sender, EventArgs e)
                        {
                            if (ViewModel == null || ViewModel.ServerConfig == null)
                                return;
                                
                            // Use a better approach to update all checkboxes
                            UpdateAllRequirementSettings(true);
                            
                            ViewModel.HasFileChanges = true;
                            
                            // Force UI update
                            this.InvalidateVisual();
                        }
                        
                        // Helper method to update all requirement settings
                        private void UpdateAllRequirementSettings(bool enabled)
                        {
                            // Get all CheckBox controls in the requirements grid
                            var requirementsGrid = this.FindControl<Grid>("RequirementsGrid");
                            if (requirementsGrid != null)
                            {
                                // Find all checkboxes in the Requirements tab
                                foreach (var checkBox in this.GetVisualDescendants().OfType<CheckBox>())
                                {
                                    // Only update the checkboxes that are bound to requirement properties
                                    if (checkBox.DataContext == ViewModel && checkBox.IsVisible)
                                    {
                                        checkBox.IsChecked = enabled;
                                    }
                                }
                            }
                            else
                            {
                                // Fallback to direct property setting if we can't find the controls
                                var updatedConfig = ViewModel.ServerConfig;
                                
                                updatedConfig.WearReqEnabled = enabled;
                                updatedConfig.UseEnchantmentReqEnabled = enabled;
                                updatedConfig.ToolReqEnabled = enabled;
                                updatedConfig.WeaponReqEnabled = enabled;
                                updatedConfig.UseReqEnabled = enabled;
                                updatedConfig.PlaceReqEnabled = enabled;
                                updatedConfig.BreakReqEnabled = enabled;
                                updatedConfig.KillReqEnabled = enabled;
                                updatedConfig.TravelReqEnabled = enabled;
                                updatedConfig.RideReqEnabled = enabled;
                                updatedConfig.TameReqEnabled = enabled;
                                updatedConfig.BreedReqEnabled = enabled;
                                updatedConfig.InteractReqEnabled = enabled;
                                updatedConfig.EntityInteractReqEnabled = enabled;
                                
                                // Create a new instance to ensure property change notification
                                ViewModel.ServerConfig = new Models.ServerConfig
                                {
                                    // Copy all existing properties
                                    CreativeReach = updatedConfig.CreativeReach,
                                    SalvageBlock = updatedConfig.SalvageBlock,
                                    TreasureEnabled = updatedConfig.TreasureEnabled,
                                    BrewingTracked = updatedConfig.BrewingTracked,
                                    MaxLevel = updatedConfig.MaxLevel,
                                    UseExponentialFormula = updatedConfig.UseExponentialFormula,
                                    LossOnDeath = updatedConfig.LossOnDeath,
                                    LoseLevelsOnDeath = updatedConfig.LoseLevelsOnDeath,
                                    LoseOnlyExcess = updatedConfig.LoseOnlyExcess,
                                    GlobalModifier = updatedConfig.GlobalModifier,
                                    LinearBaseXP = updatedConfig.LinearBaseXP,
                                    LinearPerLevel = updatedConfig.LinearPerLevel,
                                    ExpBaseXP = updatedConfig.ExpBaseXP,
                                    ExpPowerBase = updatedConfig.ExpPowerBase,
                                    ExpPerLevel = updatedConfig.ExpPerLevel,
                                    SkillModifiers = updatedConfig.SkillModifiers,
                                    PartyRange = updatedConfig.PartyRange,
                                    PartyBonus = updatedConfig.PartyBonus,
                                    EnableMobScaling = updatedConfig.EnableMobScaling,
                                    ScalingAOE = updatedConfig.ScalingAOE,
                                    BaseLevel = updatedConfig.BaseLevel,
                                    BossScaling = updatedConfig.BossScaling,
                                    MobScalingUseExponentialFormula = updatedConfig.MobScalingUseExponentialFormula,
                                    MobScalingLinearPerLevel = updatedConfig.MobScalingLinearPerLevel,
                                    MobScalingExpPowerBase = updatedConfig.MobScalingExpPowerBase,
                                    MobScalingExpPerLevel = updatedConfig.MobScalingExpPerLevel,
                                    veinEnabled = updatedConfig.veinEnabled,
                                    RequireSettings = updatedConfig.RequireSettings,
                                    VeinMineDefaultConsume = updatedConfig.VeinMineDefaultConsume,
                                    VeinChargeModifier = updatedConfig.VeinChargeModifier,
                                    BaseChargeRate = updatedConfig.BaseChargeRate,
                                    BaseVeinCapacity = updatedConfig.BaseVeinCapacity,
                                    VeinBlacklist = updatedConfig.VeinBlacklist,
                                    ReusePenalty = updatedConfig.ReusePenalty,
                                    PerksPlusConfigs = updatedConfig.PerksPlusConfigs,
                                    
                                    // Update all requirement settings
                                    WearReqEnabled = enabled,
                                    UseEnchantmentReqEnabled = enabled,
                                    ToolReqEnabled = enabled,
                                    WeaponReqEnabled = enabled,
                                    UseReqEnabled = enabled,
                                    PlaceReqEnabled = enabled,
                                    BreakReqEnabled = enabled,
                                    KillReqEnabled = enabled,
                                    TravelReqEnabled = enabled,
                                    RideReqEnabled = enabled,
                                    TameReqEnabled = enabled,
                                    BreedReqEnabled = enabled,
                                    InteractReqEnabled = enabled,
                                    EntityInteractReqEnabled = enabled
                                };
                            }
                        }
                        
                        // Disable all requirements
                        private void DisableAllRequirements_Click(object? sender, EventArgs e)
                        {
                            if (ViewModel == null || ViewModel.ServerConfig == null)
                                return;
                            
                            // Use the helper method with false to disable all requirements
                            UpdateAllRequirementSettings(false);
                            
                            ViewModel.HasFileChanges = true;
                            
                            // Force UI update
                            this.InvalidateVisual();
                        }
                        
                        // Party Bonus methods
                        private void HandlePartyBonusListBoxButtonClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
                        {
                            if (e.Source is Button button && button.Name == "RemovePartyBonusButton" && button.Tag is string skillName)
                            {
                                RemovePartyBonusButton_Click(button, EventArgs.Empty);
                                e.Handled = true;
                            }
                        }
                        
                        private void AddPartyBonus_Click(object? sender, EventArgs e)
                        {
                            if (string.IsNullOrWhiteSpace(_newPartyBonusSkill) || ViewModel.ServerConfig.PartyBonus == null)
                                return;
                            
                            // Get the value from the NumericUpDown
                            var bonusValue = this.FindControl<NumericUpDown>("NewPartyBonusValueUpDown");
                            double value = bonusValue != null && bonusValue.Value.HasValue ? (double)bonusValue.Value.Value : 5.0;
                            
                            if (!ViewModel.ServerConfig.PartyBonus.ContainsKey(_newPartyBonusSkill))
                            {
                                // Add to the server config dictionary
                                ViewModel.ServerConfig.PartyBonus[_newPartyBonusSkill] = value;
                                ViewModel.HasFileChanges = true;
                                
                                // Clear input field
                                var newPartyBonusSkillTextBox = this.FindControl<TextBox>("NewPartyBonusSkillTextBox");
                                if (newPartyBonusSkillTextBox != null)
                                    newPartyBonusSkillTextBox.Text = string.Empty;
                                
                                _newPartyBonusSkill = string.Empty;
                                
                                // Update the ListBox
                                UpdatePartyBonusListBox();
                            }
                        }
                        
                        private void RemovePartyBonusButton_Click(object? sender, EventArgs e)
                        {
                            if (sender is Button button && button.Tag is string skillName)
                            {
                                if (ViewModel.ServerConfig.PartyBonus.ContainsKey(skillName))
                                {
                                    // Remove from the server config dictionary
                                    ViewModel.ServerConfig.PartyBonus.Remove(skillName);
                                    ViewModel.HasFileChanges = true;
                                    
                                    // Update the ListBox
                                    UpdatePartyBonusListBox();
                                }
                            }
                        }
                        
                        private void UpdatePartyBonusListBox()
                        {
                            if (ViewModel.ServerConfig.PartyBonus == null)
                                ViewModel.ServerConfig.PartyBonus = new Dictionary<string, double>();
                            
                            // Create collection of KeyValuePairViewModels
                            var items = new ObservableCollection<KeyValuePairViewModel>();
                            foreach (var kvp in ViewModel.ServerConfig.PartyBonus)
                            {
                                items.Add(new KeyValuePairViewModel(kvp.Key, kvp.Value));
                            }
                            
                            // Set the ItemsSource of the party bonus list box
                            var partyBonusListBox = this.FindControl<ListBox>("PartyBonusListBox");
                            if (partyBonusListBox != null)
                            {
                                partyBonusListBox.ItemsSource = items;
                            }
                        }
    }
}
