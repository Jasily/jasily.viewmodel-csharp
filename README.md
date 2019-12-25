# Jasily.ViewModel-csharp

Base view model with no external dependencies.

## Examples

### Example 1

``` cs
class YourViewModel : BaseViewModel
{
    [ModelProperty]
    public string Name { get; set; }

    [ModelProperty]
    public string Age { get; set; }
}

var vm = new YourViewModel();
vm.RefreshProperties(); // raise PropertyChanged for "Name" and "Age".
```
