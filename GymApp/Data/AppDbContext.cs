using Microsoft.EntityFrameworkCore;
using GymApp.Models;

namespace GymApp.Data;

public class AppDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Exercise> Exercises => Set<Exercise>();
    public DbSet<WorkoutProgram> WorkoutPrograms => Set<WorkoutProgram>();
    public DbSet<ProgramExercise> WorkoutExercises => Set<ProgramExercise>();
    public DbSet<Goal> Goals => Set<Goal>();
    public DbSet<PersonalRecord> PersonalRecords => Set<PersonalRecord>();
    public DbSet<Supplement> Supplements => Set<Supplement>();
    public DbSet<SupplementIntake> SupplementIntakes => Set<SupplementIntake>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=gym.db");
    }
}