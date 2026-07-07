using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using GymApp.Data;
using GymApp.Models;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Services;

public class AuthService
{
    private readonly AppDbContext _db;
    private static User? _currentUser;

    public AuthService(AppDbContext db)
    {
        _db = db;
    }

    public User? CurrentUser => _currentUser;

    public string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes);
    }

    public async Task<User?> LoginAsync(string login, string password)
    {
        var hash = HashPassword(password);
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Login == login && u.PasswordHash == hash);

        if (user != null)
            _currentUser = user;

        return user;
    }

    public async Task<bool> RegisterAsync(string login, string password, string name)
    {
        var exists = await _db.Users.AnyAsync(u => u.Login == login);
        if (exists) return false;

        var user = new User
        {
            Login = login,
            PasswordHash = HashPassword(password),
            Name = name,
            DateOfBirth = DateTime.Now,
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        _currentUser = user;
        return true;
    }

    public void Logout()
    {
        _currentUser = null;
        DeleteRememberMe();
    }

    public void SaveRememberMe(int userId)
    {
        var path = GetRememberMePath();
        File.WriteAllText(path, userId.ToString());
    }

    public async Task<User?> TryAutoLoginAsync()
    {
        var path = GetRememberMePath();
        if (!File.Exists(path)) return null;

        var content = await File.ReadAllTextAsync(path);
        if (!int.TryParse(content, out var userId)) return null;

        var user = await _db.Users.FindAsync(userId);
        if (user != null)
            _currentUser = user;

        return user;
    }

    public void DeleteRememberMe()
    {
        var path = GetRememberMePath();
        if (File.Exists(path))
            File.Delete(path);
    }

    private string GetRememberMePath()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "GymApp",
            "remember.txt"
        );
    }
}