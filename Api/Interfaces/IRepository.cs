namespace Uam.AdvancedProgramming.Api.Interfaces;

// Contrato genérico base para operaciones comunes de cualquier entidad.
/// <summary>
/// Contrato genérico para operaciones CRUD básicas sobre una entidad.
/// </summary>
public interface IRepository<TEntity> where TEntity : class
{
    // Obtiene todos los registros de la entidad.
    /// <summary>
    /// Obtiene todos los registros de la entidad.
    /// </summary>
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    // Obtiene un registro por su id.
    /// <summary>
    /// Obtiene una entidad por su identificador.
    /// </summary>
    Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    // Agrega un nuevo registro al contexto.
    /// <summary>
    /// Agrega una nueva entidad al contexto.
    /// </summary>
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    // Marca un registro como actualizado.
    /// <summary>
    /// Marca la entidad como modificada.
    /// </summary>
    void Update(TEntity entity);

    // Marca un registro para eliminación.
    /// <summary>
    /// Marca la entidad para eliminación.
    /// </summary>
    void Remove(TEntity entity);
}
