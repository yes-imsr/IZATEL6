using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace PMMOEdit;

public partial class LootTableEditor : UserControl
{
    // Data models for loot tables
    private class LootItem
    {
        public string Type { get; set; } = "minecraft:item";
        public string Name { get; set; } = "minecraft:stone";
        public int Weight { get; set; } = 1;
        public List<LootFunction> Functions { get; set; } = new List<LootFunction>();
        public List<LootCondition> Conditions { get; set; } = new List<LootCondition>();
        
        // For UI display
        public double Chance { get; set; }
        public string DisplayName => Name.Replace("minecraft:", "");
    }
    
    private class LootFunction
    {
        public string Function { get; set; }
        public object Count { get; set; }
    }
    
    private class LootCondition
    {
        public string Condition { get; set; }
        public double? Chance { get; set; }
        public double? LootingMultiplier { get; set; }
    }
    
    private class LootPool
    {
        public object Rolls { get; set; } = 1;
        public List<LootItem> Entries { get; set; } = new List<LootItem>();
    }
    
    private class LootTable
    {
        public string Type { get; set; } = "minecraft:entity";
        public List<LootPool> Pools { get; set; } = new List<LootPool>();
    }
    
    // Editor state
    private string _datapackPath;
    private string _datapackName;
    private string _currentLootTablePath;
    private LootTable _currentLootTable;
    private bool _hasUnsavedChanges;
    
    // Common Minecraft items for quick selection
    private readonly Dictionary<string, string> _commonItems = new()
    {
        { "minecraft:stone", "Stone" },
        { "minecraft:dirt", "Dirt" },
        { "minecraft:cobblestone", "Cobblestone" },
        { "minecraft:oak_log", "Oak Log" },
        { "minecraft:coal", "Coal" },
        { "minecraft:iron_ingot", "Iron Ingot" },
        { "minecraft:gold_ingot", "Gold Ingot" },
        { "minecraft:diamond", "Diamond" },
        { "minecraft:emerald", "Emerald" },
        { "minecraft:redstone", "Redstone" },
        { "minecraft:rotten_flesh", "Rotten Flesh" },
        { "minecraft:bone", "Bone" },
        { "minecraft:string", "String" },
        { "minecraft:gunpowder", "Gunpowder" },
        { "minecraft:spider_eye", "Spider Eye" },
        { "minecraft:ender_pearl", "Ender Pearl" },
        { "minecraft:blaze_rod", "Blaze Rod" },
        { "minecraft:experience_bottle", "Experience Bottle" },
        { "minecraft:iron_sword", "Iron Sword" },
        { "minecraft:golden_apple", "Golden Apple" }
    };
    
