using System.Collections.Generic;
using System.Threading.Tasks;
using GymApp.Data;
using GymApp.Models;

namespace GymApp.Services;

public class UserService
{
    private readonly AppDbContext _db;
    
    public UserService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _db.Users.FindAsync(id);
    }

    public async Task AddUserAsync(User user)
    {
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateUserAsync(User user)
    {
        var existing  = await _db.Users.FindAsync(user.Id);
        
        if (existing == null)
            throw new KeyNotFoundException($"User with Id={user.Id} is not found");
        
        existing.Name = user.Name;
        existing.Gender = user.Gender;
        existing.DateOfBirth = user.DateOfBirth;
        existing.weight = user.weight;
        existing.height = user.height;
        
        await _db.SaveChangesAsync();
    }
}