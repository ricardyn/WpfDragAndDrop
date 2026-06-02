using System.Collections.ObjectModel;
using WpfApp.DragAndDrop.MVVM.Core;
using WpfApp.DragAndDrop.MVVM.Models;

namespace WpfApp.DragAndDrop.MVVM.ViewModels;

public class MainWindowViewModel : ObservableObject
{
    private ObservableCollection<BasketItem> _basket = new();
    
    private ObservableCollection<GroceryItem> _groceries = new()
    {
        new GroceryItem { Name = "Apple", Quantity = 5},
        new GroceryItem { Name = "Orange", Quantity = 6},
        new GroceryItem { Name = "Banana", Quantity = 24},
        new GroceryItem { Name = "Milk", Quantity = 1},
        new GroceryItem { Name = "Egg", Quantity = 30},
        new GroceryItem { Name = "Bread", Quantity = 6},
    };
    
    private GroceryItem _selectedGroceryItem;
    private int _selectedGroceryItemIndex;
    private BasketItem _selectedBasketItem;
    private int _selectedBasketItemIndex;

    public ObservableCollection<BasketItem> Basket {
        get => _basket;
        set => SetProperty(ref _basket, value);
    }

    public ObservableCollection<GroceryItem> Groceries {
        get => _groceries;
        set => SetProperty(ref _groceries, value);
    }

    public GroceryItem SelectedGroceryItem {
        get => _selectedGroceryItem;
        set => SetProperty(ref _selectedGroceryItem, value);
    }

    public int SelectedGroceryItemIndex {
        get => _selectedGroceryItemIndex;
        set => SetProperty(ref _selectedGroceryItemIndex, value);
    }

    public BasketItem SelectedBasketItem {
        get => _selectedBasketItem;
        set => SetProperty(ref _selectedBasketItem, value);
    }

    public int SelectedBasketItemIndex {
        get => _selectedBasketItemIndex;
        set => SetProperty(ref _selectedBasketItemIndex, value);
    }

    private RelayCommand? _dropItemCommand;

    public RelayCommand DropItemCommand {
        get => _dropItemCommand ??= new RelayCommand(_dropItem);
    }

    private void _dropItem(object? obj)
    {
        if (obj is not GroceryItem droppedItem) return;

        BasketItem? allreadyInBasket = Basket.FirstOrDefault(dr => dr.Name == droppedItem.Name);
        
        if (allreadyInBasket != null)
        {
            allreadyInBasket.Quantity += droppedItem.Quantity;
        }
        else
        {
            Basket.Add(new BasketItem { Name = droppedItem.Name, Quantity = droppedItem.Quantity });
        }
    }
}
