using Microsoft.EntityFrameworkCore;
using Uam.AdvancedProgramming.Api.Data;
using Uam.AdvancedProgramming.Api.Interfaces;

namespace Uam.AdvancedProgramming.Api.Repositories;

/// <summary>
/// Implementación base genérica para operaciones CRUD comunes.
/// </summary>
public class Repository<TEntity>(AppDbContext context) : IRepository<TEntity> where TEntity : class
{
    /// <summary>
    /// Contexto EF Core compartido para acceder a la base de datos.
    /// </summary>
    protected readonly AppDbContext Context = context;
    /// <summary>
    /// Set EF Core de la entidad para consultar y persistir datos.
    /// </summary>
    protected readonly DbSet<TEntity> Set = context.Set<TEntity>();

    /// <summary>
    /// Obtiene todos los registros en modo solo lectura.
    /// </summary>
    public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await Set.AsNoTracking().ToListAsync(cancellationToken);

    /// <summary>
    /// Obtiene una entidad por id.
    /// </summary>
    public virtual async Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await Set.FindAsync([id], cancellationToken);

    /// <summary>
    /// Agrega una entidad nueva al contexto.
    /// </summary>
    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default) =>
        await Set.AddAsync(entity, cancellationToken);

    /// <summary>
    /// Marca una entidad como modificada.
    /// </summary>
    public virtual void Update(TEntity entity) => Set.Update(entity);

    /// <summary>
    /// Marca una entidad para eliminación.
    /// </summary>
    public virtual void Remove(TEntity entity) => Set.Remove(entity);
}
