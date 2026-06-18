using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GymApp.Data;
using GymApp.Models;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Services;

public class WorkoutProgramService
{
    private readonly AppDbContext _db;
    
    public  WorkoutProgramService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<WorkoutProgram?> GetProgramByIdAsync(int Id)
    {
        return await _db.WorkoutPrograms.FindAsync(Id);
    }

    public async Task<List<WorkoutProgram>> GetAllProgramsAByUserAsync(int userId)
    {
        return await _db.WorkoutPrograms
            .Include(wp => wp.ProgramExercises)
            .ThenInclude(wp => wp.Exercise)
            .Where(wp => wp.UserId == userId)
            .ToListAsync();
    }
    
    public async Task<List<ProgramExercise>> GetExercisesByProgramAsync(int programId)
    {
        return await _db.WorkoutExercises
            .Include(x => x.Exercise)
            .Where(x => x.ProgramId == programId)
            .OrderBy(x => x.OrderIndex)
            .ToListAsync();
    }

    public async Task AddProgramAsync(WorkoutProgram program)
    {
        _db.WorkoutPrograms.Add(program);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteProgramAsync(WorkoutProgram program)
    {
        _db.WorkoutPrograms.Remove(program);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateProgramAsync(WorkoutProgram program)
    {
        var existing = await _db.WorkoutPrograms.FindAsync(program.Id);
        
        if  (existing == null)
            throw new KeyNotFoundException($"Program with id={program.Id} is not found");
        
        existing.Name = program.Name;
        existing.Description = program.Description;
        
        await _db.SaveChangesAsync();
    }
    
    public async Task AddExerciseToProgramAsync(ProgramExercise programExercise)
    {
        _db.WorkoutExercises.Add(programExercise);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteExerciseFromProgramAsync(ProgramExercise programExercise)
    {
        _db.WorkoutExercises.Remove(programExercise);
        await _db.SaveChangesAsync();
    }
}