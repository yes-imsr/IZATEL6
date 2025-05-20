using System;
using System.IO;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using PMMOEdit.Models;
using PMMOEdit.Helpers;

using System.Collections.ObjectModel;
using TomlServerConfigParser = PMMOEdit.Models.TomlServerConfigParser;

namespace PMMOEdit
{
    public class ServerPageViewModel : INotifyPropertyChanged
    {
        private ServerConfig _serverConfig;
        private string? _currentFilePath;
        private bool _hasFileChanges;
        private bool _hasNewFileChanges;
        
        // Collections for the various KeyValuePair bindings
        private ObservableCollection<KeyValuePairViewModel> _skillModifiers;
        private ObservableCollection<KeyValuePairViewModel> _partyBonuses;
        private ObservableCollection<KeyValuePairViewModel> _dealDamageXp;
        private ObservableCollection<KeyValuePairViewModel> _receiveDamageXp;
        private ObservableCollection<KeyValuePairViewModel> _jumpXp;
        private ObservableCollection<KeyValuePairViewModel> _playerActionXp;
        
        public ObservableCollection<KeyValuePairViewModel> SkillModifiers
        {
            get => _skillModifiers;
            set
            {
                if (_skillModifiers != value)
                {
                    _skillModifiers = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public ObservableCollection<KeyValuePairViewModel> PartyBonuses
        {
            get => _partyBonuses;
            set
            {
                if (_partyBonuses != value)
                {
                    _partyBonuses = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public ObservableCollection<KeyValuePairViewModel> DealDamageXp
        {
            get => _dealDamageXp;
            set
            {
                if (_dealDamageXp != value)
                {
                    _dealDamageXp = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public ObservableCollection<KeyValuePairViewModel> ReceiveDamageXp
        {
            get => _receiveDamageXp;
            set
            {
                if (_receiveDamageXp != value)
                {
                    _receiveDamageXp = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public ObservableCollection<KeyValuePairViewModel> JumpXp
        {
            get => _jumpXp;
            set
            {
                if (_jumpXp != value)
                {
                    _jumpXp = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public ObservableCollection<KeyValuePairViewModel> PlayerActionXp
        {
            get => _playerActionXp;
            set
            {
                if (_playerActionXp != value)
                {
                    _playerActionXp = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public ServerConfig ServerConfig
        {
            get => _serverConfig;
            set
            {
                if (_serverConfig != value)
                {
                    _serverConfig = value;
                    OnPropertyChanged();
                }
            }
        }
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
        public bool HasFileChanges
        {
            get => _hasFileChanges;
            set
            {
                if (_hasFileChanges != value)
                {
                    _hasFileChanges = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool HasNewFileChanges
        {
            get => _hasNewFileChanges;
            set
            {
                if (_hasNewFileChanges != value)
                {
                    _hasNewFileChanges = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool HasOpenedFile => !string.IsNullOrEmpty(CurrentFilePath);
        public ServerPageViewModel()
        {
            _serverConfig = new ServerConfig();
            _currentFilePath = null;
            _hasFileChanges = false;
            _hasNewFileChanges = false;
            
            // Initialize the collections
            _skillModifiers = new ObservableCollection<KeyValuePairViewModel>();
            _partyBonuses = new ObservableCollection<KeyValuePairViewModel>();
            _dealDamageXp = new ObservableCollection<KeyValuePairViewModel>();
            _receiveDamageXp = new ObservableCollection<KeyValuePairViewModel>();
            _jumpXp = new ObservableCollection<KeyValuePairViewModel>();
            _playerActionXp = new ObservableCollection<KeyValuePairViewModel>();
        }

        public bool LoadServerConfigFromFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return false;

                string tomlContent = File.ReadAllText(filePath);
                ServerConfig = TomlServerConfigParser.ParseServerConfig(tomlContent);
                CurrentFilePath = filePath;
                HasFileChanges = false;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading server config: {ex.Message}");
                return false;
            }
        }

        public bool SaveToFile(string? filePath = null)
        {
            if (filePath == null && string.IsNullOrEmpty(CurrentFilePath))
                return false;

            try
            {
                string targetPath = filePath ?? CurrentFilePath!;
                
                // Generate complete TOML content using our template-based approach
                string tomlContent = TomlServerConfigParser.GenerateTomlConfig(ServerConfig);
                
                // Log the size of the content for debugging
                System.Diagnostics.Debug.WriteLine($"Generated TOML content size: {tomlContent.Length} characters");
                
                // Ensure directory exists
                string? directory = Path.GetDirectoryName(targetPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                // Write the entire content to file in one operation
                File.WriteAllText(targetPath, tomlContent);
                
                // Verify the file was saved properly
                if (File.Exists(targetPath))
                {
                    var fileInfo = new FileInfo(targetPath);
                    System.Diagnostics.Debug.WriteLine($"Saved file size: {fileInfo.Length} bytes");
                    
                    if (fileInfo.Length < 100)
                    {
                        // File seems suspiciously small, something went wrong
                        System.Diagnostics.Debug.WriteLine("WARNING: Saved file is very small, may be truncated");
                        // Try one more time with different approach
                        File.WriteAllText(targetPath, tomlContent);
                    }
                }
        
                // Update current file path if different
                if (filePath != null)
                    CurrentFilePath = filePath;
        
                HasFileChanges = false;
                return true;
            }
            catch (Exception ex)
            {
                // Log detailed error information
                System.Diagnostics.Debug.WriteLine($"Error saving server config: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Exception type: {ex.GetType().Name}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                
                return false;
            }
        }

        // Create a new default server config
        public void CreateNewServerConfig()
        {
            ServerConfig = new ServerConfig();
            CurrentFilePath = null;
            HasFileChanges = false;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
