using Microsoft.EntityFrameworkCore;
using BusTicketReservation.Application.Interfaces;
using BusTicketReservation.Domain.Common;
using BusTicketReservation.Infrastructure.Data;

namespace BusTicketReservation.Infrastructure.Repositories;

public class EntityRepository<T> : IEntityRepository<T> where T : BaseEntity
{
    protected readonly BusTicketDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public EntityRepository(BusTicketDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = _context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        return entity;
    }

    public virtual async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await Task.CompletedTask;
    }

    public virtual async Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
        await Task.CompletedTask;
    }

    public virtual async Task<bool> ExistsAsync(Guid id)
    {
        return await _dbSet.AnyAsync(e => e.Id == id);
    }
}