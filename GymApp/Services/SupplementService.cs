using System.Collections.Generic;
using System.Threading.Tasks;
using GymApp.Data;
using GymApp.Models;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Services;

public class SupplementService
{
    private readonly AppDbContext _db;
    
    public SupplementService(AppDbContext db)
    {
        _db = db;
    }
    
    public async Task<Supplement?> GetSupplementByIdAsync(int id)
    {
        return await _db.Supplements.FindAsync(id);
    }

    public async Task<List<Supplement>> GetAllSupplementsAsync()
    {
        return await _db.Supplements.ToListAsync();
    }

    public async Task AddSupplementAsync(Supplement suplement)
    {
        _db.Supplements.Add(suplement);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteSupplementAsync(Supplement suplement)
    {
        _db.Supplements.Remove(suplement);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateSupplementAsync(Supplement suplement)
    {
        var existing =  await _db.Supplements.FindAsync(suplement.Id);
        
        if (existing == null)
            throw new KeyNotFoundException($"Supplement with Id={suplement.Id} is not found");
        
        existing.Name = suplement.Name;
        existing.DosageUnit = suplement.DosageUnit;
        existing.Description = suplement.Description;
        
        await _db.SaveChangesAsync();
    }
}