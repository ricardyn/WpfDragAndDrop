using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WpfApp.DragAndDrop.MVVM.Core;

namespace WpfApp.DragAndDrop.MVVM.Behaviors;

public class DropTargetBehavior
{
    public readonly static DependencyProperty DropCommandProperty = DependencyProperty.RegisterAttached(
        "DropCommand",
        typeof(ICommand),
        typeof(DropTargetBehavior),
        new PropertyMetadata(defaultValue: null, propertyChangedCallback: _onDropCommandChanged)
    );

    public static ICommand GetDropCommand(DependencyObject obj) => (ICommand)obj.GetValue(DropCommandProperty);
    public static void SetDropCommand(DependencyObject obj, ICommand value) => obj.SetValue(DropCommandProperty, value);

    public readonly static DependencyProperty DroppedItemTypeProperty = DependencyProperty.RegisterAttached(
        "DroppedItemType",
        typeof(Type),
        typeof(DropTargetBehavior),
        new PropertyMetadata(defaultValue: null)
    );

    public static Type? GetDroppedItemType(DependencyObject obj) => (Type?)obj.GetValue(DroppedItemTypeProperty);
    public static void SetDroppedItemType(DependencyObject obj, Type? value) => obj.SetValue(DroppedItemTypeProperty, value);

    public readonly static DependencyProperty TargetItemTypeProperty = DependencyProperty.RegisterAttached(
        "TargetItemType",
        typeof(Type),
        typeof(DropTargetBehavior),
        new PropertyMetadata(defaultValue: null)
    );

    public static Type? GetTargetItemType(DependencyObject obj) => (Type?)obj.GetValue(TargetItemTypeProperty);
    public static void SetTargetItemType(DependencyObject obj, Type? value) => obj.SetValue(TargetItemTypeProperty, value);

    private static void _onDropCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UIElement element) return;

        element.AllowDrop = true;

        element.Drop -= _onDrop;
        element.Drop += _onDrop;

        element.DragOver -= _onDragOver;
        element.DragOver += _onDragOver;
    }

    private static void _onDragOver(object sender, DragEventArgs e)
    {
        // Destaca (seleciona) o item sob o cursor enquanto o mouse passa por cima.
        if (sender is not ListView listView) return;

        ListViewItem? container = _getItemUnderMouse(listView, e);

        if (container != null)
        {
            container.IsSelected = true;
        }
    }

    private static void _onDrop(object sender, DragEventArgs e)
    {
        if (sender is not ListView listView) return;

        Type? droppedItemType = GetDroppedItemType(listView);

        if (droppedItemType == null) return;

        if (!e.Data.GetDataPresent(droppedItemType)) return;

        object? droppedItem = e.Data.GetData(droppedItemType);

        if (droppedItem == null) return;

        // Descobre o item sob o cursor e valida que ele é do tipo configurado.
        object? targetItem = _getTargetItem(listView, e);

        ICommand command = GetDropCommand(listView);

        var payload = new DropPayload(droppedItem, targetItem);

        if (!command.CanExecute(payload)) return;

        command.Execute(payload);
    }

    private static object? _getTargetItem(ListView listView, DragEventArgs e)
    {
        Type? targetItemType = GetTargetItemType(listView);

        if (targetItemType == null) return null;

        ListViewItem? container = _getItemUnderMouse(listView, e);

        object? dataContext = container?.DataContext;

        if (dataContext == null) return null;

        // Só retorna o item se ele for do tipo configurado em TargetItemType.
        return targetItemType.IsInstanceOfType(dataContext) ? dataContext : null;
    }

    private static ListViewItem? _getItemUnderMouse(ListView listView, DragEventArgs e)
    {
        var element = listView.InputHitTest(e.GetPosition(listView)) as DependencyObject;

        while (element != null && element is not ListViewItem)
        {
            element = VisualTreeHelper.GetParent(element);
        }

        return element as ListViewItem;
    }
}
