using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymApp.Models;
using GymApp.Services;

namespace GymApp.ViewModels;

public partial class WorkoutProgramViewModel : ViewModelBase
{
    private readonly WorkoutProgramService _programService;
    private readonly ExerciseService _exerciseService;

    public ObservableCollection<WorkoutProgram> WorkoutPrograms { get; } = new();
    public ObservableCollection<ProgramExercise> ProgramExercises { get; } = new();
    public ObservableCollection<Exercise> Exercises { get; } = new();

    [ObservableProperty] 
    private string programName = "";
    [ObservableProperty] 
    private string programDescription = "";
    [ObservableProperty] 
    private WorkoutProgram? selectedProgram;

    private int? _editingProgramId;
    
    public event Action? ProgramSaved;

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

    [RelayCommand]
    private async Task SaveProgramAsync()
    {
        if (_editingProgramId == null)
        {
            var program = new WorkoutProgram
            {
                UserId = 1,
                Name = ProgramName,
                Description = ProgramDescription,
            };

            await _programService.AddProgramAsync(program);
        }
        else
        {
            var program = new WorkoutProgram
            {
                Id = _editingProgramId.Value,
                Name = ProgramName,
                Description = ProgramDescription,
            };

            await _programService.UpdateProgramAsync(program);

            _editingProgramId = null;
        }

        ProgramName = "";
        ProgramDescription = "";

        await LoadProgramsAsync();
        
        ProgramSaved?.Invoke();
    }
    
    [RelayCommand]
    public async Task AddExerciseToProgramAsync()
    {
        if (SelectedProgram == null || SelectedExercise == null)
            return;
        
        var programExercise = new ProgramExercise
        {
            ProgramId = SelectedProgram.Id,
            ExerciseId = SelectedExercise.Id,
            Sets = ExerciseSets,
            Reps = ExerciseReps,
            RestTime = ExerciseRestTime,
            OrderIndex = ProgramExercises.Count + 1
        };
        
        await _programService.AddExerciseToProgramAsync(programExercise);
        await LoadProgramExercisesAsync();
        
        SelectedExercise = null;
        ExerciseSets = 0;
        ExerciseReps = 0;
        ExerciseRestTime = 0;
        
        ProgramExerciseSaved?.Invoke();
    }

    public async Task LoadProgramsAsync()
    {
        var programList = await _programService.GetAllProgramsAByUserAsync(1);

        WorkoutPrograms.Clear();
        foreach (var program in programList)
        {
            WorkoutPrograms.Add(program);
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
    
    public async Task LoadProgramExercisesAsync()
    {
        if (SelectedProgram == null)
            return;

        var exercises = await _programService.GetExercisesByProgramAsync(SelectedProgram.Id);

        ProgramExercises.Clear();

        foreach (var exercise in exercises)
        {
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
        _editingProgramId = null;
        
        await LoadProgramsAsync();
    }

    [RelayCommand]
    private async Task DeleteExerciseFromProgramAsync()
    {
        if (SelectedProgramExercise == null)
            return;

        await _programService.DeleteExerciseFromProgramAsync(SelectedProgramExercise);
        ProgramExercises.Remove(SelectedProgramExercise);
    }

    partial void OnSelectedProgramChanged(WorkoutProgram? value)
    {
        if (value == null)
            return;
        
        ProgramName =  value.Name;
        ProgramDescription =  value.Description;
        
        _editingProgramId = value.Id;

        _ = LoadProgramExercisesAsync();
    }
    
    public void ResetEditingState()
    {
        _editingProgramId = null;
    }
}