using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WpfApp.DragAndDrop.MVVM.Behaviors;

public class DragSourceBehavior
{
    private static Point _dragStartPoint;

    public readonly static DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
        "IsEnabled",
        typeof(bool),
        typeof(DragSourceBehavior),
        new PropertyMetadata(defaultValue: false, propertyChangedCallback: _onIsEnabledChanged)
    );

    public static bool GetIsEnabled(DependencyObject obj) => (bool)obj.GetValue(IsEnabledProperty);
    public static void SetIsEnabled(DependencyObject obj, bool value) => obj.SetValue(IsEnabledProperty, value);

    private static void _onIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ListView listView) return;

        if ((bool)e.NewValue)
        {
            listView.PreviewMouseLeftButtonDown += _onPreviewMouseLeftButtonDown;
            listView.MouseMove += _onMouseMove;
        }
        else
        {
            listView.PreviewMouseLeftButtonDown -= _onPreviewMouseLeftButtonDown;
            listView.MouseMove -= _onMouseMove;
        }
    }

    private static void _onPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _dragStartPoint = e.GetPosition(null);
    }

    private static void _onMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed || sender is not ListView listView) return;

        Point mousePos = e.GetPosition(null);
        Vector diff = _dragStartPoint - mousePos;

        if (Math.Abs(diff.X) <= SystemParameters.MinimumHorizontalDragDistance &&
            Math.Abs(diff.Y) <= SystemParameters.MinimumVerticalDragDistance) return;

        var depObj = (DependencyObject?)e.OriginalSource;
        
        while (depObj != null && depObj is not ListViewItem)
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
