using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GymApp.Data;
using GymApp.Models;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Services;

public class SupplementIntakeService
{
    private readonly AppDbContext _db;
    
    public SupplementIntakeService(AppDbContext db)
    {
        _db = db;
    }
    
    public async Task<List<SupplementIntake>> GetAllSupplementsByUserAsync(int userId)
    {
        return await _db.SupplementIntakes
            .Include(si => si.Supplement)
            .Where(si => si.UserId == userId)
            .ToListAsync();
    }

    public async Task AddSupplementAsync(SupplementIntake supplement)
    {
        _db.SupplementIntakes.Add(supplement);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteSupplementAsync(SupplementIntake supplement)
    {
        _db.SupplementIntakes.Remove(supplement);
        await _db.SaveChangesAsync();
    }
}