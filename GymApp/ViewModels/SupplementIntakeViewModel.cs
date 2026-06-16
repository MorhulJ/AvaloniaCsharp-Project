using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymApp.Models;
using GymApp.Services;

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

    public ObservableCollection<Supplement> Supplements { get; } = new();
    
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

    public SupplementIntakeViewModel(SupplementIntakeService supplementIntakeService, SupplementService supplementService)
    {
        _supplementIntakeService = supplementIntakeService;
        _supplementService = supplementService;
    }

    [RelayCommand]
    private async Task SaveSupplementIntakeAsync()
    {
        var supplementIntake = new SupplementIntake
        {
            UserId = 1,
            SupplementId = supplement?.Id ?? 0,
            Dosage = supplementDosage,
            Date = supplementDay,
            Time = supplementTime,
        };
        
        await _supplementIntakeService.AddSupplementAsync(supplementIntake);
        
        Supplement = null;
        SupplementDosage = 0;
        SupplementDay = DayOfWeek.Monday;
        SupplementTime = TimeSpan.Zero;

        await LoadSupplementIntakesAsync();
    }
    
    public async Task LoadSupplementIntakesAsync()
    {
        var supplementIntakesList = await _supplementIntakeService.GetAllSupplementsByUserAsync(1);
        
        SupplementIntakes.Clear();

        foreach (var supplementIntake in supplementIntakesList)
        {
            SupplementIntakes.Add(supplementIntake);
        }
    }
    
    public async Task LoadSupplementsAsync()
    {
        var supplementsList = await _supplementService.GetAllSupplementsAsync();
        
        Supplements.Clear();

        foreach (var supplement in supplementsList)
        {
            Supplements.Add(supplement);
        }
    }

    [RelayCommand]
    private async Task DeleteSupplementAsync()
    {
        if (selectedSupplementIntake == null) 
            return;
        
        await _supplementIntakeService.DeleteSupplementAsync(selectedSupplementIntake);
        await LoadSupplementIntakesAsync();
    }

    partial void OnSelectedSupplementIntakeChanged(SupplementIntake? value)
    {
        if (value == null)
            return;
        
        Supplement = Supplements.FirstOrDefault(e => e.Id == value.SupplementId);
        SupplementDosage = value.Dosage;
        SupplementDay = value.Date;
        SupplementTime =  value.Time;
    }
}