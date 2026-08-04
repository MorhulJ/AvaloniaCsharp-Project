using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using GymApp.Models;

namespace GymApp.Services;

public class SupplementService
{
    private readonly FirestoreDb _db;

    public SupplementService()
    {
        _db = FirebaseService.GetDb();
    }

    public async Task<List<Supplement>> GetAllSupplementsAsync(string userId)
    {
        var snapshot = await _db.Collection("users").Document(userId)
            .Collection("supplements").GetSnapshotAsync();

        var supplements = new List<Supplement>();
        foreach (var doc in snapshot.Documents)
        {
            supplements.Add(new Supplement
            {
                FirebaseId = doc.Id,
                UserId = userId,
                Name = doc.GetValue<string>("name"),
                DosageUnit = doc.GetValue<string>("dosageUnit"),
                Description = doc.GetValue<string>("description")
            });
        }
        return supplements;
    }

    public async Task AddSupplementAsync(Supplement supplement)
    {
        await _db.Collection("users").Document(supplement.UserId)
            .Collection("supplements").AddAsync(new
            {
                name = supplement.Name,
                dosageUnit = supplement.DosageUnit,
                description = supplement.Description
            });
    }

    public async Task UpdateSupplementAsync(Supplement supplement)
    {
        await _db.Collection("users").Document(supplement.UserId)
            .Collection("supplements").Document(supplement.FirebaseId)
            .UpdateAsync(new Dictionary<string, object>
            {
                { "name", supplement.Name },
                { "dosageUnit", supplement.DosageUnit },
                { "description", supplement.Description }
            });
    }

    public async Task DeleteSupplementAsync(Supplement supplement)
    {
        await _db.Collection("users").Document(supplement.UserId)
            .Collection("supplements").Document(supplement.FirebaseId).DeleteAsync();
    }
}