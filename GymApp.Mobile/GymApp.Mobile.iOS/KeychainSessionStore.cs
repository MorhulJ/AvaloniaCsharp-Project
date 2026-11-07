using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Foundation;
using GymApp.Services;
using Security;

namespace GymApp.Mobile.iOS;

internal sealed class KeychainSessionStore : ISessionStore
{
    private static SecRecord CreateQuery() => new(SecKind.GenericPassword)
    {
        Service = "com.morhulj.gymapp.firebase",
        Account = "remembered-session"
    };

    public StoredSession? Load()
    {
        using var query = CreateQuery();
        using var record = SecKeyChain.QueryAsRecord(query, out var status);
        if (status == SecStatusCode.ItemNotFound) return null;
        CheckStatus(status);
        using var data = record?.ValueData;
        return data is null ? null : JsonSerializer.Deserialize(data.ToArray(), SessionJsonContext.Default.StoredSession);
    }

    public void Save(StoredSession session)
    {
        using var query = CreateQuery();
        using var data = NSData.FromArray(JsonSerializer.SerializeToUtf8Bytes(session, SessionJsonContext.Default.StoredSession));
        using var update = new SecRecord()
        {
            ValueData = data,
            Accessible = SecAccessible.AfterFirstUnlockThisDeviceOnly
        };
        var status = SecKeyChain.Update(query, update);
        if (status == SecStatusCode.ItemNotFound)
        {
            query.ValueData = data;
            query.Accessible = SecAccessible.AfterFirstUnlockThisDeviceOnly;
            status = SecKeyChain.Add(query);
        }
        CheckStatus(status);
    }

    public void Delete()
    {
        using var query = CreateQuery();
        var status = SecKeyChain.Remove(query);
        if (status != SecStatusCode.ItemNotFound) CheckStatus(status);
    }

    private static void CheckStatus(SecStatusCode status)
    {
        if (status != SecStatusCode.Success)
            throw new InvalidOperationException($"Keychain operation failed ({status}).");
    }
}

[JsonSerializable(typeof(StoredSession))]
internal partial class SessionJsonContext : JsonSerializerContext
{
}
