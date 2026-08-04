namespace GymApp.Models;

public class ProgramExercise
{
    public string FirebaseId { get; set; } = "";
    public string ProgramFirebaseId { get; set; } = "";
    public string ExerciseFirebaseId { get; set; } = "";
    public int Sets { get; set; }
    public int Reps { get; set; }
    public int OrderIndex { get; set; }
    public int RestTime { get; set; }

    public WorkoutProgram? Program { get; set; }
    public Exercise? Exercise { get; set; }
}