using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymApp.Models;
using GymApp.Services;
using GymApp.Validators;

namespace GymApp.ViewModels;

public partial class SupplementIntakeViewModel : ViewModelBase
{
    public List<DayOfWeek> DayOptions { get; } = new()
    {
        DayOfWeek.Monday,
        DayOfWeek.Tuesday,
        DayOfWeek.Wednesday,
        DayOfWeek.Thursday,
        DayOfWeek.Friday,
        DayOfWeek.Saturday,
        DayOfWeek.Sunday
    };
    
    private readonly SupplementIntakeService _supplementIntakeService;
    private readonly SupplementService _supplementService;

    public ObservableCollection<SupplementIntake> SupplementIntakes { get; } = new();

    [ObservableProperty]
    private bool isIntakeListEmpty = true;
    public ObservableCollection<Supplement> Supplements { get; } = new();

    [ObservableProperty] 
    private string validationMessage = "";
    [ObservableProperty]
    private Supplement? supplement;
    [ObservableProperty]
    private double supplementDosage;
    [ObservableProperty]
    private DayOfWeek supplementDay;
    [ObservableProperty]
    private TimeSpan supplementTime;
    [ObservableProperty]
    private SupplementIntake? selectedSupplementIntake;

    public string CurrentUserId { get; set; } = "";
    public event Action? IntakeSaved;

    public SupplementIntakeViewModel(SupplementIntakeService supplementIntakeService, SupplementService supplementService)
    {
        _supplementIntakeService = supplementIntakeService;
        _supplementService = supplementService;
    }
    
    private readonly SupplementIntakeValidator _validator = new();

    [RelayCommand]
    private async Task SaveSupplementIntakeAsync()
    {
        var supplementIntake = new SupplementIntake
        {
            UserId = CurrentUserId,
            SupplementFirebaseId = supplement?.FirebaseId ?? "",
            Dosage = supplementDosage,
            Date = supplementDay,
            Time = supplementTime,
        };
        
        var result = _validator.Validate(supplementIntake);

        if (!result.IsValid)
        {
            ValidationMessage = result.Errors[0].ErrorMessage;
            return;
        }

        ValidationMessage = "";
        try { await _supplementIntakeService.AddSupplementAsync(supplementIntake); }
        catch (Exception ex)
        {
            ReportCloudFailure(ex, "save the supplement schedule entry", canRetry: false);
            ValidationMessage = StatusMessage;
            return;
        }
        
        Supplement = null;
        SupplementDosage = 0;
        SupplementDay = DayOfWeek.Monday;
        SupplementTime = TimeSpan.Zero;

        await LoadSupplementIntakesAsync(CurrentUserId);
        IntakeSaved?.Invoke();
    }
    
    public async Task LoadSupplementIntakesAsync(string userId)
    {
        SetStatusMessage("Loading the supplement schedule from Firebase…");
        IsIntakeListEmpty = false;
        try
        {
            var list = await _supplementIntakeService.GetAllSupplementsByUserAsync(userId);
            SupplementIntakes.Clear();
            foreach (var intake in list)
            {
                intake.Supplement = Supplements.FirstOrDefault(s => s.FirebaseId == intake.SupplementFirebaseId);
                SupplementIntakes.Add(intake);
            }
            IsIntakeListEmpty = SupplementIntakes.Count == 0;
            ClearStatusMessage();
        }
        catch (Exception ex)
        {
            IsIntakeListEmpty = false;
            ReportCloudFailure(ex, "load the supplement schedule");
        }
    }

    public override async Task RetryAsync()
    {
        await LoadSupplementsAsync(CurrentUserId);
        await LoadSupplementIntakesAsync(CurrentUserId);
    }
    
    public async Task LoadSupplementsAsync(string userId)
    {
        SetStatusMessage("Loading supplements from Firebase…");
        try
        {
            var list = await _supplementService.GetAllSupplementsAsync(userId);
            Supplements.Clear();
            foreach (var supplement in list)
                Supplements.Add(supplement);
            ClearStatusMessage();
        }
        catch (Exception ex) { ReportCloudFailure(ex, "load supplements for the schedule"); }
    }

    [RelayCommand]
    private async Task DeleteSupplementIntakeAsync()
    {
        if (selectedSupplementIntake == null) 
            return;
        
        try { await _supplementIntakeService.DeleteSupplementAsync(selectedSupplementIntake); }
        catch (Exception ex)
        {
            ReportCloudFailure(ex, "delete the schedule entry", canRetry: false);
            ValidationMessage = StatusMessage;
            return;
        }
        await LoadSupplementIntakesAsync(CurrentUserId);
    }

    partial void OnSelectedSupplementIntakeChanged(SupplementIntake? value)
    {
        HasSelection = value != null;
        if (value == null)
            return;
        
        Supplement = Supplements.FirstOrDefault(e => e.FirebaseId == value.SupplementFirebaseId);
        SupplementDosage = value.Dosage;
        SupplementDay = value.Date;
        SupplementTime = value.Time;
    }

    public void ResetValidation()
    {
        validationMessage = "";
    }
}
