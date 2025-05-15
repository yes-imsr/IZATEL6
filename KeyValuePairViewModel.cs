using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PMMOEdit
{
    public class KeyValuePairViewModel : INotifyPropertyChanged
    {
        private string _key;
        private double _value;
        
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
        
        public KeyValuePairViewModel(string key, double value)
        {
            _key = key;
            _value = value;
        }
        
        public event PropertyChangedEventHandler? PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
