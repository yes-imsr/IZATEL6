using System;
using System.IO;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using PMMOEdit.Models;
using PMMOEdit.Helpers;

namespace PMMOEdit
{
    public class ServerPageViewModel : INotifyPropertyChanged
    {
        private ServerConfig _serverConfig;
        private string? _currentFilePath;
        private bool _hasFileChanges;
        private bool _hasNewFileChanges;

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
                string tomlContent = TomlServerConfigParser.GenerateTomlConfig(ServerConfig);
                File.WriteAllText(targetPath, tomlContent);

                if (filePath != null)
                    CurrentFilePath = filePath;

                HasFileChanges = false;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving server config: {ex.Message}");
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
