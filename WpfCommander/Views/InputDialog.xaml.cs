using System.Windows;

namespace WpfCommander.Views
{
    public partial class InputDialog : Window
    {
        public string ResponseText { get; private set; } = string.Empty;

        public InputDialog(string title, string prompt, string defaultValue)
        {
            InitializeComponent();
            Title = title;
            PromptText.Text = prompt;
            InputBox.Text = defaultValue;
            Loaded += (_, _) =>
            {
                InputBox.SelectAll();
                InputBox.Focus();
            };
        }

        private void OnOk(object sender, RoutedEventArgs e)
        {
            ResponseText = InputBox.Text;
            DialogResult = true;
        }
    }
}
