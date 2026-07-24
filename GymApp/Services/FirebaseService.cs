using System;
using System.IO;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;

namespace GymApp.Services;

public class FirebaseService
{
    private static FirestoreDb? _db;
    private static bool _initialized = false;

    public static FirestoreDb Initialize()
    {
        if (_initialized && _db != null)
            return _db;

        var credentialsPath = Path.Combine(AppContext.BaseDirectory, "firebase-credentials.json");

        Environment.SetEnvironmentVariable(
            "GOOGLE_APPLICATION_CREDENTIALS",
            credentialsPath
        );

        FirebaseApp.Create(new AppOptions
        {
            Credential = GoogleCredential.FromFile(credentialsPath)
        });

        _db = FirestoreDb.Create("gymapp-9c7e4");
        _initialized = true;

        return _db;
    }

    public static FirestoreDb GetDb()
    {
        if (_db == null)
            throw new InvalidOperationException("Firebase not initialized");
        return _db;
    }
}