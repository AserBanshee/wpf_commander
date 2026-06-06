using System.Windows;

namespace FileVault.Views;

public partial class NamePrompt : Window
{
    public string EnteredName => InputBox.Text;

    public NamePrompt(string title, string label)
    {
        InitializeComponent();
        Title = title;
        LabelText.Text = label;
        InputBox.Focus();
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(InputBox.Text)) return;
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
        => DialogResult = false;
}
