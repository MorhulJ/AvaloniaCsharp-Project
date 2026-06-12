using System;
using GymApp.Models;

public class ProgramExercise
{
    public int Id { get; set; }
    public int ProgramId { get; set; }
    public int ExerciseId { get; set; }
    public int Sets { get; set; }
    public int Reps { get; set; }
    public int OrderIndex { get; set; }
    public int RestTime { get; set; }

    public WorkoutProgram Program { get; set; }
    public Exercise Exercise { get; set; }
}