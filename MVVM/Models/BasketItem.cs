using WpfApp.DragAndDrop.MVVM.Core;

namespace WpfApp.DragAndDrop.MVVM.Models;

public class BasketItem : ObservableObject
{
    private string _name;
    private int _quantity;

    public string Name {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public int Quantity {
        get => _quantity;
        set => SetProperty(ref _quantity, value);
    }

    public override string ToString() => $"{Name} | {Quantity}";
}
