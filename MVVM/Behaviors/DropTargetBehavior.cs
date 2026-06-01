using System.Windows;
using System.Windows.Input;

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

    private static void _onDropCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UIElement element) return;

        element.AllowDrop = true;
        element.Drop -= _onDrop;
        element.Drop += _onDrop;
    }

    private static void _onDrop(object sender, DragEventArgs e)
    {
        if (sender is not DependencyObject d) return;

        Type? droppedItemType = GetDroppedItemType(d);
        
        if (droppedItemType == null) return;

        if (!e.Data.GetDataPresent(droppedItemType)) return;

        object? droppedItem = e.Data.GetData(droppedItemType);
        
        if (droppedItem == null) return;

        ICommand command = GetDropCommand(d);

        if (!command.CanExecute(droppedItem)) return;

        command.Execute(droppedItem);
    }
}
