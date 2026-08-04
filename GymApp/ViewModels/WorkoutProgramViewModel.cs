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
        if (_editingProgramFirebaseId == null)
        {
            var program = new WorkoutProgram
            {
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
            await _programService.AddProgramAsync(program);
        }
        else
        {
            var program = new WorkoutProgram
            {
                FirebaseId = _editingProgramFirebaseId,
                UserId = CurrentUserId,
                Name = ProgramName,
                Description = ProgramDescription,
            };

            await _programService.UpdateProgramAsync(program);
            _editingProgramFirebaseId = null;
        }

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
        
        await _programService.AddExerciseToProgramAsync(programExercise, CurrentUserId);
        await LoadProgramExercisesAsync();
        
        SelectedExercise = null;
        ExerciseSets = 0;
        ExerciseReps = 0;
        ExerciseRestTime = 0;
        
        ProgramExerciseSaved?.Invoke();
    }

    public async Task LoadProgramsAsync(string userId)
    {
        var programList = await _programService.GetAllProgramsAByUserAsync(userId);

        WorkoutPrograms.Clear();
        foreach (var program in programList)
        {
            foreach (var pe in program.ProgramExercises)
            {
                pe.Exercise = Exercises.FirstOrDefault(e => e.FirebaseId == pe.ExerciseFirebaseId);
            }
            WorkoutPrograms.Add(program);
        }
    }
    
    public async Task LoadExercisesAsync()
    {
        var exercisesList = await _exerciseService.GetAllExercisesAsync(CurrentUserId);

        Exercises.Clear();
        foreach (var exercise in exercisesList)
            Exercises.Add(exercise);
    }
    
    public async Task LoadProgramExercisesAsync()
    {
        if (SelectedProgram == null)
            return;

        var exercises = await _programService.GetExercisesByProgramAsync(SelectedProgram.FirebaseId, CurrentUserId);

        ProgramExercises.Clear();
        foreach (var exercise in exercises)
        {
            exercise.Exercise = Exercises.FirstOrDefault(e => e.FirebaseId == exercise.ExerciseFirebaseId);
            ProgramExercises.Add(exercise);
        }
    }
    
    [RelayCommand]
    private async Task DeleteProgramAsync()
    {
        if (SelectedProgram == null)
            return;

        await _programService.DeleteProgramAsync(SelectedProgram);
        
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

        await _programService.DeleteExerciseFromProgramAsync(SelectedProgramExercise, CurrentUserId);
        ProgramExercises.Remove(SelectedProgramExercise);
    }

    partial void OnSelectedProgramChanged(WorkoutProgram? value)
    {
        if (value == null)
            return;
        
        ProgramName = value.Name;
        ProgramDescription = value.Description;
        _editingProgramFirebaseId = value.FirebaseId;

        _ = LoadProgramExercisesAsync();
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