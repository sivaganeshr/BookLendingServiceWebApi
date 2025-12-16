using BookLendingService.Api.Models;

namespace BookLendingService.Api.Repositories
{
    /// <summary>
    /// Defines data access operations for <see cref="Book"/> entities.
    /// </summary>
    public interface IBookRepository
    {
        /// <summary>
        /// Retrieves all books from the data store asynchronously.
        /// </summary>
        /// <returns>A collection of all books.</returns>
        Task<IEnumerable<Book>> GetAllAsync();

        /// <summary>
        /// Retrieves a book by its unique identifier asynchronously.
        /// </summary>
        /// <param name="id">The unique identifier of the book.</param>
        /// <returns>The book if found; otherwise, <c>null</c>.</returns>
        Task<Book?> GetByIdAsync(int id);

        /// <summary>
        /// Adds a new book entity to the data store.
        /// </summary>
        /// <param name="book">The book entity to add.</param>
        Task AddAsync(Book book);

        /// <summary>
        /// Updates an existing book entity in the data store.
        /// </summary>
        /// <param name="book">The book entity to update.</param>
        Task UpdateAsync(Book book);

        /// <summary>
        /// Persists all pending changes to the underlying data store.
        /// </summary>
        Task SaveChangesAsync();
    }
}
