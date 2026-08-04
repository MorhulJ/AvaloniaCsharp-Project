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
        await _db.Collection("users").Document(userId).DeleteAsync();
    }
}