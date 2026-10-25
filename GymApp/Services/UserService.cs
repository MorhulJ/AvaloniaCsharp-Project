using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using GymApp.Models;

namespace GymApp.Services;

public class UserService
{
    private readonly FirestoreDb _db;

    public UserService()
    {
        _db = FirebaseService.GetDb();
    }

    public async Task<User?> GetUserByIdAsync(string userId)
    {
        var doc = await _db.Collection("users").Document(userId).GetSnapshotAsync();
        if (!doc.Exists) return null;

        return new User
        {
            FirebaseId = doc.Id,
            Name = doc.GetValue<string>("name"),
            Email = doc.ContainsField("email") ? doc.GetValue<string>("email") : "",
            Gender = doc.GetValue<string>("gender"),
            weight = doc.ContainsField("weight") ? doc.GetValue<int>("weight") : 0,
            height = doc.ContainsField("height") ? doc.GetValue<int>("height") : 0,
            DateOfBirth = doc.ContainsField("dateOfBirth") 
                ? DateTime.Parse(doc.GetValue<string>("dateOfBirth")) 
                : DateTime.Now
        };
    }

    public async Task UpdateUserAsync(User user)
    {
        await _db.Collection("users").Document(user.FirebaseId)
            .UpdateAsync(new Dictionary<string, object>
            {
                { "name", user.Name },
                { "gender", user.Gender },
                { "weight", user.weight },
                { "height", user.height },
                { "dateOfBirth", user.DateOfBirth.ToString("yyyy-MM-dd") }
            });
    }

    public async Task DeleteUserAsync(string userId)
    {
        var userDocument = _db.Collection("users").Document(userId);

        // These are the collections currently used by GymApp. listCollectionIds is
        // a database-metadata endpoint and is not reliable with user-scoped rules;
        // list and delete each known collection through normal document requests.
        await DeleteCollectionAsync(userDocument.Collection("exercises"));
        await DeleteCollectionAsync(userDocument.Collection("goals"));
        await DeleteCollectionAsync(userDocument.Collection("personalRecords"));
        await DeleteCollectionAsync(userDocument.Collection("supplements"));
        await DeleteCollectionAsync(userDocument.Collection("supplementIntakes"));
        await DeleteCollectionAsync(userDocument.Collection("workoutPrograms"), deleteProgramExercises: true);

        await userDocument.DeleteAsync();
    }

    private static async Task DeleteCollectionAsync(CollectionReference collection, bool deleteProgramExercises = false)
    {
        var snapshot = await collection.GetSnapshotAsync();
        foreach (var item in snapshot.Documents)
        {
            var document = collection.Document(item.Id);
            if (deleteProgramExercises)
                await DeleteCollectionAsync(document.Collection("exercises"));
            await document.DeleteAsync();
        }
    }
}
