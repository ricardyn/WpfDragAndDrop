using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WpfApp.DragAndDrop.MVVM.Core;

/// <summary>
/// Represents a base class for objects that implement the INotifyPropertyChanged interface.
/// This class provides a convenient way to notify subscribers when a property value changes.
/// </summary>
public class ObservableObject : INotifyPropertyChanged, INotifyPropertyChanging
{
    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc />
    public event PropertyChangingEventHandler? PropertyChanging;

    /// <summary>
    /// Invoked when the value of a property changes.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed. Used for data binding.</param>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Invoked when the value of a property is about to change.
    /// </summary>
    /// <param name="propertyName">The name of the property that is changing. Used for data binding.</param>
    protected virtual void OnPropertyChanging([CallerMemberName] string? propertyName = null)
    {
        PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
    }

    /// <summary>
    /// Updates a backing field and raises property change notifications when the value changes.
    /// </summary>
    /// <typeparam name="T">Type of the property.</typeparam>
    /// <param name="storage">Reference to the backing field.</param>
    /// <param name="value">Value to assign.</param>
    /// <param name="propertyName">Property name (auto populated).</param>
    /// <returns>True when the value changed; otherwise false.</returns>
    protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(storage, value)) return false;

        OnPropertyChanging(propertyName);
        storage = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
