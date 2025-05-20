using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace PMMOEdit
{
    public partial class TextInputDialog : Window
    {
        private TaskCompletionSource<string> _tcs = new TaskCompletionSource<string>();
        
        public new string Title { get; set; } = "Input";
        public string Message { get; set; } = "Please enter a value:";
        public string InitialText { get; set; } = "";
        
        public TextInputDialog()
        {
            InitializeComponent();
            
            var okButton = this.FindControl<Button>("OkButton");
            var cancelButton = this.FindControl<Button>("CancelButton");
            var inputTextBox = this.FindControl<TextBox>("InputTextBox");
            
            if (okButton != null && inputTextBox != null)
            {
                okButton.Click += (sender, args) => 
                {
                    _tcs.SetResult(inputTextBox.Text ?? string.Empty);
                    Close();
                };
            }
            
            if (cancelButton != null)
            {
                cancelButton.Click += (sender, args) => 
                {
                    _tcs.SetResult(string.Empty);
                    Close();
                };
            }
            
            this.Closing += (sender, args) => 
            {
                _tcs.TrySetResult(string.Empty);
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
            var inputTextBox = this.FindControl<TextBox>("InputTextBox");
            
            if (titleText != null)
                titleText.Text = Title;
                
            if (messageText != null)
                messageText.Text = Message;
                
            if (inputTextBox != null)
            {
                inputTextBox.Text = InitialText;
                inputTextBox.CaretIndex = InitialText.Length;
                inputTextBox.Focus();
            }
        }
        
        public new Task<string> ShowDialog(Window owner)
        {
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            _tcs = new TaskCompletionSource<string>();
            this.Show(owner);
            return _tcs.Task;
        }
    }
}
