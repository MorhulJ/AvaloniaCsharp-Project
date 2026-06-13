using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GymApp.Data;
using GymApp.Models;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Services;

public class ExerciseService
{
    private readonly AppDbContext _db;
    
    public ExerciseService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Exercise?> GetExerciseByIdAsync(int id)
    {
        return await _db.Exercises.FindAsync(id);
    }
    
    public async Task<List<Exercise>> GetAllExercisesAsync()
    {
        return await _db.Exercises.ToListAsync();
    }

    public async Task AddExerciseAsync(Exercise exercise)
    {
        _db.Exercises.Add(exercise);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteExerciseAsync(Exercise exercise)
    {
        _db.Exercises.Remove(exercise);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateExerciseAsync(Exercise exercise)
    {
        _db.Exercises.Update(exercise);
        await _db.SaveChangesAsync();
    }
}