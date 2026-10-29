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

public partial class WorkoutProgramViewModel : ViewModelBase
{
    private readonly WorkoutProgramService _programService;
    private readonly ExerciseService _exerciseService;

    public ObservableCollection<WorkoutProgram> WorkoutPrograms { get; } = new();
    public ObservableCollection<ProgramExercise> ProgramExercises { get; } = new();
    public ObservableCollection<Exercise> Exercises { get; } = new();

    [ObservableProperty]
    private bool isProgramExerciseListEmpty = true;

    [ObservableProperty]
    private bool hasSelectedProgram;
    [ObservableProperty]
    private bool hasSelectedProgramExercise;

    [ObservableProperty] 
    private string programValidationMessage = "";
    [ObservableProperty] 
    private string programName = "";
    [ObservableProperty] 
    private string programDescription = "";
    [ObservableProperty] 
    private WorkoutProgram? selectedProgram;

    private string? _editingProgramFirebaseId;
    
    public string CurrentUserId { get; set; } = "";
    public event Action? ProgramSaved;
    
    [ObservableProperty]
    private string programExerciseValidationMessage = "";
    [ObservableProperty] 
    private int exerciseSets;
    [ObservableProperty] 
    private int exerciseReps;
    [ObservableProperty] 
    private int exerciseRestTime;
    [ObservableProperty] 
    private Exercise? selectedExercise;
    [ObservableProperty] 
    private ProgramExercise? selectedProgramExercise;
    
    public event Action? ProgramExerciseSaved;

    public WorkoutProgramViewModel(WorkoutProgramService programService, ExerciseService exerciseService)
    {
        _programService = programService;
        _exerciseService = exerciseService;
    }

    private readonly ProgramValidator _programValidator = new();
    private readonly ProgramExerciseValidator _programExerciseValidator = new();

    [RelayCommand]
    private async Task SaveProgramAsync()
    {
        var program = new WorkoutProgram
        {
            FirebaseId = _editingProgramFirebaseId ?? "",
            UserId = CurrentUserId,
            Name = ProgramName,
            Description = ProgramDescription,
        };
            
        var programResult = _programValidator.Validate(program);
        if (!programResult.IsValid)
        {
            ProgramValidationMessage = programResult.Errors[0].ErrorMessage;
            return;
        }

        ProgramValidationMessage = "";
        try
        {
            if (_editingProgramFirebaseId == null)
                await _programService.AddProgramAsync(program);
            else
                await _programService.UpdateProgramAsync(program);
        }
        catch (Exception ex)
        {
            ReportCloudFailure(ex, "save the workout program", canRetry: false);
            ProgramValidationMessage = StatusMessage;
            return;
        }
        _editingProgramFirebaseId = null;

        ProgramName = "";
        ProgramDescription = "";

        await LoadProgramsAsync(CurrentUserId);
        ProgramSaved?.Invoke();
    }
    
    [RelayCommand]
    public async Task AddExerciseToProgramAsync()
    {
        if (SelectedProgram == null)
            return;
        
        if (SelectedExercise == null)
        {
            ProgramExerciseValidationMessage = "Exercise is required";
            return;
        }
        
        var programExercise = new ProgramExercise
        {
            ProgramFirebaseId = SelectedProgram.FirebaseId,
            ExerciseFirebaseId = SelectedExercise.FirebaseId,
            Sets = ExerciseSets,
            Reps = ExerciseReps,
            RestTime = ExerciseRestTime,
            OrderIndex = ProgramExercises.Count + 1
        };
        
        var programExerciseResult = _programExerciseValidator.Validate(programExercise);
            
        if (!programExerciseResult.IsValid)
        {
            ProgramExerciseValidationMessage = programExerciseResult.Errors[0].ErrorMessage;
            return;
        }

        ProgramExerciseValidationMessage = "";
        
        try { await _programService.AddExerciseToProgramAsync(programExercise, CurrentUserId); }
        catch (Exception ex)
        {
            ReportCloudFailure(ex, "add the exercise to this program", canRetry: false);
            ProgramExerciseValidationMessage = StatusMessage;
            return;
        }
        await LoadProgramExercisesAsync();
        
        SelectedExercise = null;
        ExerciseSets = 0;
        ExerciseReps = 0;
        ExerciseRestTime = 0;
        
        ProgramExerciseSaved?.Invoke();
    }

