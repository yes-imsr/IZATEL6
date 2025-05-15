using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;

namespace PMMOEdit
{
    public enum MessageBoxButtons
    {
        Ok
    }
    
    public static class MessageBox
    {
        // Main method for showing message dialogs
        public static Task<string> ShowAsync(string message, string title = "Message")
        {
            var msgBox = new Window
            {
                Title = title,
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                CanResize = false
            };
            
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            
            var textBlock = new TextBlock
            {
                Text = message,
                Margin = new Thickness(20, 20, 20, 20),
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                MaxWidth = 400
            };
            
            Grid.SetRow(textBlock, 0);
            grid.Children.Add(textBlock);
            
            var button = new Button
            {
                Content = "OK",
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20),
                Width = 100
            };
            
            Grid.SetRow(button, 1);
            grid.Children.Add(button);
            
            msgBox.Content = grid;
            
            var tcs = new TaskCompletionSource<string>();
            button.Click += (_, __) => 
            {
                tcs.SetResult("OK");
                msgBox.Close();
            };
            
            msgBox.Closed += (_, __) => 
            {
                if (!tcs.Task.IsCompleted)
                    tcs.SetResult("Cancel");
            };
            
            var lifetime = Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
            if (lifetime != null)
            {
                msgBox.ShowDialog(lifetime.MainWindow);
            }
            else
            {
                msgBox.Show();
            }
            
            return tcs.Task;
        }
        
        // For compatibility with existing code
        public static Task Show(string message, string title = "Message")
        {
            return ShowAsync(message, title);
        }
        
        // Show dialog with a specific parent window
        public static Task Show(Window parent, string message, string title, MessageBoxButtons buttons = MessageBoxButtons.Ok)
        {
            var msgBox = new Window
            {
                Title = title,
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false
            };
            
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            
            var textBlock = new TextBlock
            {
                Text = message,
                Margin = new Thickness(20, 20, 20, 20),
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                MaxWidth = 400
            };
            
            Grid.SetRow(textBlock, 0);
            grid.Children.Add(textBlock);
            
            var button = new Button
            {
                Content = "OK",
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20),
                Width = 100
            };
            
            Grid.SetRow(button, 1);
            grid.Children.Add(button);
            
            msgBox.Content = grid;
            
            var tcs = new TaskCompletionSource<string>();
            button.Click += (_, __) => 
            {
                tcs.SetResult("OK");
                msgBox.Close();
            };
            
            msgBox.Closed += (_, __) => 
            {
                if (!tcs.Task.IsCompleted)
                    tcs.SetResult("Cancel");
            };
            
            msgBox.ShowDialog(parent);
            return tcs.Task;
        }
    }
}