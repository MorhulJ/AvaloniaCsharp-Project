using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using GymApp.Models;

namespace GymApp.Services;

public class SupplementIntakeService
{
    private readonly FirestoreDb _db;

    public SupplementIntakeService()
    {
        _db = FirebaseService.GetDb();
    }

    public async Task<List<SupplementIntake>> GetAllSupplementsByUserAsync(string userId)
    {
        var snapshot = await _db.Collection("users").Document(userId)
            .Collection("supplementIntakes").GetSnapshotAsync();

        var intakes = new List<SupplementIntake>();
        foreach (var doc in snapshot.Documents)
        {
            intakes.Add(new SupplementIntake
            {
                FirebaseId = doc.Id,
                UserId = userId,
                SupplementFirebaseId = doc.GetValue<string>("supplementId"),
                Dosage = doc.GetValue<double>("dosage"),
                Date = (DayOfWeek)doc.GetValue<int>("day"),
                Time = TimeSpan.Parse(doc.GetValue<string>("time"))
            });
        }
        return intakes;
    }

    public async Task AddSupplementAsync(SupplementIntake intake)
    {
        await _db.Collection("users").Document(intake.UserId)
            .Collection("supplementIntakes").AddAsync(new
            {
                supplementId = intake.SupplementFirebaseId,
                dosage = intake.Dosage,
                day = (int)intake.Date,
                time = intake.Time.ToString()
            });
    }

    public async Task DeleteSupplementAsync(SupplementIntake intake)
    {
        await _db.Collection("users").Document(intake.UserId)
            .Collection("supplementIntakes").Document(intake.FirebaseId).DeleteAsync();
    }
}