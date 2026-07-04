using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymApp.Models;
using GymApp.Services;
using GymApp.Validators;

namespace GymApp.ViewModels;

public partial class PersonalRecordViewModel : ViewModelBase
{
    private readonly PersonalRecordService _personalRecordService;
    private readonly  ExerciseService _exerciseService;

    public ObservableCollection<PersonalRecord> Records { get; } = new();

    public ObservableCollection<Exercise> Exercises { get; } = new();

    [ObservableProperty] 
    private string validationMessage = "";
    
    [ObservableProperty]
    private Exercise? recordExercise;
    [ObservableProperty]
    private double recordValue;
    [ObservableProperty]
    private DateTimeOffset recordDate =  DateTime.Today;
    [ObservableProperty]
    private PersonalRecord? selectedRecord;

    public event Action? RecordSaved;

    public PersonalRecordViewModel(PersonalRecordService personalRecordService, ExerciseService exerciseService)
    {
        _personalRecordService = personalRecordService;
        _exerciseService = exerciseService;
    }

    private readonly RecordValidator _validator = new();

    [RelayCommand]
    private async Task SaveRecordAsync()
    {
        var record = new PersonalRecord
        {
            UserId = 1,
            ExerciseId = recordExercise?.Id ?? 0,
            Value =  recordValue,
            Date = recordDate,
        };
        
        var result = _validator.Validate(record);

        if (!result.IsValid)
        {
            ValidationMessage = result.Errors[0].ErrorMessage;
            return;
        }

        ValidationMessage = "";

        await _personalRecordService.AddRecordAsync(record);
        
        RecordExercise = null;
        RecordValue = 0;
        RecordDate = DateTime.Today;
        
        await LoadRecordsAsync();
        
        RecordSaved?.Invoke();
    }

    public async Task LoadRecordsAsync()
    {
        var recordList = await _personalRecordService.GetAllRecordsByUserAsync(1);
        
        Records.Clear();

        foreach (var record in recordList)
        {
            Records.Add(record);
        }
    }
    
    public async Task LoadExercisesAsync()
    {
        var exercisesList = await _exerciseService.GetAllExercisesAsync();
            
        Exercises.Clear();

        foreach (var exercise in exercisesList)
        {
            Exercises.Add(exercise);
        }
    }

    [RelayCommand]
    private async Task DeleteRecordAsync()
    {
        if (selectedRecord == null)
            return;
        
        await _personalRecordService.DeleteRecordAsync(selectedRecord);
        await LoadRecordsAsync();
    }

    partial void OnSelectedRecordChanged(PersonalRecord? value)
    {
        if (value == null) 
            return;
        
        RecordExercise =  Exercises.FirstOrDefault(e => e.Id == value.ExerciseId);
        RecordValue =  value.Value;
        RecordDate =  value.Date;
    }

    public void ResetValidation()
    {
        validationMessage = "";
    }
}