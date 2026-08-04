using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using GymApp.Models;

namespace GymApp.Services;

public class GoalService
{
    private readonly FirestoreDb _db;

    public GoalService()
    {
        _db = FirebaseService.GetDb();
    }

    public async Task<List<Goal>> GetAllGoalsByUserAsync(string userId)
    {
        var snapshot = await _db.Collection("users").Document(userId)
            .Collection("goals").GetSnapshotAsync();

        var goals = new List<Goal>();
        foreach (var doc in snapshot.Documents)
        {
            goals.Add(new Goal
            {
                FirebaseId = doc.Id,
                UserId = userId,
                Title = doc.GetValue<string>("title"),
                TargetValue = doc.GetValue<double>("targetValue"),
                CurrentValue = doc.GetValue<double>("currentValue"),
                ExerciseFirebaseId = doc.ContainsField("exerciseId") ? doc.GetValue<string>("exerciseId") : null
            });
        }
        return goals;
    }

    public async Task AddGoalAsync(Goal goal)
    {
        await _db.Collection("users").Document(goal.UserId)
            .Collection("goals").AddAsync(new
            {
                title = goal.Title,
                targetValue = goal.TargetValue,
                currentValue = goal.CurrentValue,
                exerciseId = goal.ExerciseFirebaseId ?? ""
            });
    }

    public async Task UpdateGoalAsync(Goal goal)
    {
        await _db.Collection("users").Document(goal.UserId)
            .Collection("goals").Document(goal.FirebaseId)
            .UpdateAsync(new Dictionary<string, object>
            {
                { "title", goal.Title },
                { "targetValue", goal.TargetValue },
                { "currentValue", goal.CurrentValue },
                { "exerciseId", goal.ExerciseFirebaseId ?? "" }
            });
    }

    public async Task DeleteGoalAsync(Goal goal)
    {
        await _db.Collection("users").Document(goal.UserId)
            .Collection("goals").Document(goal.FirebaseId).DeleteAsync();
    }
}