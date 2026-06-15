using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GymApp.Data;
using GymApp.Models;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Services;

public class PersonalRecordService
{
    private readonly AppDbContext _db;

    public PersonalRecordService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<PersonalRecord>> GetAllRecordsByUserAsync(int userId)
    {
        return await _db.PersonalRecords
            .Include(pr => pr.Exercise)
            .Where(pr => pr.UserId == userId)
            .ToListAsync();
    }

    public async Task AddRecordAsync(PersonalRecord record)
    {
        _db.PersonalRecords.Add(record);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteRecordAsync(PersonalRecord record)
    {
        _db.PersonalRecords.Remove(record);
        await _db.SaveChangesAsync();
    }
}