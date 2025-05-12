using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PMMOEdit
{
    public class Skill : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private long _maxLevel = 2147483647;
        private bool _displayGroupName;
        private bool _useTotalLevels;
        private int _color = 16777215;
        private bool _showInList = true;
        private string _icon = "pmmo:textures/skills/missing_icon.png";
        private int _iconSize = 18;
        private bool _noAfkPenalty;

        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        public long MaxLevel
        {
            get => _maxLevel;
            set
            {
                if (_maxLevel != value)
                {
                    _maxLevel = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool DisplayGroupName
        {
            get => _displayGroupName;
            set
            {
                if (_displayGroupName != value)
                {
                    _displayGroupName = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool UseTotalLevels
        {
            get => _useTotalLevels;
            set
            {
                if (_useTotalLevels != value)
                {
                    _useTotalLevels = value;
                    OnPropertyChanged();
                }
            }
        }

        public int Color
        {
            get => _color;
            set
            {
                if (_color != value)
                {
                    _color = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ColorHex));
                }
            }
        }
        
        public string ColorHex
        {
            get
            {
                byte r = (byte)((Color >> 16) & 0xFF);
                byte g = (byte)((Color >> 8) & 0xFF);
                byte b = (byte)(Color & 0xFF);
                return $"#{r:X2}{g:X2}{b:X2}";
            }
            set
            {
                if (value != null && value.StartsWith("#") && value.Length == 7)
                {
                    string hexColor = value.Substring(1);
                    if (int.TryParse(hexColor, System.Globalization.NumberStyles.HexNumber, null, out int colorValue))
                    {
                        Color = colorValue;
                    }
                }
            }
        }

        public bool ShowInList
        {
            get => _showInList;
            set
            {
                if (_showInList != value)
                {
                    _showInList = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Icon
        {
            get => _icon;
            set
            {
                if (_icon != value)
                {
                    _icon = value;
                    OnPropertyChanged();
                }
            }
        }

        public int IconSize
        {
            get => _iconSize;
            set
            {
                if (_iconSize != value)
                {
                    _iconSize = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool NoAfkPenalty
        {
            get => _noAfkPenalty;
            set
            {
                if (_noAfkPenalty != value)
                {
                    _noAfkPenalty = value;
                    OnPropertyChanged();
                }
            }
        }

        public Dictionary<string, decimal>? GroupFor { get; set; }
        
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(Name)
                   && MaxLevel > 0
                   && IconSize > 0
                   && !string.IsNullOrEmpty(Icon);
        }

        public bool IsGroupSkill()
        {
            return GroupFor != null && GroupFor.Count > 0;
        }
    }
}