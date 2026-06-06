using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WpfCommander.Models;
using WpfCommander.ViewModels;

namespace WpfCommander.Views
{
    public partial class PanelView : UserControl
    {
        public PanelView()
        {
            InitializeComponent();
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is PanelViewModel vm)
            {
                vm.SelectedItems = ItemsGrid.SelectedItems
                    .Cast<FileSystemItem>()
                    .ToList();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private void OnRowDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is not PanelViewModel vm) return;

            var src = e.OriginalSource as DependencyObject;
            while (src != null && src is not DataGridRow)
                src = VisualTreeHelper.GetParent(src);

            if (src is DataGridRow && vm.SelectedItem != null)
                vm.OpenItemCommand.Execute(vm.SelectedItem);
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter
                && DataContext is PanelViewModel vm
                && vm.SelectedItem != null)
            {
                vm.OpenItemCommand.Execute(vm.SelectedItem);
                e.Handled = true;
            }
            else if (e.Key == Key.Back && DataContext is PanelViewModel vm2)
            {
                if (vm2.GoUpCommand.CanExecute(null))
                {
                    vm2.GoUpCommand.Execute(null);
                    e.Handled = true;
                }
            }
        }
    }
}
