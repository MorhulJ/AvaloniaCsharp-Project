using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using GymApp.Models;

namespace GymApp.Services;

public class PersonalRecordService
{
    private readonly FirestoreDb _db;

    public PersonalRecordService()
    {
        _db = FirebaseService.GetDb();
    }

    public async Task<List<PersonalRecord>> GetAllRecordsByUserAsync(string userId)
    {
        var snapshot = await _db.Collection("users").Document(userId)
            .Collection("personalRecords").GetSnapshotAsync();

        var records = new List<PersonalRecord>();
        foreach (var doc in snapshot.Documents)
        {
            records.Add(new PersonalRecord
            {
                FirebaseId = doc.Id,
                UserId = userId,
                ExerciseFirebaseId = doc.GetValue<string>("exerciseId"),
                Value = doc.GetValue<double>("value"),
                Date = DateTimeOffset.Parse(doc.GetValue<string>("date"))
            });
        }
        return records;
    }

    public async Task AddRecordAsync(PersonalRecord record)
    {
        await _db.Collection("users").Document(record.UserId)
            .Collection("personalRecords").AddAsync(new
            {
                exerciseId = record.ExerciseFirebaseId,
                value = record.Value,
                date = record.Date.ToString("O")
            });
    }

    public async Task DeleteRecordAsync(PersonalRecord record)
    {
        await _db.Collection("users").Document(record.UserId)
            .Collection("personalRecords").Document(record.FirebaseId).DeleteAsync();
    }
}