using System.Collections.ObjectModel;
using WpfApp.DragAndDrop.MVVM.Core;
using WpfApp.DragAndDrop.MVVM.Models;

namespace WpfApp.DragAndDrop.MVVM.ViewModels;

public class MainWindowViewModel : ObservableObject
{
    private ObservableCollection<BasketItem> _basket = new();
    
    private ObservableCollection<BasketItem> _groceries = new()
    {
        new BasketItem { Name = "Apple", Quantity = 5},
        new BasketItem { Name = "Orange", Quantity = 6},
        new BasketItem { Name = "Banana", Quantity = 24},
        new BasketItem { Name = "Milk", Quantity = 1},
        new BasketItem { Name = "Egg", Quantity = 30},
        new BasketItem { Name = "Bread", Quantity = 6},
    };
    
    private BasketItem _selectedGroceryItem;
    private int _selectedGroceryItemIndex;
    private BasketItem _selectedBasketItem;
    private int _selectedBasketItemIndex;

    public ObservableCollection<BasketItem> Basket {
        get => _basket;
        set => SetProperty(ref _basket, value);
    }

    public ObservableCollection<BasketItem> Groceries {
        get => _groceries;
        set => SetProperty(ref _groceries, value);
    }

    public BasketItem SelectedGroceryItem {
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
}
