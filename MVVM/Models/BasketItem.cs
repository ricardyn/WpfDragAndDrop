namespace WpfApp.DragAndDrop.MVVM.Models;

public class BasketItem
{
    public string Name { get; set; }
    public int Quantity { get; set; }

    public override string ToString() => $"{Name} | {Quantity}";
}
