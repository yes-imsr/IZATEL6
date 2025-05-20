using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace PMMOEdit
{
    public partial class CustomConfirmDialog : Window
    {
        private TaskCompletionSource<bool> _tcs = new TaskCompletionSource<bool>();
        
        public new string Title { get; set; } = "Confirm";
        public string Message { get; set; } = "Are you sure?";
        public string ConfirmButtonText { get; set; } = "Confirm";
        public string CancelButtonText { get; set; } = "Cancel";
        public bool ShowCancelButton { get; set; } = true;
        
        public CustomConfirmDialog()
        {
            InitializeComponent();
            
            var confirmButton = this.FindControl<Button>("ConfirmButton");
            var cancelButton = this.FindControl<Button>("CancelButton");
            
            if (confirmButton != null)
            {
                confirmButton.Click += (sender, args) => 
                {
                    _tcs.SetResult(true);
                    Close();
                };
            }
            
            if (cancelButton != null)
            {
                cancelButton.Click += (sender, args) => 
                {
                    _tcs.SetResult(false);
                    Close();
                };
            }
            
            this.Closing += (sender, args) => 
            {
                _tcs.TrySetResult(false);
            };
        }
        
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        
        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            
            var titleText = this.FindControl<TextBlock>("TitleText");
            var messageText = this.FindControl<TextBlock>("MessageText");
            var confirmButton = this.FindControl<Button>("ConfirmButton");
            var cancelButton = this.FindControl<Button>("CancelButton");
            
            if (titleText != null)
                titleText.Text = Title;
                
            if (messageText != null)
                messageText.Text = Message;
                
            if (confirmButton != null)
                confirmButton.Content = ConfirmButtonText;
                
            if (cancelButton != null)
            {
                cancelButton.Content = CancelButtonText;
                
                if (!ShowCancelButton)
                {
                    cancelButton.IsVisible = false;
                }
            }
        }
        
        public new Task<bool> ShowDialog(Window owner)
        {
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            _tcs = new TaskCompletionSource<bool>();
            this.Show(owner);
            return _tcs.Task;
        }
    }
}
