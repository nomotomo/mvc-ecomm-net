using System.Linq.Expressions;
using Ordering.Core.Common;

namespace Ordering.Core.Repositories;

public interface IAsyncRepository<T> where T : EntityBase
{
    /// <summary>
    /// Retrieves all entities of type T asynchronously.
    /// </summary>
    Task<IReadOnlyList<T>> GetAllAsync();

    /// <summary>
    /// Retrieves entities of type T that match the given predicate asynchronously.
    /// </summary>
    /// <param name="predicate">A LINQ expression to filter entities.</param>
    Task<IReadOnlyList<T>> GetAllAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Retrieves a single entity of type T by its unique identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    Task<T?> GetByIdAsync(Guid id);

    /// <summary>
    /// Adds a new entity of type T asynchronously and returns the added entity.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    Task<T> AddAsync(T entity);

    /// <summary>
    /// Updates an existing entity of type T asynchronously.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    Task UpdateAsync(T entity);

    /// <summary>
    /// Deletes an existing entity of type T asynchronously.
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    Task DeleteAsync(T entity);
}