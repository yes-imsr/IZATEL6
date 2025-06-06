using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;

namespace PMMOEdit;

public enum MessageBoxButtons
{
    Ok
}

public class MessageBox
{
    public static async Task Show(Window parent, string message, string title, MessageBoxButtons buttons)
    {
        var dialog = new Window
        {
            Title = title,
            Width = 400,
            Height = 150,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        var messageText = new TextBlock
        {
            Text = message,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            Margin = new Avalonia.Thickness(20)
        };

        var okButton = new Button
        {
            Content = "OK",
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Width = 80,
            Margin = new Avalonia.Thickness(0, 0, 0, 20)
        };

        var panel = new StackPanel();
        panel.Children.Add(messageText);
        panel.Children.Add(okButton);

        dialog.Content = panel;

        var tcs = new TaskCompletionSource<object>();
        okButton.Click += (s, e) =>
        {
            dialog.Close();
            tcs.TrySetResult(null);
        };

        await dialog.ShowDialog(parent);
        await tcs.Task;
    }
    
    public static Task Show(string message, string title)
    {
        var mainWindow = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.MainWindow
            : null;
            
        if (mainWindow != null)
        {
            return Show(mainWindow, message, title, MessageBoxButtons.Ok);
        }
        else
        {
            var window = new Window
            {
                Title = title,
                Width = 400,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            
            window.Show(); // window visibility check
            return Show(window, message, title, MessageBoxButtons.Ok);
        }
    }
}