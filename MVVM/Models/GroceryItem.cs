namespace WpfApp.DragAndDrop.MVVM.Models;

public class GroceryItem
{
    public string Name { get; set; }
    public int Quantity { get; set; }

    public override string ToString() => $"{Name} | {Quantity}";
}
