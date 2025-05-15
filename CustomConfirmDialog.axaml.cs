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
        
        public string Title { get; set; } = "Confirm";
        public string Message { get; set; } = "Are you sure?";
        public string ConfirmButtonText { get; set; } = "Confirm";
        public string CancelButtonText { get; set; } = "Cancel";
        public bool ShowCancelButton { get; set; } = true;
        
        public CustomConfirmDialog()
        {
            InitializeComponent();
            
            var confirmButton = this.FindControl<Button>("ConfirmButton");
            var cancelButton = this.FindControl<Button>("CancelButton");
            
            confirmButton.Click += (sender, args) => 
            {
                _tcs.SetResult(true);
                Close();
            };
            
            cancelButton.Click += (sender, args) => 
            {
                _tcs.SetResult(false);
                Close();
            };
            
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
            
            titleText.Text = Title;
            messageText.Text = Message;
            confirmButton.Content = ConfirmButtonText;
            cancelButton.Content = CancelButtonText;
            
            if (!ShowCancelButton)
            {
                cancelButton.IsVisible = false;
            }
        }
        
        public Task<bool> ShowDialog(Window owner)
        {
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            _tcs = new TaskCompletionSource<bool>();
            this.Show(owner);
            return _tcs.Task;
        }
    }
}
