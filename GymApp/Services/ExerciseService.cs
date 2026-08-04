using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using GymApp.Models;

namespace GymApp.Services;

public class ExerciseService
{
    private readonly FirestoreDb _db;

    public ExerciseService()
    {
        _db = FirebaseService.GetDb();
    }

    public async Task<List<Exercise>> GetAllExercisesAsync(string userId)
    {
        var snapshot = await _db.Collection("users").Document(userId)
            .Collection("exercises").GetSnapshotAsync();
    
        var exercises = new List<Exercise>();
        foreach (var doc in snapshot.Documents)
        {
            exercises.Add(new Exercise
            {
                FirebaseId = doc.Id,
                Name = doc.GetValue<string>("name"),
                MuscleGroup = doc.GetValue<string>("muscleGroup"),
                Description = doc.GetValue<string>("description")
            });
        }
        return exercises;
    }

    public async Task AddExerciseAsync(Exercise exercise, string userId)
    {
        await _db.Collection("users").Document(userId)
            .Collection("exercises").AddAsync(new
            {
                name = exercise.Name,
                muscleGroup = exercise.MuscleGroup,
                description = exercise.Description
            });
    }

    public async Task UpdateExerciseAsync(Exercise exercise, string userId)
    {
        await _db.Collection("users").Document(userId)
            .Collection("exercises").Document(exercise.FirebaseId)
            .UpdateAsync(new Dictionary<string, object>
            {
                { "name", exercise.Name },
                { "muscleGroup", exercise.MuscleGroup },
                { "description", exercise.Description }
            });
    }

    public async Task DeleteExerciseAsync(Exercise exercise, string userId)
    {
        await _db.Collection("users").Document(userId)
            .Collection("exercises").Document(exercise.FirebaseId).DeleteAsync();
    }
}