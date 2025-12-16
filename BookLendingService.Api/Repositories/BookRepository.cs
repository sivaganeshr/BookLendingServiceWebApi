using BookLendingService.Api.Data;
using BookLendingService.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BookLendingService.Api.Repositories
{
    /// <summary>
    /// Provides methods for managing and accessing <see cref="Book"/> entities in the data store.
    /// </summary>
    /// <remarks>The <see cref="BookRepository"/> class implements the <see cref="IBookRepository"/> interface
    /// and enables asynchronous operations for retrieving, adding, and updating books. It is intended to be used as a
    /// data access layer component within applications that interact with a database context.</remarks>
    public class BookRepository: IBookRepository
    {
        private readonly AppDbContext _context;

        public BookRepository(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Asynchronously retrieves all books from the data store.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains an <see
        /// cref="IEnumerable{Book}"/> with all books in the data store. If no books exist, the collection is empty.</returns>
        public async Task<IEnumerable<Book>> GetAllAsync()
        {
            return await _context.Books.ToListAsync();
        }

        /// <summary>
        /// Asynchronously retrieves a book with the specified identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the book to retrieve. Must be a positive integer.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="Book"/> with the
        /// specified identifier, or <see langword="null"/> if no matching book is found.</returns>
        public async Task<Book?> GetByIdAsync(int id)
        {
            return await _context.Books.FindAsync(id);
        }

        /// <summary>
        /// Asynchronously adds a new book to the data store.
        /// </summary>
        /// <remarks>This method only adds the entity to the change tracker. Call <see cref="SaveChangesAsync"/> to persist changes.
        /// </remarks>
        public async Task AddAsync(Book book)
        {
            ArgumentNullException.ThrowIfNull(book);
            await _context.Books.AddAsync(book);
        }

        /// <summary>
        /// Updates the specified <see cref="Book"/> entity in the data store asynchronously.
        /// </summary>
        public Task UpdateAsync(Book book)
        {
            ArgumentNullException.ThrowIfNull(book);
            _context.Books.Update(book);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Persists all pending changes in the current context to the database.
        /// </summary>
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