    public LootTableEditor()
    {
        InitializeComponent();
        
        // Set up event handlers
        SetupEventHandlers();
        
        // Initialize with a default loot table
        _currentLootTable = new LootTable
        {
            Type = "minecraft:entity",
            Pools = new List<LootPool>
            {
                new LootPool
                {
                    Rolls = 1,
                    Entries = new List<LootItem>
                    {
                        new LootItem
                        {
                            Type = "minecraft:item",
                            Name = "minecraft:rotten_flesh",
                            Weight = 10,
                            Functions = new List<LootFunction>
                            {
                                new LootFunction
                                {
                                    Function = "minecraft:set_count",
                                    Count = new { min = 0, max = 2, type = "minecraft:uniform" }
                                },
                                new LootFunction
                                {
                                    Function = "minecraft:looting_enchant",
                                    Count = new { min = 0, max = 1 }
                                }
                            }
                        },
                        new LootItem
                        {
                            Type = "minecraft:item",
                            Name = "minecraft:iron_ingot",
                            Weight = 1,
                            Conditions = new List<LootCondition>
                            {
                                new LootCondition
                                {
                                    Condition = "minecraft:random_chance_with_looting",
                                    Chance = 0.025,
                                    LootingMultiplier = 0.01
                                }
                            }
                        },
                        new LootItem
                        {
                            Type = "minecraft:item",
                            Name = "minecraft:emerald",
                            Weight = 4
                        }
                    }
                }
            }
        };
        
        // Calculate chances for initial display
        UpdateItemChances();
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    private void SetupEventHandlers()
    {
        var helpButton = this.FindControl<Button>("HelpButton");
        if (helpButton != null)
        {
            helpButton.Click += HelpButton_Click;
        }
        
        var newLootTableButton = this.FindControl<Button>("NewLootTableButton");
        if (newLootTableButton != null)
        {
            newLootTableButton.Click += NewLootTableButton_Click;
        }
        
        var lootTableTypeSelector = this.FindControl<ComboBox>("LootTableTypeSelector");
        if (lootTableTypeSelector != null)
        {
            lootTableTypeSelector.SelectionChanged += LootTableTypeSelector_SelectionChanged;
        }
        
        var treeView = this.FindControl<TreeView>("LootTableTreeView");
        if (treeView != null)
        {
            treeView.SelectionChanged += TreeView_SelectionChanged;
        }
        
        var saveButton = this.FindControl<Button>("SaveButton");
        if (saveButton != null)
        {
            saveButton.Click += SaveButton_Click;
        }
        
        var testButton = this.FindControl<Button>("TestButton");
        if (testButton != null)
        {
            testButton.Click += TestButton_Click;
        }
        
        var contextTypeComboBox = this.FindControl<ComboBox>("LootTableContextType");
        if (contextTypeComboBox != null)
        {
            contextTypeComboBox.SelectionChanged += ContextType_SelectionChanged;
        }
    }
    
    public void Initialize(string datapackPath, string datapackName)
    {
        _datapackPath = datapackPath;
        _datapackName = datapackName;
        
        // Update UI with datapack info
        var datapackNameDisplay = this.FindControl<TextBlock>("DatapackNameDisplay");
        if (datapackNameDisplay != null)
        {
            datapackNameDisplay.Text = $" - {datapackName}";
        }
        
        LoadExistingLootTables();
    }
    
    private void LoadExistingLootTables()
    {
        if (string.IsNullOrEmpty(_datapackPath) || !Directory.Exists(_datapackPath))
        {
            return;
        }
        
        var namespacePath = Path.Combine(_datapackPath, "data", _datapackName.ToLower());
        var lootTablesPath = Path.Combine(namespacePath, "loot_tables");
        
        if (!Directory.Exists(lootTablesPath))
        {
            Directory.CreateDirectory(lootTablesPath);
        }
        
        var treeView = this.FindControl<TreeView>("LootTableTreeView");
        if (treeView != null)
        {
            treeView.Items.Clear();
            
            // Entity loot tables
            var entityLootTablesPath = Path.Combine(lootTablesPath, "entities");
            AddLootTableCategory(treeView, "Entity Loot Tables", entityLootTablesPath);
            
            // Chest loot tables
            var chestLootTablesPath = Path.Combine(lootTablesPath, "chests");
            AddLootTableCategory(treeView, "Chest Loot Tables", chestLootTablesPath);
            
            // Block loot tables
            var blockLootTablesPath = Path.Combine(lootTablesPath, "blocks");
            AddLootTableCategory(treeView, "Block Loot Tables", blockLootTablesPath);
        }
    }
    
    private void AddLootTableCategory(TreeView treeView, string categoryName, string path)
    {
        var category = new TreeViewItem { Header = categoryName, IsExpanded = true };
        
        if (Directory.Exists(path))
        {
            var files = Directory.GetFiles(path, "*.json");
            foreach (var file in files)
            {
                category.Items.Add(new TreeViewItem 
                { 
                    Header = Path.GetFileName(file),
                    Tag = file // Store the full file path for later use
                });
            }
        }
        else
        {
            Directory.CreateDirectory(path);
        }
        
        category.Items.Add(new TreeViewItem { Header = "+ Add New...", Tag = "add_new" });
        treeView.Items.Add(category);
    }
    
    private async Task LoadLootTable(string filePath)
    {
        try
        {
            _currentLootTablePath = filePath;
            
            // Read the JSON file
            string jsonContent = File.ReadAllText(filePath);
            _currentLootTable = JsonSerializer.Deserialize<LootTable>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            // Update the UI
            UpdateUI();
            
            // Reset unsaved changes flag
            _hasUnsavedChanges = false;
        }
        catch (Exception ex)
        {
            await ShowError($"Error loading loot table: {ex.Message}");
        }
    }
    
    private async Task SaveLootTable()
    {
        if (_currentLootTable == null || string.IsNullOrEmpty(_currentLootTablePath))
        {
            await ShowError("No loot table to save or file path not set.");
            return;
        }
        
        try
        {
            // Make sure directory exists
            string dirPath = Path.GetDirectoryName(_currentLootTablePath);
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            
            // Serialize the loot table
            string json = JsonSerializer.Serialize(_currentLootTable, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            
            // Write to file
            await File.WriteAllTextAsync(_currentLootTablePath, json);
            
            // Update the file name in case it was changed
            var titleTextBox = this.FindControl<TextBox>("LootTableTitleTextBox");
            if (titleTextBox != null)
            {
                string newFileName = titleTextBox.Text.Trim();
                if (!string.IsNullOrEmpty(newFileName) && !newFileName.Equals(Path.GetFileName(_currentLootTablePath)))
                {
                    string fileDirPath = Path.GetDirectoryName(_currentLootTablePath);
                    string newPath = Path.Combine(fileDirPath, newFileName);
                    
                    if (!newFileName.EndsWith(".json"))
                    {
                        newPath += ".json";
                    }
                    
                    if (File.Exists(newPath) && !newPath.Equals(_currentLootTablePath, StringComparison.OrdinalIgnoreCase))
                    {
                        await ShowError("A file with this name already exists. Please choose a different name.");
                        return;
                    }
                    
                    // Rename file
                    File.Move(_currentLootTablePath, newPath);
                    _currentLootTablePath = newPath;
                    
                    // Refresh tree view
                    LoadExistingLootTables();
                }
            }
            
            // Reset unsaved changes flag
            _hasUnsavedChanges = false;
        }
        catch (Exception ex)
        {
            await ShowError($"Error saving loot table: {ex.Message}");
        }
    }
    
    private void UpdateUI()
    {
        if (_currentLootTable == null)
            return;
        
        // Update file name
        var titleTextBox = this.FindControl<TextBox>("LootTableTitleTextBox");
        if (titleTextBox != null && !string.IsNullOrEmpty(_currentLootTablePath))
        {
            titleTextBox.Text = Path.GetFileName(_currentLootTablePath);
        }
        
        // Update context type
        var contextTypeComboBox = this.FindControl<ComboBox>("LootTableContextType");
        if (contextTypeComboBox != null)
        {
            switch (_currentLootTable.Type)
            {
                case "minecraft:entity":
                    contextTypeComboBox.SelectedIndex = 0;
                    break;
                case "minecraft:chest":
                    contextTypeComboBox.SelectedIndex = 1;
                    break;
                case "minecraft:block":
                    contextTypeComboBox.SelectedIndex = 2;
                    break;
                case "minecraft:fishing":
                    contextTypeComboBox.SelectedIndex = 3;
                    break;
            }
        }
        
        // Calculate chances
        UpdateItemChances();
        
        // Update JSON editor tab
        UpdateJsonEditor();
        
        // Update visualization
        UpdateVisualization();
    }
    
    private void UpdateItemChances()
    {
        if (_currentLootTable?.Pools == null || _currentLootTable.Pools.Count == 0)
            return;
            
        foreach (var pool in _currentLootTable.Pools)
        {
            int totalWeight = pool.Entries.Sum(e => e.Weight);
            foreach (var entry in pool.Entries)
            {
                entry.Chance = (double)entry.Weight / totalWeight * 100;
            }
        }
    }
    
    private void UpdateJsonEditor()
    {
        // This is a simplified JSON serialization for the display
        var jsonEditor = this.FindControl<TextBox>("JsonEditor");
        if (jsonEditor != null && _currentLootTable != null)
        {
            string json = JsonSerializer.Serialize(_currentLootTable, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            
            // Replace escaped quotes with actual quotes for readability
            jsonEditor.Text = json;
        }
    }
    
    private void UpdateVisualization()
    {
        if (_currentLootTable?.Pools == null || _currentLootTable.Pools.Count == 0)
            return;
            
        // For simplicity, we'll just visualize the first pool
        var pool = _currentLootTable.Pools[0];
        var items = pool.Entries.OrderByDescending(e => e.Weight).ToList();
        
        // In a real implementation, you would dynamically create the visualization
        // based on the actual loot table items and weights
        
        // Update average items calculation
        double averageItems = 1.0; // Default for fixed rolls of 1
        if (pool.Rolls is int intRolls)
        {
            averageItems = intRolls;
        }
        
        var averageItemsText = this.FindControl<TextBlock>("AverageItemsText");
        if (averageItemsText != null)
        {
            averageItemsText.Text = $"Average items per roll: {averageItems}";
        }
    }
    
    private async Task ShowError(string message)
    {
        var dialog = new Window
        {
            Title = "Error",
            Width = 400,
            Height = 150,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };
        
        var panel = new StackPanel
        {
            Margin = new Thickness(20)
        };
        
        panel.Children.Add(new TextBlock
        {
            Text = message,
            TextWrapping = TextWrapping.Wrap
        });
        
        var okButton = new Button
        {
            Content = "OK",
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Margin = new Thickness(0, 20, 0, 0),
            Width = 80
        };
        
        okButton.Click += (s, e) => dialog.Close();
        panel.Children.Add(okButton);
        
        dialog.Content = panel;
        
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is Window parentWindow)
        {
            await dialog.ShowDialog(parentWindow);
        }
        else
        {
            dialog.Show();
        }
    }
    
    // Event handlers for UI elements
    
    private async void HelpButton_Click(object sender, RoutedEventArgs e)
    {
        var helpWindow = new Window
        {
            Title = "Loot Table Help",
            Width = 600,
            Height = 500,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };
        
        var scrollViewer = new ScrollViewer();
        var helpPanel = new StackPanel
        {
            Margin = new Thickness(20)
        };
        
        // Help title
        helpPanel.Children.Add(new TextBlock
        {
            Text = "Loot Table Editor Help",
            FontSize = 22,
            FontWeight = FontWeight.Bold,
            Margin = new Thickness(0, 0, 0, 15)
        });
        
        // Basic explanation
        helpPanel.Children.Add(new TextBlock
        {
            Text = "Loot tables control what items are dropped when entities die, chests are opened, or blocks are broken.",
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 10)
        });
        
        // Basic concepts section
        helpPanel.Children.Add(new TextBlock
        {
            Text = "Basic Concepts",
            FontSize = 18,
            FontWeight = FontWeight.Bold,
            Margin = new Thickness(0, 10, 0, 5)
        });
        
        helpPanel.Children.Add(new TextBlock
        {
            Text = "• Pools: Groups of items that can drop together. Each pool is processed independently.",
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 5, 0, 0)
        });
        
        helpPanel.Children.Add(new TextBlock
        {
            Text = "• Rolls: How many times to pick from a pool. Can be a fixed number or a range.",
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 5, 0, 0)
        });
        
