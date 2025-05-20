using System;

namespace PMMOEdit
{

    public static class CommandManager
    {
        public static event EventHandler? RequerySuggested;
        
        public static void InvalidateRequerySuggested()
        {
            RequerySuggested?.Invoke(null, EventArgs.Empty);
        }
    }
}
