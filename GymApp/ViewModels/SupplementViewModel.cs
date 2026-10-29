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
    private bool isSupplementListEmpty = true;

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
        var supplement = new Supplement
        {
            FirebaseId = _editingSupplementFirebaseId ?? "",
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
        try
        {
            if (_editingSupplementFirebaseId == null)
                await _supplementService.AddSupplementAsync(supplement);
            else
                await _supplementService.UpdateSupplementAsync(supplement);
        }
        catch (Exception ex)
        {
            ReportCloudFailure(ex, "save the supplement", canRetry: false);
            ValidationMessage = StatusMessage;
            return;
        }
        _editingSupplementFirebaseId = null;

        SupplementName = "";
        DosageUnit = "";
        Description = "";
        
        await LoadSupplementAsync(CurrentUserId);
        SupplementSaved?.Invoke();
    }

    public async Task LoadSupplementAsync(string userId)
    {
        SetStatusMessage("Loading supplements from Firebase…");
        IsSupplementListEmpty = false;
        try
        {
            var supplementList = await _supplementService.GetAllSupplementsAsync(userId);
            Suplements.Clear();
            foreach (var supplement in supplementList)
                Suplements.Add(supplement);
            IsSupplementListEmpty = Suplements.Count == 0;
            ClearStatusMessage();
        }
        catch (Exception ex)
        {
            IsSupplementListEmpty = false;
            ReportCloudFailure(ex, "load supplements");
        }
    }

    public override Task RetryAsync() => LoadSupplementAsync(CurrentUserId);

    [RelayCommand]
    private async Task DeleteSupplementAsync()
    {
        if (SelectedSupplement == null)
            return;

        try { await _supplementService.DeleteSupplementAsync(SelectedSupplement); }
        catch (Exception ex)
        {
            ReportCloudFailure(ex, "delete the supplement", canRetry: false);
            ValidationMessage = StatusMessage;
            return;
        }

        SupplementName = "";
        DosageUnit = "";
        Description = "";
        _editingSupplementFirebaseId = null;
        
        await LoadSupplementAsync(CurrentUserId);
    }

    partial void OnSelectedSupplementChanged(Supplement? value)
    {
        HasSelection = value != null;
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
        ValidationMessage = "";
    }
}
