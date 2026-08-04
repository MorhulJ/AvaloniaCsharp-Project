using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using GymApp.Models;

namespace GymApp.Services;

public class WorkoutProgramService
{
    private readonly FirestoreDb _db;

    public WorkoutProgramService()
    {
        _db = FirebaseService.GetDb();
    }

    public async Task<List<WorkoutProgram>> GetAllProgramsAByUserAsync(string userId)
    {
        var snapshot = await _db.Collection("users").Document(userId)
            .Collection("workoutPrograms").GetSnapshotAsync();

        var programs = new List<WorkoutProgram>();
        foreach (var doc in snapshot.Documents)
        {
            var program = new WorkoutProgram
            {
                FirebaseId = doc.Id,
                UserId = userId,
                Name = doc.GetValue<string>("name"),
                Description = doc.GetValue<string>("description")
            };

            var exercisesSnapshot = await _db.Collection("users").Document(userId)
                .Collection("workoutPrograms").Document(doc.Id)
                .Collection("exercises").GetSnapshotAsync();

            foreach (var exDoc in exercisesSnapshot.Documents)
            {
                program.ProgramExercises.Add(new ProgramExercise
                {
                    FirebaseId = exDoc.Id,
                    ProgramFirebaseId = doc.Id,
                    ExerciseFirebaseId = exDoc.GetValue<string>("exerciseId"),
                    Sets = exDoc.GetValue<int>("sets"),
                    Reps = exDoc.GetValue<int>("reps"),
                    RestTime = exDoc.GetValue<int>("restTime"),
                    OrderIndex = exDoc.GetValue<int>("orderIndex")
                });
            }

            programs.Add(program);
        }
        return programs;
    }

    public async Task<List<ProgramExercise>> GetExercisesByProgramAsync(string programFirebaseId, string userId)
    {
        var snapshot = await _db.Collection("users").Document(userId)
            .Collection("workoutPrograms").Document(programFirebaseId)
            .Collection("exercises").GetSnapshotAsync();

        var exercises = new List<ProgramExercise>();
        foreach (var doc in snapshot.Documents)
        {
            exercises.Add(new ProgramExercise
            {
                FirebaseId = doc.Id,
                ProgramFirebaseId = programFirebaseId,
                ExerciseFirebaseId = doc.GetValue<string>("exerciseId"),
                Sets = doc.GetValue<int>("sets"),
                Reps = doc.GetValue<int>("reps"),
                RestTime = doc.GetValue<int>("restTime"),
                OrderIndex = doc.GetValue<int>("orderIndex")
            });
        }
        return exercises;
    }

    public async Task AddProgramAsync(WorkoutProgram program)
    {
        await _db.Collection("users").Document(program.UserId)
            .Collection("workoutPrograms").AddAsync(new
            {
                name = program.Name,
                description = program.Description,
                createdDate = DateTime.Now.ToString("yyyy-MM-dd")
            });
    }

    public async Task UpdateProgramAsync(WorkoutProgram program)
    {
        await _db.Collection("users").Document(program.UserId)
            .Collection("workoutPrograms").Document(program.FirebaseId)
            .UpdateAsync(new Dictionary<string, object>
            {
                { "name", program.Name },
                { "description", program.Description }
            });
    }

    public async Task DeleteProgramAsync(WorkoutProgram program)
    {
        await _db.Collection("users").Document(program.UserId)
            .Collection("workoutPrograms").Document(program.FirebaseId).DeleteAsync();
    }

    public async Task AddExerciseToProgramAsync(ProgramExercise pe, string userId)
    {
        await _db.Collection("users").Document(userId)
            .Collection("workoutPrograms").Document(pe.ProgramFirebaseId)
            .Collection("exercises").AddAsync(new
            {
                exerciseId = pe.ExerciseFirebaseId,
                sets = pe.Sets,
                reps = pe.Reps,
                restTime = pe.RestTime,
                orderIndex = pe.OrderIndex
            });
    }

    public async Task DeleteExerciseFromProgramAsync(ProgramExercise pe, string userId)
    {
        await _db.Collection("users").Document(userId)
            .Collection("workoutPrograms").Document(pe.ProgramFirebaseId)
            .Collection("exercises").Document(pe.FirebaseId).DeleteAsync();
    }
}