    public async Task LoadProgramsAsync(string userId)
    {
        SetStatusMessage("Loading workout programs from Firebase…");
        IsProgramExerciseListEmpty = false;
        try
        {
            var selectedProgramId = SelectedProgram?.FirebaseId;
            var programList = await _programService.GetAllProgramsAByUserAsync(userId);
            WorkoutPrograms.Clear();
            foreach (var program in programList)
            {
                foreach (var pe in program.ProgramExercises)
                    pe.Exercise = Exercises.FirstOrDefault(e => e.FirebaseId == pe.ExerciseFirebaseId);
                WorkoutPrograms.Add(program);
            }

            SelectedProgram = WorkoutPrograms.FirstOrDefault(program => program.FirebaseId == selectedProgramId)
                              ?? WorkoutPrograms.FirstOrDefault();
            IsProgramExerciseListEmpty = ProgramExercises.Count == 0;
            ClearStatusMessage();
        }
        catch (Exception ex)
        {
            IsProgramExerciseListEmpty = false;
            ReportCloudFailure(ex, "load workout programs");
        }
    }

    public override async Task RetryAsync()
    {
        await LoadExercisesAsync();
        await LoadProgramsAsync(CurrentUserId);
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
        catch (Exception ex) { ReportCloudFailure(ex, "load exercises for workout programs"); }
    }
    
    public async Task LoadProgramExercisesAsync()
    {
        if (SelectedProgram == null)
            return;

        SetStatusMessage("Loading program exercises from Firebase…");
        IsProgramExerciseListEmpty = false;
        try
        {
            var exercises = await _programService.GetExercisesByProgramAsync(SelectedProgram.FirebaseId, CurrentUserId);
            ProgramExercises.Clear();
            foreach (var exercise in exercises)
            {
                exercise.Exercise = Exercises.FirstOrDefault(e => e.FirebaseId == exercise.ExerciseFirebaseId);
                ProgramExercises.Add(exercise);
            }
            IsProgramExerciseListEmpty = ProgramExercises.Count == 0;
            ClearStatusMessage();
        }
        catch (Exception ex)
        {
            IsProgramExerciseListEmpty = false;
            ReportCloudFailure(ex, "load exercises in this program");
        }
    }
    
    [RelayCommand]
    private async Task DeleteProgramAsync()
    {
        if (SelectedProgram == null)
            return;

        try { await _programService.DeleteProgramAsync(SelectedProgram); }
        catch (Exception ex)
        {
            ReportCloudFailure(ex, "delete the workout program", canRetry: false);
            ProgramValidationMessage = StatusMessage;
            return;
        }
        
        ProgramExercises.Clear();
        SelectedProgram = null;
        ProgramName = "";
        ProgramDescription = "";
        _editingProgramFirebaseId = null;
        
        await LoadProgramsAsync(CurrentUserId);
    }

    [RelayCommand]
    private async Task DeleteExerciseFromProgramAsync()
    {
        if (SelectedProgramExercise == null)
            return;

        try { await _programService.DeleteExerciseFromProgramAsync(SelectedProgramExercise, CurrentUserId); }
        catch (Exception ex)
        {
            ReportCloudFailure(ex, "delete the exercise from this program", canRetry: false);
            ProgramExerciseValidationMessage = StatusMessage;
            return;
        }
        ProgramExercises.Remove(SelectedProgramExercise);
        IsProgramExerciseListEmpty = ProgramExercises.Count == 0;
    }

    partial void OnSelectedProgramChanged(WorkoutProgram? value)
    {
        ProgramExercises.Clear();
        IsProgramExerciseListEmpty = true;
        HasSelectedProgram = value != null;

        if (value == null)
            return;
        
        ProgramName = value.Name;
        ProgramDescription = value.Description;
        _editingProgramFirebaseId = value.FirebaseId;

        _ = LoadProgramExercisesAsync();
    }

    partial void OnSelectedProgramExerciseChanged(ProgramExercise? value)
    {
        HasSelectedProgramExercise = value != null;
    }
    
    public void ResetEditingState()
    {
        _editingProgramFirebaseId = null;
    }
    
    public void ResetProgramValidation()
    {
        ProgramValidationMessage = "";
    }
    
    public void ResetProgramExerciseValidation()
    {
        ProgramExerciseValidationMessage = "";
    }
}
