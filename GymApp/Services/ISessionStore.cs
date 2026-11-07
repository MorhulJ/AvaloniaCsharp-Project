namespace GymApp.Services;

public sealed record StoredSession(string RefreshToken, string Email);

public interface ISessionStore
{
    StoredSession? Load();
    void Save(StoredSession session);
    void Delete();
}