        helpPanel.Children.Add(new TextBlock
        {
            Text = "• Entries: Items that can be chosen in a roll. Each has a weight (relative chance).",
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 5, 0, 0)
        });
        
        helpPanel.Children.Add(new TextBlock
        {
            Text = "• Weight: The relative chance of an item being selected. Higher weight = more common.",
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 5, 0, 0)
        });
        
        helpPanel.Children.Add(new TextBlock
        {
            Text = "• Functions: Modify items after they're selected (e.g., set count, apply enchantments).",
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 5, 0, 10)
        });
        
        // Example section
        helpPanel.Children.Add(new TextBlock
        {
            Text = "Example Calculation",
            FontSize = 18,
            FontWeight = FontWeight.Bold,
            Margin = new Thickness(0, 10, 0, 5)
        });
        
        helpPanel.Children.Add(new TextBlock
        {
            Text = "If you have 3 items with weights 10, 4, and 1, their percentage chances are:",
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 5, 0, 0)
        });
        
        helpPanel.Children.Add(new TextBlock
        {
            Text = "• Item 1 (weight 10): 10 ÷ (10+4+1) = 10 ÷ 15 = 67%",
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(10, 5, 0, 0)
        });
        
        helpPanel.Children.Add(new TextBlock
        {
            Text = "• Item 2 (weight 4): 4 ÷ 15 = 27%",
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(10, 5, 0, 0)
        });
        
        helpPanel.Children.Add(new TextBlock
        {
            Text = "• Item 3 (weight 1): 1 ÷ 15 = 6%",
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(10, 5, 0, 10)
        });
        
        // Common functions section
        helpPanel.Children.Add(new TextBlock
        {
            Text = "Common Loot Functions",
            FontSize = 18,
            FontWeight = FontWeight.Bold,
            Margin = new Thickness(0, 10, 0, 5)
        });
        
        helpPanel.Children.Add(new TextBlock
        {
            Text = "• set_count: Controls how many of an item drop",
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 5, 0, 0)
        });
        
        helpPanel.Children.Add(new TextBlock
        {
            Text = "• looting_enchant: Increases drops based on the looting enchantment level",
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 5, 0, 0)
        });
        
        helpPanel.Children.Add(new TextBlock
        {
            Text = "• enchant_randomly: Adds random enchantments to the item",
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 5, 0, 0)
        });
        
        helpPanel.Children.Add(new TextBlock
        {
            Text = "• random_chance: Only applies the drop some percentage of the time",
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 5, 0, 10)
        });
        
        // Close button
        var closeButton = new Button
        {
            Content = "Close",
            Width = 100,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Padding = new Thickness(10, 5),
            Background = new SolidColorBrush(Color.Parse("#3498DB")),
            Foreground = new SolidColorBrush(Colors.White)
        };
        
        closeButton.Click += (s, args) => helpWindow.Close();
        helpPanel.Children.Add(closeButton);
        
        scrollViewer.Content = helpPanel;
        helpWindow.Content = scrollViewer;
        
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is Window parentWindow)
        {
            await helpWindow.ShowDialog(parentWindow);
        }
        else
        {
            helpWindow.Show();
        }
    }
    
    private async void NewLootTableButton_Click(object sender, RoutedEventArgs e)
    {
        var lootTableTypeSelector = this.FindControl<ComboBox>("LootTableTypeSelector");
        string category = "entities";
        
        if (lootTableTypeSelector != null && lootTableTypeSelector.SelectedItem != null)
        {
            var selectedItem = lootTableTypeSelector.SelectedItem as ComboBoxItem;
            if (selectedItem != null)
            {
                string headerText = selectedItem.Content.ToString();
                if (headerText.Contains("Chest"))
                {
                    category = "chests";
                }
                else if (headerText.Contains("Block"))
                {
                    category = "blocks";
                }
            }
        }
        
        // Create dialog to get new file name
        var dialog = new Window
        {
            Title = "New Loot Table",
            Width = 400,
            Height = 200,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };
        
        var panel = new StackPanel
        {
            Margin = new Thickness(20)
        };
        
        panel.Children.Add(new TextBlock
        {
            Text = "Enter loot table name:",
            Margin = new Thickness(0, 0, 0, 10)
        });
        
        var nameTextBox = new TextBox
        {
            Text = "new_loot_table",
            Margin = new Thickness(0, 0, 0, 20)
        };
        panel.Children.Add(nameTextBox);
        
        var buttonPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Spacing = 10
        };
        
        var cancelButton = new Button
        {
            Content = "Cancel",
            Width = 100
        };
        
        var createButton = new Button
        {
            Content = "Create",
            Width = 100,
            Background = new SolidColorBrush(Color.Parse("#27AE60")),
            Foreground = new SolidColorBrush(Colors.White)
        };
        
        buttonPanel.Children.Add(cancelButton);
        buttonPanel.Children.Add(createButton);
        panel.Children.Add(buttonPanel);
        
        dialog.Content = panel;
        
        cancelButton.Click += (s, args) => dialog.Close();
        
        var tcs = new TaskCompletionSource<string>();
        
        createButton.Click += (s, args) =>
        {
            string fileName = nameTextBox.Text.Trim();
            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }
            
            if (!fileName.EndsWith(".json"))
            {
                fileName += ".json";
            }
            
            tcs.SetResult(fileName);
            dialog.Close();
        };
        
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is Window parentWindow)
        {
            await dialog.ShowDialog(parentWindow);
        }
        else
        {
            dialog.Show();
        }
        
        try
        {
            string fileName = await tcs.Task;
            
            // Create the loot table file path
            var namespacePath = Path.Combine(_datapackPath, "data", _datapackName.ToLower());
            var lootTablesPath = Path.Combine(namespacePath, "loot_tables", category);
            
            if (!Directory.Exists(lootTablesPath))
            {
                Directory.CreateDirectory(lootTablesPath);
            }
            
            string filePath = Path.Combine(lootTablesPath, fileName);
            
            // Create new default loot table
            _currentLootTable = new LootTable
            {
                Type = category == "entities" ? "minecraft:entity" : 
                       category == "chests" ? "minecraft:chest" : "minecraft:block",
                Pools = new List<LootPool>
                {
                    new LootPool
                    {
                        Rolls = 1,
                        Entries = new List<LootItem>
                        {
                            new LootItem
                            {
                                Type = "minecraft:item",
                                Name = "minecraft:stone",
                                Weight = 1
                            }
                        }
                    }
                }
            };
            
            // Save to file
            _currentLootTablePath = filePath;
            await SaveLootTable();
            
            // Update UI
            UpdateUI();
            
            // Refresh tree view
            LoadExistingLootTables();
        }
        catch (Exception ex)
        {
            await ShowError($"Error creating loot table: {ex.Message}");
        }
    }
    
    private void LootTableTypeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // This event is for the type selector in the sidebar
        // Update the tree view to show relevant loot tables
        if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem selectedItem)
        {
            var treeView = this.FindControl<TreeView>("LootTableTreeView");
            if (treeView != null)
            {
                string selectedType = selectedItem.Content.ToString();
                
                foreach (var item in treeView.Items)
                {
                    if (item is TreeViewItem treeViewItem)
                    {
                        if (treeViewItem.Header.ToString().Contains(selectedType))
                        {
                            treeViewItem.IsVisible = true;
                            treeViewItem.IsExpanded = true;
                        }
                        else
                        {
                            treeViewItem.IsVisible = false;
                        }
                    }
                }
            }
        }
    }
    
    private async void TreeView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is TreeView treeView && treeView.SelectedItem is TreeViewItem selectedItem)
        {
            if (selectedItem.Tag is string tag)
            {
                if (tag == "add_new")
                {
                    // User selected "Add New..." entry - trigger the new loot table dialog
                    NewLootTableButton_Click(sender, e);
                    return;
                }
                
                if (!string.IsNullOrEmpty(tag) && File.Exists(tag))
                {
                    // Load the selected loot table
                    await LoadLootTable(tag);
                }
            }
        }
    }
    
    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        await SaveLootTable();
    }
    
    private async void TestButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentLootTable == null)
        {
            await ShowError("No loot table to test.");
            return;
        }
        
        // Simulate loot drops
        var resultWindow = new Window
        {
            Title = "Loot Table Test Results",
            Width = 500,
            Height = 400,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };
        
        var panel = new StackPanel
        {
            Margin = new Thickness(20)
        };
        
        // Add header
        panel.Children.Add(new TextBlock
        {
            Text = "Simulated Loot Drops",
            FontSize = 20,
            FontWeight = FontWeight.Bold,
            Margin = new Thickness(0, 0, 0, 15)
        });
        
        // Add description
        panel.Children.Add(new TextBlock
        {
            Text = "This simulation shows 10 rolls of your loot table to demonstrate what players might receive.",
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 15)
        });
        
        // Create a simple grid to display simulated rolls
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        grid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
        
        // Add header row
        grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        
        var rollHeader = new TextBlock
        {
            Text = "Roll",
            FontWeight = FontWeight.Bold,
            Margin = new Thickness(5)
        };
        
        var itemsHeader = new TextBlock
        {
            Text = "Items Dropped",
            FontWeight = FontWeight.Bold,
            Margin = new Thickness(5)
        };
        
        Grid.SetRow(rollHeader, 0);
        Grid.SetColumn(rollHeader, 0);
        
        Grid.SetRow(itemsHeader, 0);
        Grid.SetColumn(itemsHeader, 1);
        
        grid.Children.Add(rollHeader);
        grid.Children.Add(itemsHeader);
        
        // Simulate 10 rolls
        var random = new Random();
        for (int i = 0; i < 10; i++)
        {
            grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            
            var rollNumber = new TextBlock
            {
                Text = $"Roll {i + 1}",
                Margin = new Thickness(5),
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            };
            
            var result = new TextBlock
            {
                Text = SimulateLootRoll(random),
                Margin = new Thickness(5),
                TextWrapping = TextWrapping.Wrap
            };
            
            Grid.SetRow(rollNumber, i + 1);
            Grid.SetColumn(rollNumber, 0);
            
            Grid.SetRow(result, i + 1);
            Grid.SetColumn(result, 1);
            
            grid.Children.Add(rollNumber);
            grid.Children.Add(result);
        }
        
        // Add grid to a border for better visual appearance
        var border = new Border
        {
            BorderBrush = new SolidColorBrush(Color.Parse("#CCCCCC")),
            BorderThickness = new Thickness(1),
            Margin = new Thickness(0, 0, 0, 15),
            Child = grid
        };
        
        panel.Children.Add(border);
        
        // Add close button
        var closeButton = new Button
        {
            Content = "Close",
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Width = 100,
            Padding = new Thickness(10, 5),
            Background = new SolidColorBrush(Color.Parse("#3498DB")),
            Foreground = new SolidColorBrush(Colors.White)
        };
        
        closeButton.Click += (s, args) => resultWindow.Close();
        panel.Children.Add(closeButton);
        
        // Wrap in scroll viewer in case of many results
        var scrollViewer = new ScrollViewer
        {
            Content = panel
        };
        
        resultWindow.Content = scrollViewer;
        
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is Window parentWindow)
        {
            await resultWindow.ShowDialog(parentWindow);
        }
        else
        {
            resultWindow.Show();
        }
    }
    
    private string SimulateLootRoll(Random random)
    {
        // This is a simplified simulation - a real one would properly handle all loot table mechanics
        if (_currentLootTable?.Pools == null || _currentLootTable.Pools.Count == 0)
            return "No items";
        
        var pool = _currentLootTable.Pools[0];
        
        // Determine number of rolls
        int rolls = 1;
        if (pool.Rolls is int intRolls)
        {
            rolls = intRolls;
        }
        else if (pool.Rolls is JsonElement element)
        {
            // Handle min/max range rolling
            if (element.TryGetProperty("min", out var minElement) && 
                element.TryGetProperty("max", out var maxElement))
            {
                int min = minElement.GetInt32();
                int max = maxElement.GetInt32();
                rolls = random.Next(min, max + 1);
            }
        }
        
        // No entries, return empty result
        if (pool.Entries == null || pool.Entries.Count == 0)
            return "No items";
        
        // Calculate total weight
        int totalWeight = pool.Entries.Sum(e => e.Weight);
        
        // Perform rolls
        List<string> results = new List<string>();
        for (int i = 0; i < rolls; i++)
        {
            // Select an item based on weight
            int roll = random.Next(1, totalWeight + 1);
            int weightSum = 0;
            
            foreach (var entry in pool.Entries)
            {
                weightSum += entry.Weight;
                if (roll <= weightSum)
                {
                    // Item was selected
                    string itemName = entry.DisplayName;
                    
                    // Determine count (simplified)
                    int count = 1;
                    var setCountFunction = entry.Functions?
                        .FirstOrDefault(f => f.Function == "minecraft:set_count");
                    
                    if (setCountFunction != null && setCountFunction.Count != null)
                    {
                        // Handle various count specifications (simplified)
                        if (setCountFunction.Count is JsonElement countElement)
                        {
                            if (countElement.TryGetProperty("min", out var minElement) && 
                                countElement.TryGetProperty("max", out var maxElement))
                            {
                                int min = minElement.GetInt32();
                                int max = maxElement.GetInt32();
                                count = random.Next(min, max + 1);
                            }
                        }
                    }
                    
                    results.Add($"{count}x {itemName}");
                    break;
                }
            }
        }
        
        return results.Count > 0 ? string.Join(", ", results) : "No items";
    }
    
    private void ContextType_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem selectedItem)
        {
            if (_currentLootTable != null)
            {
                string content = selectedItem.Content.ToString();
                _currentLootTable.Type = content;
                
                // Mark that we have unsaved changes
                _hasUnsavedChanges = true;
            }
        }
    }
    
    // Additional methods for item manipulation
    
    private async Task AddItemToCurrentPool(string itemName = null)
    {
        if (_currentLootTable == null)
        {
            await ShowError("No loot table is currently open.");
            return;
        }
        
        if (_currentLootTable.Pools == null || _currentLootTable.Pools.Count == 0)
        {
            _currentLootTable.Pools = new List<LootPool>
            {
                new LootPool
                {
                    Rolls = 1,
                    Entries = new List<LootItem>()
                }
            };
        }
        
        // If no item name was provided, show item selection dialog
        if (string.IsNullOrEmpty(itemName))
        {
            itemName = await ShowItemSelectionDialog();
            if (string.IsNullOrEmpty(itemName))
            {
                // User canceled
                return;
            }
        }
        
        // Add the new item to the first pool
        _currentLootTable.Pools[0].Entries.Add(new LootItem
        {
            Type = "minecraft:item",
            Name = itemName,
            Weight = 1
        });
        
        // Update the UI
        UpdateItemChances();
        UpdateUI();
        
        _hasUnsavedChanges = true;
    }
    
    private async Task<string> ShowItemSelectionDialog()
    {
        var dialog = new Window
        {
            Title = "Select Item",
            Width = 400,
            Height = 300,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };
        
        var panel = new StackPanel
        {
            Margin = new Thickness(20)
        };
        
        panel.Children.Add(new TextBlock
        {
            Text = "Choose an item or enter a custom item ID:",
            Margin = new Thickness(0, 0, 0, 10)
        });
        
        // Item search/input box
        var itemTextBox = new TextBox
        {
            Watermark = "minecraft:item_id",
            Margin = new Thickness(0, 0, 0, 10)
        };
        panel.Children.Add(itemTextBox);
        
        // Common items list
        var commonItemsListBox = new ListBox
        {
            MaxHeight = 150,
            Margin = new Thickness(0, 0, 0, 10)
        };
        
        foreach (var item in _commonItems)
        {
            commonItemsListBox.Items.Add(new TextBlock { Text = $"{item.Value} ({item.Key})" });
        }
        
        panel.Children.Add(new TextBlock
        {
            Text = "Common Items:",
            Margin = new Thickness(0, 0, 0, 5)
        });
        
        panel.Children.Add(commonItemsListBox);
        
        // When an item is selected from the list, populate the text box
        commonItemsListBox.SelectionChanged += (s, e) =>
        {
            if (commonItemsListBox.SelectedItem is TextBlock selectedText)
            {
                string text = selectedText.Text;
                int startIndex = text.IndexOf('(') + 1;
                int endIndex = text.IndexOf(')');
                
                if (startIndex > 0 && endIndex > startIndex)
                {
                    string itemId = text.Substring(startIndex, endIndex - startIndex);
                    itemTextBox.Text = itemId;
                }
            }
        };
        
        var buttonPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Spacing = 10,
            Margin = new Thickness(0, 10, 0, 0)
        };
        
        var cancelButton = new Button
        {
            Content = "Cancel",
            Width = 100
        };
        
        var addButton = new Button
        {
            Content = "Add Item",
            Width = 100,
            Background = new SolidColorBrush(Color.Parse("#27AE60")),
            Foreground = new SolidColorBrush(Colors.White),
            IsEnabled = false
        };
        
        // Enable the add button when there's text
        itemTextBox.TextChanged += (s, e) =>
        {
            addButton.IsEnabled = !string.IsNullOrWhiteSpace(itemTextBox.Text);
        };
        
        buttonPanel.Children.Add(cancelButton);
        buttonPanel.Children.Add(addButton);
        panel.Children.Add(buttonPanel);
        
        dialog.Content = panel;
        
        var tcs = new TaskCompletionSource<string>();
        
        cancelButton.Click += (s, e) =>
        {
            tcs.SetResult(null);
            dialog.Close();
        };
        
        addButton.Click += (s, e) =>
        {
            string item = itemTextBox.Text?.Trim();
            if (!string.IsNullOrEmpty(item))
            {
                // Ensure the item ID has the minecraft: prefix if not provided
                if (!item.Contains(':'))
                {
                    item = "minecraft:" + item;
                }
                
                tcs.SetResult(item);
                dialog.Close();
            }
        };
        
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is Window parentWindow)
        {
            await dialog.ShowDialog(parentWindow);
        }
        else
        {
            dialog.Show();
        }
        
        return await tcs.Task;
    }
    
    private async Task AddLootPool()
    {
        if (_currentLootTable == null)
        {
            await ShowError("No loot table is currently open.");
            return;
        }
        
        if (_currentLootTable.Pools == null)
        {
            _currentLootTable.Pools = new List<LootPool>();
        }
        
        // Add a new empty pool with a default item
        _currentLootTable.Pools.Add(new LootPool
        {
            Rolls = 1,
            Entries = new List<LootItem>
            {
                new LootItem
                {
                    Type = "minecraft:item",
                    Name = "minecraft:stone",
                    Weight = 1
                }
            }
        });
        
        // Update UI
        UpdateUI();
        
        _hasUnsavedChanges = true;
    }
    
    private async Task RemoveItemFromPool(int poolIndex, int itemIndex)
    {
        if (_currentLootTable?.Pools == null || poolIndex >= _currentLootTable.Pools.Count)
        {
            await ShowError("Invalid pool index.");
            return;
        }
        
        var pool = _currentLootTable.Pools[poolIndex];
        if (pool.Entries == null || itemIndex >= pool.Entries.Count)
        {
            await ShowError("Invalid item index.");
            return;
        }
        
        // We need at least one item in a pool
        if (pool.Entries.Count <= 1)
        {
            await ShowError("Cannot remove the last item from a pool. Add another item first or remove the entire pool.");
            return;
        }
        
        // Remove the item
        pool.Entries.RemoveAt(itemIndex);
        
        // Update UI
        UpdateItemChances();
        UpdateUI();
        
        _hasUnsavedChanges = true;
    }
    
    private async Task RemoveLootPool(int poolIndex)
    {
        if (_currentLootTable?.Pools == null || poolIndex >= _currentLootTable.Pools.Count)
        {
            await ShowError("Invalid pool index.");
            return;
        }
        
        // We need at least one pool in a loot table
        if (_currentLootTable.Pools.Count <= 1)
        {
            await ShowError("Cannot remove the last pool. Add another pool first.");
            return;
        }
        
        // Remove the pool
        _currentLootTable.Pools.RemoveAt(poolIndex);
        
        // Update UI
        UpdateUI();
        
        _hasUnsavedChanges = true;
    }
    
    private async Task EditItemWeight(int poolIndex, int itemIndex)
    {
        if (_currentLootTable?.Pools == null || poolIndex >= _currentLootTable.Pools.Count)
        {
            await ShowError("Invalid pool index.");
            return;
        }
        
        var pool = _currentLootTable.Pools[poolIndex];
        if (pool.Entries == null || itemIndex >= pool.Entries.Count)
        {
            await ShowError("Invalid item index.");
            return;
        }
        
        var item = pool.Entries[itemIndex];
        
        // Create a dialog to edit weight
        var dialog = new Window
        {
            Title = "Edit Item Weight",
            Width = 350,
            Height = 200,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };
        
        var panel = new StackPanel
        {
            Margin = new Thickness(20)
        };
        
        panel.Children.Add(new TextBlock
        {
            Text = $"Edit weight for {item.DisplayName}:",
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 10)
        });
        
        var weightNumeric = new NumericUpDown
        {
            Value = item.Weight,
            Minimum = 1,
            Maximum = 1000,
            Increment = 1,
            FormatString = "0",
            Margin = new Thickness(0, 0, 0, 20)
        };
        panel.Children.Add(weightNumeric);
        
        var buttonPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Spacing = 10
        };
        
        var cancelButton = new Button
        {
            Content = "Cancel",
            Width = 100
        };
        
        var saveButton = new Button
        {
            Content = "Save",
            Width = 100,
            Background = new SolidColorBrush(Color.Parse("#27AE60")),
            Foreground = new SolidColorBrush(Colors.White)
        };
        
        buttonPanel.Children.Add(cancelButton);
        buttonPanel.Children.Add(saveButton);
        panel.Children.Add(buttonPanel);
        
        dialog.Content = panel;
        
        var tcs = new TaskCompletionSource<int?>();
        
        cancelButton.Click += (s, e) =>
        {
            tcs.SetResult(null);
            dialog.Close();
        };
        
        saveButton.Click += (s, e) =>
        {
            tcs.SetResult((int)weightNumeric.Value);
            dialog.Close();
        };
        
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is Window parentWindow)
        {
            await dialog.ShowDialog(parentWindow);
        }
        else
        {
            dialog.Show();
        }
        
        var newWeight = await tcs.Task;
        if (newWeight.HasValue)
        {
            // Update weight and UI
            item.Weight = newWeight.Value;
            UpdateItemChances();
            UpdateUI();
            
            _hasUnsavedChanges = true;
        }
    }
    
    private async Task AddItemFunction(int poolIndex, int itemIndex, string functionType)
    {
        if (_currentLootTable?.Pools == null || poolIndex >= _currentLootTable.Pools.Count)
        {
            await ShowError("Invalid pool index.");
            return;
        }
        
        var pool = _currentLootTable.Pools[poolIndex];
        if (pool.Entries == null || itemIndex >= pool.Entries.Count)
        {
            await ShowError("Invalid item index.");
            return;
        }
        
        var item = pool.Entries[itemIndex];
        
        // Initialize the Functions collection if it doesn't exist
        if (item.Functions == null)
        {
            item.Functions = new List<LootFunction>();
        }
        
        // Add function based on type
        switch (functionType)
        {
            case "set_count":
                item.Functions.Add(new LootFunction
                {
                    Function = "minecraft:set_count",
                    Count = new { min = 1, max = 3, type = "minecraft:uniform" }
                });
                break;
                
            case "looting_enchant":
                item.Functions.Add(new LootFunction
                {
                    Function = "minecraft:looting_enchant",
                    Count = new { min = 0, max = 1 }
                });
                break;
                
            case "enchant_randomly":
                item.Functions.Add(new LootFunction
                {
                    Function = "minecraft:enchant_randomly"
                });
                break;
                
            default:
                await ShowError($"Unknown function type: {functionType}");
                return;
        }
        
        // Update UI
        UpdateUI();
        
        _hasUnsavedChanges = true;
    }
    
    private async Task EditPoolRolls(int poolIndex)
    {
        if (_currentLootTable?.Pools == null || poolIndex >= _currentLootTable.Pools.Count)
        {
            await ShowError("Invalid pool index.");
            return;
        }
        
        var pool = _currentLootTable.Pools[poolIndex];
        
        // Create a dialog to edit rolls
        var dialog = new Window
        {
            Title = "Edit Pool Rolls",
            Width = 400,
            Height = 250,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };
        
        var panel = new StackPanel
        {
            Margin = new Thickness(20)
        };
        
        panel.Children.Add(new TextBlock
        {
            Text = "Set how many times items are drawn from this pool:",
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 15)
        });
        
        // Fixed or range selection
        var fixedRadio = new RadioButton
        {
            Content = "Fixed number of rolls",
            IsChecked = !(pool.Rolls is JsonElement),
            GroupName = "RollsType",
            Margin = new Thickness(0, 0, 0, 5)
        };
        panel.Children.Add(fixedRadio);
        
        var fixedValue = new NumericUpDown
        {
            Value = pool.Rolls is int intRolls ? intRolls : 1,
            Minimum = 1,
            Maximum = 100,
            FormatString = "0",
            Margin = new Thickness(20, 0, 0, 15)
        };
        panel.Children.Add(fixedValue);
        
        var rangeRadio = new RadioButton
        {
            Content = "Random range of rolls",
            IsChecked = pool.Rolls is JsonElement,
            GroupName = "RollsType",
            Margin = new Thickness(0, 0, 0, 5)
        };
        panel.Children.Add(rangeRadio);
        
        // Range panel with min and max controls
        var rangePanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Margin = new Thickness(20, 0, 0, 15)
        };
        
        var minValue = new NumericUpDown
        {
            Value = 1,
            Minimum = 0,
            Maximum = 100,
            FormatString = "0",
            Width = 100
        };
        
        var toLabel = new TextBlock
        {
            Text = "to",
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Margin = new Thickness(10, 0)
        };
        
        var maxValue = new NumericUpDown
        {
            Value = 3,
            Minimum = 1,
            Maximum = 100,
            FormatString = "0",
            Width = 100
        };
        
        // If rolls is a range object, use its values
        if (pool.Rolls is JsonElement element)
        {
            if (element.TryGetProperty("min", out var minElement) &&
                element.TryGetProperty("max", out var maxElement))
            {
                minValue.Value = minElement.GetInt32();
                maxValue.Value = maxElement.GetInt32();
            }
        }
        
        rangePanel.Children.Add(minValue);
        rangePanel.Children.Add(toLabel);
        rangePanel.Children.Add(maxValue);
        panel.Children.Add(rangePanel);
        
        // Enable/disable controls based on selection
        fixedRadio.IsCheckedChanged += (s, e) =>
        {
            if (fixedRadio.IsChecked == true)
            {
                fixedValue.IsEnabled = true;
                minValue.IsEnabled = maxValue.IsEnabled = false;
            }
        };
        
        rangeRadio.IsCheckedChanged += (s, e) =>
        {
            if (rangeRadio.IsChecked == true)
            {
                fixedValue.IsEnabled = false;
                minValue.IsEnabled = maxValue.IsEnabled = true;
            }
        };
        
        // Set initial enabled state
        if (fixedRadio.IsChecked == true)
        {
            minValue.IsEnabled = maxValue.IsEnabled = false;
        }
        else
        {
            fixedValue.IsEnabled = false;
        }
        
        // Buttons
        var buttonPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Spacing = 10,
            Margin = new Thickness(0, 10, 0, 0)
        };
        
        var cancelButton = new Button
        {
            Content = "Cancel",
            Width = 100
        };
        
        var saveButton = new Button
        {
            Content = "Save",
            Width = 100,
            Background = new SolidColorBrush(Color.Parse("#27AE60")),
            Foreground = new SolidColorBrush(Colors.White)
        };
        
        buttonPanel.Children.Add(cancelButton);
        buttonPanel.Children.Add(saveButton);
        panel.Children.Add(buttonPanel);
        
        dialog.Content = panel;
        
        var tcs = new TaskCompletionSource<object>();
        
        cancelButton.Click += (s, e) =>
        {
            tcs.SetResult(null);
            dialog.Close();
        };
        
        saveButton.Click += (s, e) =>
        {
            if (fixedRadio.IsChecked == true)
            {
                tcs.SetResult((int)fixedValue.Value);
            }
            else
            {
                tcs.SetResult(new
                {
                    min = (int)minValue.Value,
                    max = (int)maxValue.Value,
                    type = "minecraft:uniform"
                });
            }
            dialog.Close();
        };
        
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is Window parentWindow)
        {
            await dialog.ShowDialog(parentWindow);
        }
        else
        {
            dialog.Show();
        }
        
        var result = await tcs.Task;
        if (result != null)
        {
            // Update rolls and UI
            pool.Rolls = result;
            UpdateUI();
            
            _hasUnsavedChanges = true;
        }
    }
}
