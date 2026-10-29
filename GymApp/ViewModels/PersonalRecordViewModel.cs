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
    private readonly ExerciseService _exerciseService;

    public ObservableCollection<PersonalRecord> Records { get; } = new();

    [ObservableProperty]
    private bool isRecordListEmpty = true;
    public ObservableCollection<Exercise> Exercises { get; } = new();

    [ObservableProperty] 
    private string validationMessage = "";
    [ObservableProperty]
    private Exercise? recordExercise;
    [ObservableProperty]
    private double recordValue;
    [ObservableProperty]
    private DateTimeOffset recordDate = DateTime.Today;
    [ObservableProperty]
    private PersonalRecord? selectedRecord;

    public string CurrentUserId { get; set; } = "";
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
            UserId = CurrentUserId,
            ExerciseFirebaseId = recordExercise?.FirebaseId ?? "",
            Value = recordValue,
            Date = recordDate,
        };
        
        var result = _validator.Validate(record);

        if (!result.IsValid)
        {
            ValidationMessage = result.Errors[0].ErrorMessage;
            return;
        }

        ValidationMessage = "";

        try { await _personalRecordService.AddRecordAsync(record); }
        catch (Exception ex)
        {
            ReportCloudFailure(ex, "save the personal record", canRetry: false);
            ValidationMessage = StatusMessage;
            return;
        }
        
        RecordExercise = null;
        RecordValue = 0;
        RecordDate = DateTime.Today;
        
        await LoadRecordsAsync(CurrentUserId);
        RecordSaved?.Invoke();
    }

    public async Task LoadRecordsAsync(string userId)
    {
        SetStatusMessage("Loading personal records from Firebase…");
        IsRecordListEmpty = false;
        try
        {
            var recordList = await _personalRecordService.GetAllRecordsByUserAsync(userId);
            Records.Clear();
            foreach (var record in recordList)
            {
                record.Exercise = Exercises.FirstOrDefault(e => e.FirebaseId == record.ExerciseFirebaseId);
                Records.Add(record);
            }
            IsRecordListEmpty = Records.Count == 0;
            ClearStatusMessage();
        }
        catch (Exception ex)
        {
            IsRecordListEmpty = false;
            ReportCloudFailure(ex, "load personal records");
        }
    }

    public override async Task RetryAsync()
    {
        await LoadExercisesAsync();
        await LoadRecordsAsync(CurrentUserId);
    }
    
    public async Task LoadExercisesAsync()
    {
        SetStatusMessage("Loading exercises from Firebase…");
        try
        {
            var exercisesList = await _exerciseService.GetAllExercisesAsync(CurrentUserId);
            Exercises.Clear();
            foreach (var exercise in exercisesList)
                Exercises.Add(exercise);
            ClearStatusMessage();
        }
        catch (Exception ex) { ReportCloudFailure(ex, "load exercises for personal records"); }
    }

    [RelayCommand]
    private async Task DeleteRecordAsync()
    {
        if (selectedRecord == null)
            return;
        
        try { await _personalRecordService.DeleteRecordAsync(selectedRecord); }
        catch (Exception ex)
        {
            ReportCloudFailure(ex, "delete the personal record", canRetry: false);
            ValidationMessage = StatusMessage;
            return;
        }
        await LoadRecordsAsync(CurrentUserId);
    }

    partial void OnSelectedRecordChanged(PersonalRecord? value)
    {
        HasSelection = value != null;
        if (value == null) return;
        
        RecordExercise = Exercises.FirstOrDefault(e => e.FirebaseId == value.ExerciseFirebaseId);
        RecordValue = value.Value;
        RecordDate = value.Date;
    }

    public void ResetValidation()
    {
        ValidationMessage = "";
    }
}
