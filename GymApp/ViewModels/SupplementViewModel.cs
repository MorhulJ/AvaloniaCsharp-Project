using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymApp.Models;
using GymApp.Services;

namespace GymApp.ViewModels;

public partial class SupplementViewModel : ViewModelBase
{
    private readonly SupplementService _supplementService;

    public ObservableCollection<Supplement> Suplements { get; } = new();

    [ObservableProperty] 
    private string supplementName = "";
    [ObservableProperty] 
    private string dosageUnit = "";
    [ObservableProperty] 
    private string description = "";
    [ObservableProperty]
    private Supplement? selectedSupplement;

    private int? _editingSupplementId;

    public SupplementViewModel(SupplementService supplementService)
    {
        _supplementService = supplementService;
    }

    [RelayCommand]
    private async Task SaveSupplementAsync()
    {
        if (_editingSupplementId == null)
        {
            var supplement = new Supplement()
            {
                Name = supplementName,
                DosageUnit = dosageUnit,
                Description = description
            };
            
            await _supplementService.AddSupplementAsync(supplement);
        }
        else
        {
            var supplement = new Supplement()
            {
                Id = _editingSupplementId.Value,
                Name = supplementName,
                DosageUnit = dosageUnit,
                Description = description
            };
            
            await _supplementService.UpdateSupplementAsync(supplement);
            
            _editingSupplementId = null;
        }

        SupplementName = "";
        DosageUnit = "";
        Description = "";
        
        await LoadSupplementAsync();
    }

    public async Task LoadSupplementAsync()
    {
        var supplementList = await _supplementService.GetAllSupplementsAsync();
        
        Suplements.Clear();
        
        foreach (var supplement in supplementList)
        {
            Suplements.Add(supplement);
        }
    }

    [RelayCommand]
    private async Task DeleteSupplementAsync()
    {
        if (SelectedSupplement == null)
            return;

        await _supplementService.DeleteSupplementAsync(SelectedSupplement);

        SupplementName = "";
        DosageUnit = "";
        Description = "";
        _editingSupplementId = null;
        
        await LoadSupplementAsync();
    }

    partial void OnSelectedSupplementChanged(Supplement? value)
    {
        if (value == null) return;
    
        SupplementName = value.Name;
        DosageUnit = value.DosageUnit;
        Description = value.Description;
        
        _editingSupplementId = value.Id;
    }
}