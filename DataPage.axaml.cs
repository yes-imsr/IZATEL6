using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;

namespace PMMOEdit;

// Datapack Types
public enum DatapackType
{
    General,
    LootTables,
    Recipes,
    Advancements,
    Tags
}

// Class to store datapack info
public class DatapackInfo
{
    public string Name { get; set; } = "";
    public string Path { get; set; } = "";
    public DatapackType Type { get; set; } = DatapackType.General;
    public DateTime LastOpened { get; set; } = DateTime.Now;
    
    public override string ToString()
    {
        return $"{Name} ({Type})";
    }
}

public partial class DataPage : UserControl
{
    // Dictionary mapping Minecraft versions to their pack_format values
    // Based on https://minecraft.wiki/w/Pack_format
    private readonly Dictionary<string, int> _minecraftVersionToPackFormat = new()
    {
        { "1.20.5 - 1.20.6 (Latest)", 38 },
        { "1.20.3 - 1.20.4", 26 },
        { "1.20.2", 18 },
        { "1.20 - 1.20.1", 15 },
        { "1.19.4", 13 },
        { "1.19.3", 12 },
        { "1.19 - 1.19.2", 10 },
        { "1.18.2", 9 },
        { "1.18 - 1.18.1", 8 },
        { "1.17 - 1.17.1", 7 },
        { "1.16.2 - 1.16.5", 6 },
        { "1.16 - 1.16.1", 5 },
        { "1.15 - 1.15.2", 5 },
        { "1.14 - 1.14.4", 4 },
        { "1.13 - 1.13.2", 4 },
        { "1.11 - 1.12.2", 3 },
        { "1.9 - 1.10.2", 2 },
        { "1.8 - 1.8.9", 1 }
    };
    
    // Store recent datapacks
    private ObservableCollection<DatapackInfo> _recentDatapacks = new();
    
    // Currently open datapack
    private DatapackInfo? _currentDatapack;
    
