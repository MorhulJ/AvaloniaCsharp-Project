using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymApp.Models;
using GymApp.Services;
using GymApp.Validators;

namespace GymApp.ViewModels;

public partial class SupplementViewModel : ViewModelBase
{
    private readonly SupplementService _supplementService;

    public ObservableCollection<Supplement> Suplements { get; } = new();

    [ObservableProperty] 
    private string validationMessage = "";
    [ObservableProperty] 
    private string supplementName = "";
    [ObservableProperty] 
    private string dosageUnit = "";
    [ObservableProperty] 
    private string description = "";
    [ObservableProperty]
    private Supplement? selectedSupplement;

    private string? _editingSupplementFirebaseId;
    
    public string CurrentUserId { get; set; } = "";
    public event Action? SupplementSaved;

    public SupplementViewModel(SupplementService supplementService)
    {
        _supplementService = supplementService;
    }

    private readonly SupplementValidator _validator = new();

    [RelayCommand]
    private async Task SaveSupplementAsync()
    {
        if (_editingSupplementFirebaseId == null)
        {
            var supplement = new Supplement
            {
                UserId = CurrentUserId,
                Name = supplementName,
                DosageUnit = dosageUnit,
                Description = description
            };
            
            var result = _validator.Validate(supplement);

            if (!result.IsValid)
            {
                ValidationMessage = result.Errors[0].ErrorMessage;
                return;
            }

            ValidationMessage = "";
            await _supplementService.AddSupplementAsync(supplement);
        }
        else
        {
            var supplement = new Supplement
            {
                FirebaseId = _editingSupplementFirebaseId,
                UserId = CurrentUserId,
                Name = supplementName,
                DosageUnit = dosageUnit,
                Description = description
            };
            
            await _supplementService.UpdateSupplementAsync(supplement);
            _editingSupplementFirebaseId = null;
        }

        SupplementName = "";
        DosageUnit = "";
        Description = "";
        
        await LoadSupplementAsync(CurrentUserId);
        SupplementSaved?.Invoke();
    }

    public async Task LoadSupplementAsync(string userId)
    {
        var supplementList = await _supplementService.GetAllSupplementsAsync(userId);
        
        Suplements.Clear();
        foreach (var supplement in supplementList)
            Suplements.Add(supplement);
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
        _editingSupplementFirebaseId = null;
        
        await LoadSupplementAsync(CurrentUserId);
    }

    partial void OnSelectedSupplementChanged(Supplement? value)
    {
        if (value == null) return;
    
        SupplementName = value.Name;
        DosageUnit = value.DosageUnit;
        Description = value.Description;
        _editingSupplementFirebaseId = value.FirebaseId;
    }
    
    public void ResetEditingState()
    {
        _editingSupplementFirebaseId = null;
    }

    public void ResetValidation()
    {
        validationMessage = "";
    }
}