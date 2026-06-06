using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FileVault.Models;
using FileVault.ViewModels;

namespace FileVault.Views;

public partial class ExplorerPane : UserControl
{
    // Получаем VM панели
    private ExplorerPaneVM? PaneVM => DataContext as ExplorerPaneVM;

    public ExplorerPane()
    {
        InitializeComponent();
    }

    // Двойной клик — войти в папку
    private void Row_DoubleClick(object sender, MouseButtonEventArgs e)
    {
        PaneVM?.OpenSelected();
    }

    // Enter — войти, Backspace — вверх
    private void List_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)    { PaneVM?.OpenSelected(); e.Handled = true; }
        if (e.Key == Key.Back)     { PaneVM?.GoUp();         e.Handled = true; }
    }

    // Клик по панели — сделать активной
    private void Panel_MouseDown(object sender, MouseButtonEventArgs e)
    {
        // Поднимаем событие к MainViewModel через команду
        var main = Window.GetWindow(this)?.DataContext as MainViewModel;
        if (main == null || PaneVM == null) return;

        if (PaneVM == main.LeftPane)
            main.ActivateLeftCommand.Execute(null);
        else
            main.ActivateRightCommand.Execute(null);
    }

    // Чекбокс выделения строки
    private void CheckBox_Changed(object sender, RoutedEventArgs e)
    {
        // Биндинг сам обновляет FsEntry.IsSelected
    }
}
