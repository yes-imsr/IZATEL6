using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PMMOEdit
{
    public class KeyValuePairViewModel : INotifyPropertyChanged
    {
        private string _key;
        private double _value;
        private string _skillName;

        public string Key
        {
            get => _key;
            set
            {
                if (_key != value)
                {
                    _key = value;
                    OnPropertyChanged();
                }
            }
        }

        public double Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SkillName
        {
            get => _skillName;
            set
            {
                if (_skillName != value)
                {
                    _skillName = value;
                    OnPropertyChanged();
                }
            }
        }

        public KeyValuePairViewModel() 
        {
            _key = string.Empty;
            _value = 0;
            _skillName = string.Empty;
        }

        // Changed parameter order to avoid signature conflict
        public KeyValuePairViewModel(string key, double value, bool dummy = false)
        {
            _key = key;
            _value = value;
            _skillName = string.Empty;
        }

        public KeyValuePairViewModel(string key, string skillName, double value)
        {
            _key = key;
            _value = value;
            _skillName = skillName;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        // Only one OnPropertyChanged method
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
