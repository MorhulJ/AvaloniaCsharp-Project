using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using GymApp.Models;

namespace GymApp.Services;

public class UserService
{
    public const string ProfileDataConsentVersion = "2026-10-29-v1";
    private static readonly string[] ProfileDataFields = { "gender", "weight", "height", "dateOfBirth" };
    private readonly FirestoreDb _db;

    public UserService()
    {
        _db = FirebaseService.GetDb();
    }

    public async Task<User?> GetUserByIdAsync(string userId)
    {
        var userDocument = _db.Collection("users").Document(userId);
        var doc = await userDocument.GetSnapshotAsync("name", "email", "profileDataConsent", "profileDataConsentVersion");
        if (!doc.Exists) return null;

        var hasCurrentConsent = doc.ContainsField("profileDataConsent")
            && doc.GetValue<bool>("profileDataConsent")
            && doc.ContainsField("profileDataConsentVersion")
            && doc.GetValue<string>("profileDataConsentVersion") == ProfileDataConsentVersion;

        var user = new User
        {
            FirebaseId = doc.Id,
            Name = doc.GetValue<string>("name"),
            Email = doc.ContainsField("email") ? doc.GetValue<string>("email") : "",
            ProfileDataConsent = hasCurrentConsent
        };

        if (!hasCurrentConsent) return user;

        // Fetch profile measurements only after a current consent has been recorded.
        // Read the document normally here: Firestore REST field masks have caused
        // profile retries to fail for otherwise valid, consented user documents.
        var profile = await userDocument.GetSnapshotAsync();
        user.Gender = profile.ContainsField("gender") ? profile.GetValue<string>("gender") : "";
        user.weight = profile.ContainsField("weight") ? profile.GetValue<int>("weight") : 0;
        user.height = profile.ContainsField("height") ? profile.GetValue<int>("height") : 0;
        user.DateOfBirth = profile.ContainsField("dateOfBirth")
            ? DateTime.ParseExact(profile.GetValue<string>("dateOfBirth"), "yyyy-MM-dd", CultureInfo.InvariantCulture)
            : DateTime.MinValue;
        return user;
    }

    public async Task UpdateUserAsync(User user)
    {
        var updates = new Dictionary<string, object> { { "name", user.Name } };
        var fieldsToDelete = new List<string>();
        if (user.ProfileDataConsent)
        {
            AddOptionalField(updates, fieldsToDelete, "gender", user.Gender);
            AddOptionalField(updates, fieldsToDelete, "weight", user.weight, user.weight != 0);
            AddOptionalField(updates, fieldsToDelete, "height", user.height, user.height != 0);
            AddOptionalField(updates, fieldsToDelete, "dateOfBirth",
                user.DateOfBirth.ToString("yyyy-MM-dd"), user.DateOfBirth != DateTime.MinValue);
        }

        var userDocument = _db.Collection("users").Document(user.FirebaseId);
        await userDocument.UpdateAsync(updates);
        if (fieldsToDelete.Count > 0)
            await userDocument.DeleteFieldsAsync(fieldsToDelete.ToArray());
    }

    private static void AddOptionalField(
        IDictionary<string, object> updates,
        ICollection<string> fieldsToDelete,
        string field,
        object? value,
        bool hasValue = true)
    {
        if (!hasValue || value is null || value is string text && string.IsNullOrWhiteSpace(text))
            fieldsToDelete.Add(field);
        else
            updates[field] = value;
    }

    public async Task GrantProfileDataConsentAsync(string userId)
    {
        await _db.Collection("users").Document(userId).UpdateAsync(new Dictionary<string, object>
        {
            { "profileDataConsent", true },
            { "profileDataConsentVersion", ProfileDataConsentVersion },
            { "profileDataConsentAtUtc", DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture) }
        });
    }

    public async Task RevokeProfileDataConsentAsync(string userId)
    {
        var userDocument = _db.Collection("users").Document(userId);
        await userDocument.UpdateAsync(new Dictionary<string, object>
        {
            { "profileDataConsent", false },
            { "profileDataConsentVersion", ProfileDataConsentVersion },
            { "profileDataConsentWithdrawnAtUtc", DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture) }
        });
        await userDocument.DeleteFieldsAsync(ProfileDataFields);
    }

    public async Task DeleteUnconsentedProfileDataAsync(string userId)
    {
        await _db.Collection("users").Document(userId).DeleteFieldsAsync(ProfileDataFields);
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