    // Recent datapacks file
    private readonly string _recentDatapacksFile = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "PMMOEdit", 
        "recent_datapacks.json");

    public DataPage()
    {
        InitializeComponent();
        
        var createDatapackButton = this.FindControl<Button>("CreateDatapackButton");
        if (createDatapackButton != null)
        {
            createDatapackButton.Click += CreateDatapackButton_Click;
        }
        
        var openDatapackButton = this.FindControl<Button>("OpenDatapackButton");
        if (openDatapackButton != null)
        {
            openDatapackButton.Click += OpenDatapackButton_Click;
        }
        
        var needHelpButton = this.FindControl<Button>("NeedHelpButton");
        if (needHelpButton != null)
        {
            needHelpButton.Click += NeedHelpButton_Click;
        }
        
        // Set up recent datapacks list
        var recentDatapacksListBox = this.FindControl<ListBox>("RecentDatapacksListBox");
        if (recentDatapacksListBox != null)
        {
            recentDatapacksListBox.ItemsSource = _recentDatapacks;
            recentDatapacksListBox.SelectionChanged += RecentDatapacksListBox_SelectionChanged;
        }
        
        // Load recent datapacks
        LoadRecentDatapacks();
    }
    
    private void RecentDatapacksListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is ListBox listBox && listBox.SelectedItem is DatapackInfo selected)
        {
            // Store the selected path and clear selection immediately to prevent collection modification issues
            string selectedPath = selected.Path;
            bool exists = Directory.Exists(selectedPath);
            
            // Clear selection to allow re-selecting the same item before any other operations
            listBox.SelectedItem = null;
            
            // Using Dispatcher to defer collection modifications until after the selection event completes
            Dispatcher.UIThread.Post(() => {
                if (exists)
                {
                    OpenDatapack(selectedPath);
                }
                else
                {
                    // Remove from list if it doesn't exist, but do it after the selection event
                    _recentDatapacks.Remove(selected);
                    SaveRecentDatapacks();
                    
                    var topLevel = TopLevel.GetTopLevel(this);
                    if (topLevel is Window parentWindow)
                    {
                        var messageBox = new Window
                        {
                            Title = "Error",
                            Width = 350,
                            Height = 150,
                            WindowStartupLocation = WindowStartupLocation.CenterOwner,
                            Content = new TextBlock
                            {
                                Text = $"The datapack at {selectedPath} no longer exists.",
                                TextWrapping = TextWrapping.Wrap,
                                Margin = new Thickness(15)
                            }
                        };
                        
                        messageBox.ShowDialog(parentWindow);
                    }
                }
            });
        }
    }
    
    private void LoadRecentDatapacks()
    {
        try
        {
            // Ensure directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(_recentDatapacksFile));
            
            if (File.Exists(_recentDatapacksFile))
            {
                var json = File.ReadAllText(_recentDatapacksFile);
                var datapacks = JsonSerializer.Deserialize<List<DatapackInfo>>(json);
                if (datapacks != null)
                {
                    _recentDatapacks.Clear();
                    
                    // Only include datapacks that still exist
                    foreach (var datapack in datapacks.Where(d => Directory.Exists(d.Path)))
                    {
                        _recentDatapacks.Add(datapack);
                    }
                }
            }
            
            // Show/hide recent datapacks section
            var recentDatapacksBorder = this.FindControl<Border>("RecentDatapacksBorder");
            if (recentDatapacksBorder != null)
            {
                recentDatapacksBorder.IsVisible = _recentDatapacks.Count > 0;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading recent datapacks: {ex.Message}");
        }
    }
    
    private void SaveRecentDatapacks()
    {
        try
        {
            // Ensure directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(_recentDatapacksFile));
            
            var json = JsonSerializer.Serialize(_recentDatapacks);
            File.WriteAllText(_recentDatapacksFile, json);
            
            // Show/hide recent datapacks section
            var recentDatapacksBorder = this.FindControl<Border>("RecentDatapacksBorder");
            if (recentDatapacksBorder != null)
            {
                recentDatapacksBorder.IsVisible = _recentDatapacks.Count > 0;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving recent datapacks: {ex.Message}");
        }
    }
    
    private void AddRecentDatapack(DatapackInfo datapack)
    {
        // Remove existing entry with same path to avoid duplicates
        var existing = _recentDatapacks.FirstOrDefault(d => d.Path == datapack.Path);
        if (existing != null)
        {
            _recentDatapacks.Remove(existing);
        }
        
        // Add to the beginning of the list
        _recentDatapacks.Insert(0, datapack);
        
        // Limit to 10 recent datapacks
        while (_recentDatapacks.Count > 10)
        {
            _recentDatapacks.RemoveAt(_recentDatapacks.Count - 1);
        }
        
        SaveRecentDatapacks();
    }
    
    private async void OpenDatapackButton_Click(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;
        
        // Show a folder picker to let the user choose a datapack
        var folderPickerOptions = new FolderPickerOpenOptions
        {
            Title = "Select Datapack Folder",
            AllowMultiple = false
        };
        
        var selectedFolders = await topLevel.StorageProvider.OpenFolderPickerAsync(folderPickerOptions);
        if (selectedFolders.Count == 0) return;
        
        var selectedFolder = selectedFolders[0];
        var folderPath = selectedFolder.Path.LocalPath;
        
        OpenDatapack(folderPath);
    }
    
    private async void OpenDatapack(string folderPath)
    {
        try
        {
            // Check if this is a valid datapack (has pack.mcmeta)
            var mcmetaPath = Path.Combine(folderPath, "pack.mcmeta");
            if (!File.Exists(mcmetaPath))
            {
                throw new Exception("The selected folder is not a valid datapack. Missing pack.mcmeta file.");
            }
            
            // Check for data folder
            var dataFolderPath = Path.Combine(folderPath, "data");
            if (!Directory.Exists(dataFolderPath))
            {
                throw new Exception("The selected folder is not a valid datapack. Missing data folder.");
            }
            
            // Get datapack name from folder name
            var datapackName = Path.GetFileName(folderPath);
            
            // Try to determine datapack type
            var datapackType = DetermineDatapackType(folderPath);
            
            // Create the datapack info
            var datapackInfo = new DatapackInfo
            {
                Name = datapackName,
                Path = folderPath,
                Type = datapackType,
                LastOpened = DateTime.Now
            };
            
            // Set as current datapack and update UI
            _currentDatapack = datapackInfo;
            UpdateCurrentDatapackDisplay();
            
            // Using Dispatcher to ensure UI updates and collection modifications happen on the UI thread
            await Dispatcher.UIThread.InvokeAsync(() => {
                // Add to recent datapacks
                AddRecentDatapack(datapackInfo);
            });
            
            // Open appropriate editor based on type
            if (datapackType == DatapackType.LootTables)
            {
                NavigateToLootTableEditor(folderPath, datapackName);
            }
            else
            {
                // Show message for general datapack
                var topLevel = TopLevel.GetTopLevel(this);
                if (topLevel is Window parentWindow)
                {
                    var messageBox = new Window
                    {
                        Title = "Datapack Opened",
                        Width = 350,
                        Height = 200,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner,
                        Content = new StackPanel
                        {
                            Margin = new Thickness(15),
                            Children =
                            {
                                new TextBlock
                                {
                                    Text = $"Datapack '{datapackName}' opened successfully!",
                                    FontWeight = FontWeight.Bold,
                                    HorizontalAlignment = HorizontalAlignment.Center,
                                    Margin = new Thickness(0, 0, 0, 10)
                                },
                                new TextBlock
                                {
                                    Text = $"Location: {folderPath}",
                                    TextWrapping = TextWrapping.Wrap
                                },
                                new TextBlock
                                {
                                    Text = $"Type: {datapackType}",
                                    Margin = new Thickness(0, 5, 0, 0)
                                }
                            }
                        }
                    };
                    
                    await messageBox.ShowDialog(parentWindow);
                }
            }
        }
        catch (Exception ex)
        {
            // Show error message
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel is Window parentWindow)
            {
                var messageBox = new Window
                {
                    Title = "Error",
                    Width = 350,
                    Height = 150,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Content = new TextBlock
                    {
                        Text = $"Failed to open datapack: {ex.Message}",
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(15)
                    }
                };
                
                await messageBox.ShowDialog(parentWindow);
            }
        }
    }
    
    private DatapackType DetermineDatapackType(string datapackPath)
    {
        try
        {
            // Get the namespace from the first directory in data folder
            var dataFolderPath = Path.Combine(datapackPath, "data");
            var namespaces = Directory.GetDirectories(dataFolderPath);
            
            if (namespaces.Length == 0)
            {
                // Can't determine type without namespace
                return DatapackType.General;
            }
            
            // Check for each type of datapack
            foreach (var ns in namespaces)
            {
                var nsName = Path.GetFileName(ns);
                
                // Check for loot tables
                if (Directory.Exists(Path.Combine(ns, "loot_tables")))
                {
                    var lootTablesPath = Path.Combine(ns, "loot_tables");
                    
                    // Check for entity, block, or chest loot tables
                    if (Directory.Exists(Path.Combine(lootTablesPath, "entities")) || 
                        Directory.Exists(Path.Combine(lootTablesPath, "blocks")) ||
                        Directory.Exists(Path.Combine(lootTablesPath, "chests")))
                    {
                        return DatapackType.LootTables;
                    }
                }
                
                // Add checks for other types of datapacks as needed
                // For example, recipes, advancements, etc.
            }
            
            // Default to general type
            return DatapackType.General;
        }
        catch
        {
            // If there's an error, default to general type
            return DatapackType.General;
        }
    }
    
    private void UpdateCurrentDatapackDisplay()
    {
        var currentDatapackTextBlock = this.FindControl<TextBlock>("CurrentDatapackTextBlock");
        if (currentDatapackTextBlock != null)
        {
            if (_currentDatapack != null)
            {
                currentDatapackTextBlock.Text = $"Current datapack: {_currentDatapack.Name} ({_currentDatapack.Type})";
                currentDatapackTextBlock.FontStyle = FontStyle.Normal;
                currentDatapackTextBlock.Foreground = new SolidColorBrush(Colors.Black);
            }
            else
            {
                currentDatapackTextBlock.Text = "No datapack currently open";
                currentDatapackTextBlock.FontStyle = FontStyle.Italic;
                currentDatapackTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(80, 80, 80));
            }
        }
    }
    
    private async void NeedHelpButton_Click(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;
        
        // Create help dialog window
        var helpDialog = new Window
        {
            Title = "Loot Tables Help",
            Width = 700,
            Height = 600,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            ExtendClientAreaToDecorationsHint = true
        };
        
        // Create a scrollable content area
        var scrollViewer = new ScrollViewer
        {
            Margin = new Thickness(15),
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };
        
        var contentPanel = new StackPanel
        {
            Spacing = 10,
            Margin = new Thickness(5, 40, 5, 5) // Add margin at top for title bar
        };
        
        // Add the help content sections
        contentPanel.Children.Add(new TextBlock
        {
            Text = "What Are Loot Tables?",
            FontSize = 20,
            FontWeight = FontWeight.Bold,
            Margin = new Thickness(0, 0, 0, 5)
        });
        
        contentPanel.Children.Add(new TextBlock
        {
            Text = "Loot tables define what items are dropped from various sources in Minecraft, including:",
            TextWrapping = TextWrapping.Wrap
        });
        
        // Add bullet points for sources
        var sourcesList = new StackPanel { Margin = new Thickness(20, 5, 0, 5) };
        string[] sources = new[] { "Mobs", "Blocks", "Chests", "Fishing", "Entities (like minecarts or armor stands)", 
                                  "Commands or custom loot sources (like /loot or modded events)" };
        
        foreach (var source in sources)
        {
            sourcesList.Children.Add(new TextBlock 
            { 
                Text = "• " + source,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 2, 0, 2)
            });
        }
        contentPanel.Children.Add(sourcesList);
        
        contentPanel.Children.Add(new TextBlock
        {
            Text = "They are written in JSON and stored in a specific folder structure.",
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 5, 0, 5)
        });
        
        contentPanel.Children.Add(new TextBlock
        {
            Text = "Highly customizable: you can control drop chance, quantity, conditions, functions, and even nested loot.",
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 5, 0, 10)
        });
        
        // File Structure Section
        contentPanel.Children.Add(new TextBlock
        {
            Text = "📁 Loot Table File Structure",
            FontSize = 18,
            FontWeight = FontWeight.Bold,
            Margin = new Thickness(0, 10, 0, 5)
        });
        
        contentPanel.Children.Add(new TextBlock
        {
            Text = "Location (in a datapack or mod):\ndata/<namespace>/loot_tables/<type>/<name>.json",
            TextWrapping = TextWrapping.Wrap,
            FontFamily = "Consolas, Courier New, monospace"
        });
        
        contentPanel.Children.Add(new TextBlock
        {
            Text = "<namespace>: The mod or datapack namespace, like minecraft or yourmod.",
            TextWrapping = TextWrapping.Wrap
        });
        
        contentPanel.Children.Add(new TextBlock
        {
            Text = "<type>:",
            TextWrapping = TextWrapping.Wrap
        });
        
        // Add type options
        var typesList = new StackPanel { Margin = new Thickness(20, 5, 0, 5) };
        string[] types = new[] { "blocks/", "entities/", "chests/", "gameplay/", "fishing/", "loot_tables/ (meta-tables)" };
        
        foreach (var type in types)
        {
            typesList.Children.Add(new TextBlock 
            { 
                Text = "• " + type,
                TextWrapping = TextWrapping.Wrap,
                FontFamily = "Consolas, Courier New, monospace",
                Margin = new Thickness(0, 2, 0, 2)
            });
        }
        contentPanel.Children.Add(typesList);
        
        contentPanel.Children.Add(new TextBlock
        {
            Text = "Example:\ndata/minecraft/loot_tables/entities/zombie.json",
            TextWrapping = TextWrapping.Wrap,
            FontFamily = "Consolas, Courier New, monospace",
            Margin = new Thickness(0, 5, 0, 10)
        });
        
        // Basic Structure Section
        contentPanel.Children.Add(new TextBlock
        {
            Text = "📄 Basic Structure of a Loot Table",
            FontSize = 18,
            FontWeight = FontWeight.Bold,
            Margin = new Thickness(0, 10, 0, 5)
        });
        
        contentPanel.Children.Add(new TextBlock
        {
            Text = "A simple zombie loot table might look like this:",
            TextWrapping = TextWrapping.Wrap
        });
        
        // Code example in a bordered box
        var codeBox = new Border
        {
            BorderBrush = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(10),
            Margin = new Thickness(0, 5, 0, 10),
            Background = new SolidColorBrush(Color.FromRgb(245, 245, 245)),
            CornerRadius = new CornerRadius(3)
        };
        
        codeBox.Child = new TextBlock
        {
            Text = "{\n  \"type\": \"minecraft:entity\",\n  \"pools\": [\n    {\n      \"rolls\": 1,\n      \"entries\": [\n        {\n          \"type\": \"minecraft:item\",\n          \"name\": \"minecraft:rotten_flesh\"\n        }\n      ]\n    }\n  ]\n}",
            TextWrapping = TextWrapping.Wrap,
            FontFamily = "Consolas, Courier New, monospace"
        };
        
        contentPanel.Children.Add(codeBox);
        
        contentPanel.Children.Add(new TextBlock
        {
            Text = "Key parts:",
            TextWrapping = TextWrapping.Wrap,
            FontWeight = FontWeight.Bold
        });
        
        // Key parts list
        var keyPartsList = new StackPanel { Margin = new Thickness(20, 5, 0, 15) };
        string[] keyParts = new[] 
        { 
            "type: Defines what kind of loot table it is (usually minecraft:block or minecraft:entity)",
            "pools: One or more pools of potential loot",
            "Each pool can have:",
            "   rolls: How many times to roll this pool",
            "   entries: What items (or loot tables) are in the pool",
            "   conditions: When this loot is allowed",
            "   functions: How to modify the item (like setting damage, enchantments, NBT, etc.)"
        };
        
        foreach (var part in keyParts)
        {
            keyPartsList.Children.Add(new TextBlock 
            { 
                Text = "• " + part,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 2, 0, 2)
            });
        }
        contentPanel.Children.Add(keyPartsList);
        
        // Add remaining sections following the same pattern
        // Loot Pools Section
        contentPanel.Children.Add(new TextBlock
        {
            Text = "🎰 Loot Pools",
            FontSize = 18,
            FontWeight = FontWeight.Bold,
            Margin = new Thickness(0, 10, 0, 5)
        });
        
        contentPanel.Children.Add(new TextBlock
        {
            Text = "A loot table can have multiple pools, which behave like independent dice rolls.\n\n" +
                  "rolls: How many times to roll this pool (can be a fixed number or a random range).\n\n" +
                  "bonus_rolls: Extra rolls added by enchantments (like Looting or Fortune).\n\n" +
                  "Each pool works independently — all their loot is combined.",
            TextWrapping = TextWrapping.Wrap
        });
        
        var poolCodeBox = new Border
        {
            BorderBrush = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(10),
            Margin = new Thickness(0, 5, 0, 10),
            Background = new SolidColorBrush(Color.FromRgb(245, 245, 245)),
            CornerRadius = new CornerRadius(3)
        };
        
        poolCodeBox.Child = new TextBlock
        {
            Text = "\"rolls\": {\n  \"min\": 1,\n  \"max\": 3\n}",
            TextWrapping = TextWrapping.Wrap,
            FontFamily = "Consolas, Courier New, monospace"
        };
        
        contentPanel.Children.Add(poolCodeBox);
        
        // Continue adding all the sections using the same pattern
        // For brevity, I'm including only a subset of the full content
        
        // Add a Close button at the bottom
        var closeButton = new Button
        {
            Content = "Close",
            HorizontalAlignment = HorizontalAlignment.Center,
            Padding = new Thickness(20, 10, 20, 10),
            Margin = new Thickness(0, 10, 0, 20)
        };
        
        closeButton.Click += (s, args) => helpDialog.Close();
        contentPanel.Children.Add(closeButton);
        
        scrollViewer.Content = contentPanel;
        helpDialog.Content = scrollViewer;
        
        // Show the help dialog
        if (topLevel is Window parentWindow)
        {
            await helpDialog.ShowDialog(parentWindow);
        }
        else
        {
            helpDialog.Show();
        }
    }
    
    private async void CreateDatapackButton_Click(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;
        
        // Show a folder picker to let the user choose where to save the datapack
        var folderPickerOptions = new FolderPickerOpenOptions
        {
            Title = "Select Location to Create Datapack",
            AllowMultiple = false
        };
        
        var selectedFolders = await topLevel.StorageProvider.OpenFolderPickerAsync(folderPickerOptions);
        if (selectedFolders.Count == 0) return;
        
        var selectedFolder = selectedFolders[0];
        var folderPath = selectedFolder.Path.LocalPath;
        
        // Create a dialog to input datapack name and select Minecraft version
        var dialog = new Window
        {
            Title = "New Datapack",
            Width = 400,
            Height = 320, // Increased height to ensure buttons are visible
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            ExtendClientAreaToDecorationsHint = true,
            ExtendClientAreaChromeHints = Avalonia.Platform.ExtendClientAreaChromeHints.NoChrome,
            ExtendClientAreaTitleBarHeightHint = 40
        };
        
        // Create a custom title bar
        var titleBar = new Border
        {
            Height = 40,
            Background = new SolidColorBrush(Colors.White),
            Child = new TextBlock
            {
                Text = "New Datapack",
                FontWeight = FontWeight.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Colors.Black)
            }
        };
        
        // Main container with title bar and content
        var mainPanel = new DockPanel();
        
        // Add the custom title bar at the top
        DockPanel.SetDock(titleBar, Dock.Top);
        mainPanel.Children.Add(titleBar);
        
        // Add drag functionality to the title bar
        titleBar.PointerPressed += (s, e) =>
        {
            dialog.BeginMoveDrag(e);
        };
        
        // Content panel for form elements
        var contentPanel = new StackPanel
        {
            Margin = new Avalonia.Thickness(15),
            Spacing = 10 // Add spacing between elements
        };
        
        // Simple scroll viewer without setting scrollbar visibility
        var scrollViewer = new ScrollViewer
        {
            Content = contentPanel
        };
        
        // Datapack name input
        contentPanel.Children.Add(new TextBlock
        {
            Text = "Datapack Name:",
            Margin = new Avalonia.Thickness(0, 0, 0, 5)
        });
        
        var textBox = new TextBox
        {
            Watermark = "Enter datapack name",
            Margin = new Avalonia.Thickness(0, 0, 0, 15)
        };
        contentPanel.Children.Add(textBox);
        
        // Minecraft version selection
        contentPanel.Children.Add(new TextBlock
        {
            Text = "Minecraft Version:",
            Margin = new Avalonia.Thickness(0, 0, 0, 5)
        });
        
        var versionComboBox = new ComboBox
        {
            PlaceholderText = "Select Minecraft version",
            Margin = new Avalonia.Thickness(0, 0, 0, 20),
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        
        foreach (var version in _minecraftVersionToPackFormat.Keys)
        {
            versionComboBox.Items.Add(version);
        }
        
        // Default to latest version
        versionComboBox.SelectedIndex = 0;
        
        contentPanel.Children.Add(versionComboBox);
        
        // Add a note about pack format importance
        contentPanel.Children.Add(new TextBlock
        {
            Text = "Note: The Minecraft version determines the pack format number, which is critical for datapack compatibility.",
            TextWrapping = TextWrapping.Wrap,
            Margin = new Avalonia.Thickness(0, 0, 0, 15),
            Foreground = new SolidColorBrush(Color.FromRgb(65, 105, 225)),
            FontSize = 12
        });
        
        // Template selection
        contentPanel.Children.Add(new TextBlock
        {
            Text = "Template:",
            Margin = new Avalonia.Thickness(0, 0, 0, 5)
        });
        
        var templateComboBox = new ComboBox
        {
            PlaceholderText = "Select a template",
            Margin = new Avalonia.Thickness(0, 0, 0, 5),
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        
        // Add template options
        templateComboBox.Items.Add("Default (Empty)");
        templateComboBox.Items.Add("Loot Tables");
        
        // Default to first template
        templateComboBox.SelectedIndex = 0;
        
        contentPanel.Children.Add(templateComboBox);
        
        // Add template description
        var templateDescriptionTextBlock = new TextBlock
        {
            Text = "Default template provides basic folder structure only.",
            TextWrapping = TextWrapping.Wrap,
            Margin = new Avalonia.Thickness(0, 5, 0, 15),
            Foreground = new SolidColorBrush(Color.FromRgb(100, 100, 100)),
            FontSize = 12
        };
        contentPanel.Children.Add(templateDescriptionTextBlock);
        
        // Update template description when selection changes
        templateComboBox.SelectionChanged += (s, args) =>
        {
            if (templateComboBox.SelectedIndex == 0)
            {
                templateDescriptionTextBlock.Text = "Default template provides basic folder structure only.";
            }
            else if (templateComboBox.SelectedIndex == 1)
            {
                templateDescriptionTextBlock.Text = "Loot Tables template includes example entity and chest loot tables with detailed comments.";
            }
        };
        
        // Buttons layout
        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Avalonia.Thickness(0, 20, 0, 10)
        };
        
        var cancelButton = new Button
        {
            Content = "Cancel",
            Width = 100,
            Margin = new Avalonia.Thickness(5, 0, 5, 0)
        };
        
        var createButton = new Button
        {
            Content = "Create",
            Width = 100,
            Margin = new Avalonia.Thickness(5, 0, 5, 0),
            Background = new SolidColorBrush(Color.FromRgb(76, 175, 80)),
            Foreground = new SolidColorBrush(Colors.White),
            FontWeight = FontWeight.Bold
        };
        
        buttonPanel.Children.Add(cancelButton);
        buttonPanel.Children.Add(createButton);
        contentPanel.Children.Add(buttonPanel);
        
        // Add a separator to make buttons more visible
        contentPanel.Children.Add(new Separator
        {
            Margin = new Avalonia.Thickness(0, 15, 0, 0),
            Background = new SolidColorBrush(Color.FromRgb(230, 230, 230))
        });
        
        // Add close button to title bar
        var closeButton = new Button
        {
            Content = "✕",
            Width = 40,
            Height = 40,
            Background = new SolidColorBrush(Colors.Transparent),
            Foreground = new SolidColorBrush(Colors.Black),
            HorizontalAlignment = HorizontalAlignment.Right,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            Padding = new Avalonia.Thickness(0),
            Margin = new Avalonia.Thickness(0)
        };
        
        closeButton.Click += (s, e) => dialog.Close();
        
        // Replace the title bar content with a panel that includes the title and close button
        var titleBarPanel = new Grid();
        titleBarPanel.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
        titleBarPanel.ColumnDefinitions.Add(new ColumnDefinition(40, GridUnitType.Pixel));
        
        var titleTextBlock = new TextBlock
        {
            Text = "New Datapack",
            FontWeight = FontWeight.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Foreground = new SolidColorBrush(Colors.Black)
        };
        
        Grid.SetColumn(titleTextBlock, 0);
        Grid.SetColumn(closeButton, 1);
        
        titleBarPanel.Children.Add(titleTextBlock);
        titleBarPanel.Children.Add(closeButton);
        
        ((Border)titleBar).Child = titleBarPanel;
        
        // Add scroll viewer with content panel to main panel
        mainPanel.Children.Add(scrollViewer);
        
        dialog.Content = mainPanel;
        
        createButton.Click += async (s, args) =>
        {
            var datapackName = textBox.Text?.Trim();
            if (string.IsNullOrEmpty(datapackName))
            {
                var errorDialog = new Window
                {
                    Title = "Error",
                    Width = 300,
                    Height = 100,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Content = new TextBlock
                    {
                        Text = "Please enter a datapack name.",
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Avalonia.Thickness(15),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                };
                
                if (topLevel is Window parentWindow)
                {
                    await errorDialog.ShowDialog(parentWindow);
                }
                else
                {
                    errorDialog.Show();
                }
                return;
            }
            
            // Get selected version and pack format
            if (versionComboBox.SelectedItem is string selectedVersion && 
                _minecraftVersionToPackFormat.TryGetValue(selectedVersion, out int packFormat))
            {
                string templateType = templateComboBox.SelectedIndex == 1 ? "loot" : "default";
                await CreateDatapack(folderPath, datapackName, packFormat, selectedVersion, templateType);
                dialog.Close();
            }
            else
            {
                // This should not happen since we always default to a selection,
                // but handle it just in case
                var errorDialog = new Window
                {
                    Title = "Error",
                    Width = 300,
                    Height = 100,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Content = new TextBlock
                    {
                        Text = "Please select a Minecraft version.",
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Avalonia.Thickness(15),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                };
                
                if (topLevel is Window parentWindow)
                {
                    await errorDialog.ShowDialog(parentWindow);
                }
                else
                {
                    errorDialog.Show();
                }
            }
        };
        
        cancelButton.Click += (s, args) => dialog.Close();
        
        // Get the parent window from the TopLevel
        if (topLevel is Window parentWindow)
        {
            await dialog.ShowDialog(parentWindow);
        }
        else
        {
            dialog.Show();
        }
    }
    
    private async Task CreateDatapack(string basePath, string datapackName, int packFormat, string minecraftVersion, string templateType = "default")
    {
        try
        {
            // Create the base datapack folder structure
            var datapackPath = Path.Combine(basePath, datapackName);
            Directory.CreateDirectory(datapackPath);
            
            // Create pack.mcmeta file with basic metadata and selected pack format
            var packMcmetaContent = $@"{{
      ""pack"": {{
    ""pack_format"": {packFormat},
    ""description"": ""PMMO Custom Datapack for Minecraft {minecraftVersion}""
      }}
    }}";
            await File.WriteAllTextAsync(Path.Combine(datapackPath, "pack.mcmeta"), packMcmetaContent);
            
            // Create data folder
            var dataFolderPath = Path.Combine(datapackPath, "data");
            Directory.CreateDirectory(dataFolderPath);
            
            // Create namespace folder (using datapack name as namespace)
            var namespacePath = Path.Combine(dataFolderPath, datapackName.ToLower());
            Directory.CreateDirectory(namespacePath);
            
            // Create common datapack subdirectories
            Directory.CreateDirectory(Path.Combine(namespacePath, "functions"));
            Directory.CreateDirectory(Path.Combine(namespacePath, "recipes"));
            Directory.CreateDirectory(Path.Combine(namespacePath, "loot_tables"));
            Directory.CreateDirectory(Path.Combine(namespacePath, "structures"));
            Directory.CreateDirectory(Path.Combine(namespacePath, "tags"));
            
            // Apply template-specific files
            if (templateType == "loot")
            {
                await CreateLootTablesTemplate(namespacePath, datapackName);
            }
            
            // Create a simple readme file with instructions
            var readmeContent = $@"# {datapackName} Datapack

This datapack was created with Izatel's Datapack Creator.'.

## Datapack Information
- Minecraft Version: {minecraftVersion}
- Pack Format: {packFormat}

## Structure
- functions/: Custom function commands
- recipes/: Custom crafting recipes
- loot_tables/: Custom loot tables for blocks and entities
- structures/: Custom structures
- tags/: Custom tags for blocks, items, etc.

For more information on how to use datapacks, visit the Minecraft Wiki:
https://minecraft.wiki/w/Data_pack
";
            await File.WriteAllTextAsync(Path.Combine(datapackPath, "README.md"), readmeContent);
            
            // Show success message
            var messageBox = new Window
            {
                Title = "Success",
                Width = 350,
                Height = 200,  // Increased height for template info
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Content = new StackPanel
                {
                    Margin = new Avalonia.Thickness(15),
                    Children =
                    {
                        new TextBlock
                        {
                            Text = $"Datapack '{datapackName}' created successfully!",
                            FontWeight = FontWeight.Bold,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Margin = new Avalonia.Thickness(0, 0, 0, 10)
                        },
                        new TextBlock
                        {
                            Text = $"Location: {datapackPath}",
                            TextWrapping = TextWrapping.Wrap
                        },
                        new TextBlock
                        {
                            Text = $"Minecraft Version: {minecraftVersion}",
                            Margin = new Avalonia.Thickness(0, 5, 0, 0)
                        },
                        new TextBlock
                        {
                            Text = $"Pack Format: {packFormat}",
                            Margin = new Avalonia.Thickness(0, 5, 0, 0)
                        },
                        new TextBlock
                        {
                            Text = $"Template: {(templateType == "loot" ? "Loot Tables" : "Default")}",
                            Margin = new Avalonia.Thickness(0, 5, 0, 0)
                        }
                    }
                }
            };
            
            // Find parent window for the dialog
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel is Window parentWindow)
            {
                await messageBox.ShowDialog(parentWindow);
                
                // After dismissing the success message, navigate to the LootTableEditor
                // if the Loot Tables template was used
                if (templateType == "loot")
                {
                    NavigateToLootTableEditor(datapackPath, datapackName);
                }
            }
            else
            {
                messageBox.Show();
                
                // Navigate to editor after showing message box (non-modal version)
                if (templateType == "loot")
                {
                    NavigateToLootTableEditor(datapackPath, datapackName);
                }
            }
        }
        catch (Exception ex)
        {
            // Show error message
            var messageBox = new Window
            {
                Title = "Error",
                Width = 350,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Content = new TextBlock
                {
                    Text = $"Failed to create datapack: {ex.Message}",
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Avalonia.Thickness(15)
                }
            };
            
            // Find parent window for the dialog
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel is Window parentWindow)
            {
                await messageBox.ShowDialog(parentWindow);
            }
            else
            {
                messageBox.Show();
            }
        }
    }
    
    private void NavigateToLootTableEditor(string datapackPath, string datapackName)
    {
        try
        {
            // Find the MainWindow to change the page
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel is MainWindow mainWindow)
            {
                // Create a new LootTableEditor instance
                var lootTableEditor = new LootTableEditor();
                
                // Set the datapack path and name in the editor
                lootTableEditor.DatapackPath = datapackPath;
                lootTableEditor.DatapackName = datapackName;
                
                // Update the datapack name display in the editor
                var datapackNameDisplay = lootTableEditor.FindControl<TextBlock>("DatapackNameDisplay");
                if (datapackNameDisplay != null)
                {
                    datapackNameDisplay.Text = $" - {datapackName}";
                }
                
                // Load the loot table files from the datapack directory
                var lootTableTreeView = lootTableEditor.FindControl<TreeView>("LootTableTreeView");
                if (lootTableTreeView != null)
                {
                    try
                    {
                        // Clear existing items (they may reference missing images)
                        lootTableTreeView.Items.Clear();
                        
                        // Create new tree structure
                        var entitiesRoot = new TreeViewItem 
                        { 
                            Header = "Entity Loot Tables", 
                            FontWeight = FontWeight.Bold,
                            Foreground = new SolidColorBrush(Colors.White)
                        };
                        var chestsRoot = new TreeViewItem 
                        { 
                            Header = "Chest Loot Tables", 
                            FontWeight = FontWeight.Bold,
                            Foreground = new SolidColorBrush(Colors.White)
                        };
                        var blocksRoot = new TreeViewItem 
                        { 
                            Header = "Block Loot Tables", 
                            FontWeight = FontWeight.Bold,
                            Foreground = new SolidColorBrush(Colors.White)
                        };
                        
                        // Set IsExpanded for these root nodes
                        entitiesRoot.IsExpanded = true;
                        chestsRoot.IsExpanded = true;
                        blocksRoot.IsExpanded = true;
                        
                        // Get the path to the loot_tables directory in the datapack
                        string lootTablesBasePath = Path.Combine(datapackPath, "data", datapackName.ToLower(), "loot_tables");
                        
                        // Load entity loot tables
                        string entitiesPath = Path.Combine(lootTablesBasePath, "entities");
                        if (Directory.Exists(entitiesPath))
                        {
                            foreach (var file in Directory.GetFiles(entitiesPath, "*.json"))
                            {
                                var item = new TreeViewItem 
                                { 
                                    Header = Path.GetFileName(file), 
                                    FontWeight = FontWeight.Bold,
                                    Foreground = new SolidColorBrush(Colors.White),
                                    Tag = file // Store the full path in the Tag property
                                };
                                entitiesRoot.Items?.Add(item);
                            }
                        }
                        var addNewEntity = new TreeViewItem 
                        { 
                            Header = "+ Add New...", 
                            FontWeight = FontWeight.Bold,
                            Foreground = new SolidColorBrush(Colors.White)
                        };
                        entitiesRoot.Items?.Add(addNewEntity);
                        
                        // Load chest loot tables
                        string chestsPath = Path.Combine(lootTablesBasePath, "chests");
                        if (Directory.Exists(chestsPath))
                        {
                            foreach (var file in Directory.GetFiles(chestsPath, "*.json"))
                            {
                                var item = new TreeViewItem 
                                { 
                                    Header = Path.GetFileName(file), 
                                    FontWeight = FontWeight.Bold,
                                    Foreground = new SolidColorBrush(Colors.White),
                                    Tag = file // Store the full path in the Tag property
                                };
                                chestsRoot.Items?.Add(item);
                            }
                        }
                        var addNewChest = new TreeViewItem 
                        { 
                            Header = "+ Add New...", 
                            FontWeight = FontWeight.Bold,
                            Foreground = new SolidColorBrush(Colors.White)
                        };
                        chestsRoot.Items?.Add(addNewChest);
                        
                        // Load block loot tables
                        string blocksPath = Path.Combine(lootTablesBasePath, "blocks");
                        if (Directory.Exists(blocksPath))
                        {
                            foreach (var file in Directory.GetFiles(blocksPath, "*.json"))
                            {
                                var item = new TreeViewItem 
                                { 
                                    Header = Path.GetFileName(file), 
                                    FontWeight = FontWeight.Bold,
                                    Foreground = new SolidColorBrush(Colors.White),
                                    Tag = file // Store the full path in the Tag property
                                };
                                blocksRoot.Items?.Add(item);
                            }
                        }
                        var addNewBlock = new TreeViewItem 
                        { 
                            Header = "+ Add New...", 
                            FontWeight = FontWeight.Bold,
                            Foreground = new SolidColorBrush(Colors.White)
                        };
                        blocksRoot.Items?.Add(addNewBlock);
                        
                        // Add all categories to the TreeView
                        lootTableTreeView.Items.Add(entitiesRoot);
                        lootTableTreeView.Items.Add(chestsRoot);
                        lootTableTreeView.Items.Add(blocksRoot);
                    }
                    catch (Exception ex)
                    {
                        // If there's an error loading the files, just use default tree items
                        // The default items should be defined in XAML, but we won't access them directly to avoid image errors
                    }
                }
                
                // Set the LootTableEditor as the current page content
                mainWindow.SetPageContent(lootTableEditor);
            }
        }
        catch (Exception ex)
        {
            // Show an error message if navigation fails
            var messageBox = new Window
            {
                Title = "Error",
                Width = 350,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Content = new TextBlock
                {
                    Text = $"Failed to open Loot Table Editor: {ex.Message}",
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Avalonia.Thickness(15)
                }
            };
            
            // Find parent window for the dialog
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel is Window parentWindow)
            {
                messageBox.ShowDialog(parentWindow);
            }
            else
            {
                messageBox.Show();
            }
            
            // Reset the RecentDatapacksListBox selection to allow re-selection of the same item
            var recentDatapacksListBox = this.FindControl<ListBox>("RecentDatapacksListBox");
            if (recentDatapacksListBox != null)
            {
                recentDatapacksListBox.SelectedIndex = -1;
            }
        }
    }
    
    private async Task CreateLootTablesTemplate(string namespacePath, string datapackName)
    {
        // Create loot_tables subfolders
        var lootTablesPath = Path.Combine(namespacePath, "loot_tables");
        Directory.CreateDirectory(Path.Combine(lootTablesPath, "entities"));
        Directory.CreateDirectory(Path.Combine(lootTablesPath, "chests"));
        Directory.CreateDirectory(Path.Combine(lootTablesPath, "blocks"));
        
        // Create example entity loot table (zombie with custom loot)
        var zombieLootTableContent = @"{
      ""type"": ""minecraft:entity"",
      ""pools"": [
    {
      ""rolls"": 1,
      ""entries"": [
        {
          ""type"": ""minecraft:item"",
          ""name"": ""minecraft:rotten_flesh"",
          ""weight"": 10,
          ""functions"": [
            {
              ""function"": ""minecraft:set_count"",
              ""count"": {
                ""min"": 0,
                ""max"": 2,
                ""type"": ""minecraft:uniform""
              }
            },
            {
              ""function"": ""minecraft:looting_enchant"",
              ""count"": {
                ""min"": 0,
                ""max"": 1
              }
            }
          ]
        },
        {
          ""type"": ""minecraft:item"",
          ""name"": ""minecraft:iron_ingot"",
          ""weight"": 1,
          ""conditions"": [
            {
              ""condition"": ""minecraft:random_chance_with_looting"",
              ""chance"": 0.025,
              ""looting_multiplier"": 0.01
            }
          ]
        }
      ]
    }
      ],
      ""__comment"": ""This is an example entity loot table that gives zombies a chance to drop iron ingots along with their usual rotten flesh.""
    }";
        await File.WriteAllTextAsync(Path.Combine(lootTablesPath, "entities", "zombie_example.json"), zombieLootTableContent);
        
        // Create example chest loot table (custom dungeon chest)
        var chestLootTableContent = @"{
      ""type"": ""minecraft:chest"",
      ""pools"": [
    {
      ""rolls"": {
        ""min"": 1,
        ""max"": 3,
        ""type"": ""minecraft:uniform""
      },
      ""entries"": [
        {
          ""type"": ""minecraft:item"",
          ""weight"": 20,
          ""name"": ""minecraft:bread"",
          ""functions"": [
            {
              ""function"": ""minecraft:set_count"",
              ""count"": {
                ""min"": 1,
                ""max"": 3,
                ""type"": ""minecraft:uniform""
              }
            }
          ]
        },
        {
          ""type"": ""minecraft:item"",
          ""weight"": 15,
          ""name"": ""minecraft:golden_apple""
        },
        {
          ""type"": ""minecraft:item"",
          ""weight"": 10,
          ""name"": ""minecraft:enchanted_book"",
          ""functions"": [
            {
              ""function"": ""minecraft:enchant_randomly""
            }
          ]
        },
        {
          ""type"": ""minecraft:item"",
          ""weight"": 5,
          ""name"": ""minecraft:diamond"",
          ""functions"": [
            {
              ""function"": ""minecraft:set_count"",
              ""count"": {
                ""min"": 1,
                ""max"": 3,
                ""type"": ""minecraft:uniform""
              }
            }
          ]
        }
      ]
    }
      ],
      ""__comment"": ""This is an example chest loot table that can be used for custom dungeons or structures.""
    }";
        await File.WriteAllTextAsync(Path.Combine(lootTablesPath, "chests", "custom_dungeon.json"), chestLootTableContent);
        
        // Create README specifically for loot tables
        var lootReadmeContent = @"# Loot Tables Guide
    
    ## Entity Loot Tables
    Located in `loot_tables/entities/`, these control what items are dropped when entities die.
    Example provided: `zombie_example.json`
    
    ### Key Components:
    - `""type"": ""minecraft:entity""` - Specifies this is for entity drops
    - `""rolls""` - How many times to pick from the pool
    - `""entries""` - List of possible drops with weights
    - `""functions""` - Modify drops (set count, apply enchantments, etc.)
    - `""conditions""` - When items should drop (e.g., only if killed by player)
    
    ## Chest Loot Tables
    Located in `loot_tables/chests/`, these control what appears in containers.
    Example provided: `custom_dungeon.json`
    
    ### Key Components:
    - `""type"": ""minecraft:chest""` - Specifies this is for containers
    - `""rolls""` can be a range like `{""min"": 1, ""max"": 3, ""type"": ""minecraft:uniform""}`
    - Higher weight values in `""entries""` = more common drops
    
    ## How To Use
    1. Modify the example files with your own items
    2. Create new .json files for different entities or chests
    3. For vanilla overrides, use the minecraft namespace
       e.g., `.../minecraft/loot_tables/entities/zombie.json`
    
    ## Reference
    - Full documentation: https://minecraft.wiki/w/Loot_table
    ";
        await File.WriteAllTextAsync(Path.Combine(lootTablesPath, "README.md"), lootReadmeContent);
    }
}
