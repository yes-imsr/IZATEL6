using System;

namespace PMMOEdit
{
    // Simple implementation of CommandManager for Avalonia
    // since Avalonia doesn't have the WPF CommandManager built-in
    public static class CommandManager
    {
        public static event EventHandler RequerySuggested;
        
        public static void InvalidateRequerySuggested()
        {
            RequerySuggested?.Invoke(null, EventArgs.Empty);
        }
    }
}
