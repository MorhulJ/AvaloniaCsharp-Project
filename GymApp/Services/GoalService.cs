using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GymApp.Data;
using GymApp.Models;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Services;

public class GoalService
{
    private readonly AppDbContext _db;
    
    public GoalService(AppDbContext db)
    {
        _db = db;
    }
    
    public async Task<Goal?> GetGoalByIdAsync(int id)
    {
        return await _db.Goals.FindAsync(id);
    }
    
    public async Task<List<Goal>> GetAllGoalsByUserAsync(int userId)
    {
        return await _db.Goals
            .Include(g => g.Exercise)
            .Where(g => g.UserId == userId)
            .ToListAsync();
    }

    public async Task AddGoalAsync(Goal goal)
    {
        _db.Goals.Add(goal);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteGoalAsync(Goal goal)
    {
        _db.Goals.Remove(goal);
        await _db.SaveChangesAsync();
    }
    
    
    public async Task UpdateGoalAsync(Goal goal)
    {
        var existing = await GetGoalByIdAsync(goal.Id);
        
        if (existing == null) 
            throw new KeyNotFoundException($"Goal with Id={goal.Id} is not found");
        
        existing.Title = goal.Title;
        existing.TargetValue = goal.TargetValue;
        existing.CurrentValue = goal.CurrentValue;
        existing.ExerciseId = goal.ExerciseId;
        
        await _db.SaveChangesAsync();
    }
}