using System;
using System.Collections.Generic;

namespace GymApp.Models;

public class WorkoutProgram
{
    public string FirebaseId { get; set; } = "";
    public string UserId { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    
    public User? User { get; set; }
    public List<ProgramExercise> ProgramExercises { get; set; } = new();
}