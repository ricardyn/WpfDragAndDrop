using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WpfApp.DragAndDrop.MVVM.Models;
using WpfApp.DragAndDrop.MVVM.ViewModels;

namespace WpfApp.DragAndDrop.MVVM.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private MainWindowViewModel _viewModel;
    private Point _dragStartPoint;
    
    public MainWindow()
    {
        InitializeComponent();
        _viewModel = new MainWindowViewModel();
        DataContext = _viewModel;
    }

    private void ListViewGroceries_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _dragStartPoint = e.GetPosition(null);
    }

    private void ListViewGroceries_OnMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed || sender is not ListView listView) return;

        Point mousePos = e.GetPosition(null);
        Vector diff = _dragStartPoint - mousePos;

        if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
            Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
        {
            // Encontra qual item visual foi clicado
            var depObj = (DependencyObject)e.OriginalSource;
                
            while (depObj != null && !(depObj is ListViewItem))
            {
                depObj = VisualTreeHelper.GetParent(depObj);
            }

            if (depObj is not ListViewItem item) return;

            object draggedData = listView.ItemContainerGenerator.ItemFromContainer(item);

            if (draggedData == null) return;

            DataObject dragData = new(draggedData);
            DragDrop.DoDragDrop(listView, dragData, DragDropEffects.All);
        }
    }

    private void ListViewBasket_OnDrop(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(typeof(BasketItem))) return;

        if (e.Data.GetData(typeof(BasketItem)) is not BasketItem droppedItem) return;
            
        _viewModel.Groceries.Remove(droppedItem);
        _viewModel.Basket.Add(droppedItem);
    }
}
