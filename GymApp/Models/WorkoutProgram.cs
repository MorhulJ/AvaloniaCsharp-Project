using System;
using System.Collections.Generic;

namespace GymApp.Models;

public class WorkoutProgram
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public DateTime CreatedDate { get; set; } =  DateTime.Now;
    
    public User User { get; set; }
    public List<ProgramExercise> ProgramExercises { get; set; } = new();
}