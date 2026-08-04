using System;
using System.IO;
using FirebaseAdmin;
using Firebase.Auth;
using Firebase.Auth.Providers;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;

namespace GymApp.Services;

public class FirebaseService
{
    private static FirestoreDb? _db;
    private static FirebaseAuthClient? _authClient;
    private static bool _initialized = false;

    public static void Initialize()
    {
        if (_initialized) return;

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

        var config = new FirebaseAuthConfig
        {
            ApiKey = "AIzaSyCk8jFOyXPO92qmvz_KDATgEr5jVj3u1Jw",
            AuthDomain = "gymapp-9c7e4.firebaseapp.com",
            Providers = new FirebaseAuthProvider[]
            {
                new EmailProvider()
            }
        };

        _authClient = new FirebaseAuthClient(config);
        _initialized = true;
    }

    public static FirestoreDb GetDb()
    {
        if (_db == null)
            throw new InvalidOperationException("Firebase not initialized");
        return _db;
    }

    public static FirebaseAuthClient GetAuth()
    {
        if (_authClient == null)
            throw new InvalidOperationException("Firebase not initialized");
        return _authClient;
    }
}