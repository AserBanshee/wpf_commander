using System.Windows;
using System.Windows.Input;
using WpfCommander.ViewModels;

namespace WpfCommander.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnPanelGotFocus(object sender, KeyboardFocusChangedEventArgs e)
            => ActivatePanel(sender);

        private void OnPanelPreviewMouseDown(object sender, MouseButtonEventArgs e)
            => ActivatePanel(sender);

        private void ActivatePanel(object sender)
        {
            if (sender is FrameworkElement fe
                && fe.DataContext is PanelViewModel panel
                && DataContext is MainViewModel main)
            {
                main.SetActive(panel);
            }
        }
    }
}
