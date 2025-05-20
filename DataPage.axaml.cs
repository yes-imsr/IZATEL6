using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;

namespace PMMOEdit;

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

    public DataPage()
    {
        InitializeComponent();
        
        var createDatapackButton = this.FindControl<Button>("CreateDatapackButton");
        if (createDatapackButton != null)
        {
            createDatapackButton.Click += CreateDatapackButton_Click;
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

This datapack was created with PMMO Editor.

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
            }
            else
            {
                messageBox.Show();
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
    
    private async Task CreateLootTablesTemplate(string namespacePath, string datapackName)
    {
        // Create loot_tables subfolders
        var lootTablesPath = Path.Combine(namespacePath, "loot_tables");
        Directory.CreateDirectory(Path.Combine(lootTablesPath, "entities"));
        Directory.CreateDirectory(Path.Combine(lootTablesPath, "chests"));
        
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
