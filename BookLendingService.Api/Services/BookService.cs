using BookLendingService.Api.DTOs;
using BookLendingService.Api.Models;
using BookLendingService.Api.Repositories;

namespace BookLendingService.Api.Services
{
    /// <summary>
    /// Provides operations for managing books, including retrieval, creation, checkout, and return functionality.
    /// </summary>
    /// <remarks>The <see cref="BookService"/> class implements the <see cref="IBookService"/> interface and
    /// acts as the main entry point for book-related business logic. It enables asynchronous access to book data,
    /// supports creating new book records, and manages book availability through checkout and return
    /// operations.</remarks>
    public class BookService : IBookService
    {
        private readonly IBookRepository _repository;

        public BookService(IBookRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Asynchronously retrieves all books from the data store.
        /// </summary>
        /// <returns>An enumerable collection of <see cref="BookDto"/> objects representing all books. The collection is empty if
        /// no books are found.</returns>
        public async Task<IEnumerable<BookDto>> GetAllAsync()
        {
            var books = await _repository.GetAllAsync();

            return books.Select(b => new BookDto
            {
                Id = b.Id,
                Title = b.Title,
                Author = b.Author,
                ISBN = b.ISBN,
                PublishedYear = b.PublishedYear,
                IsAvailable = b.IsAvailable
            });
        }

        /// <summary>
        /// Asynchronously retrieves a book by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the book to retrieve. Must be a positive integer.</param>
        /// <returns>A <see cref="BookDto"/> representing the book with the specified identifier, or <see langword="null"/> if no
        /// matching book is found.</returns>
        public async Task<BookDto?> GetByIdAsync(int id)
        {
            var book = await _repository.GetByIdAsync(id);
            return book is null ? null : MapToDto(book);
        }

        /// <summary>
        /// Creates a new book record using the specified data transfer object asynchronously.
        /// </summary>
        /// <param name="dto">The data transfer object containing the details of the book to create. Must not be <c>null</c>.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="BookDto"/>
        /// representing the newly created book.</returns>
        public async Task<BookDto> CreateAsync(CreateBookDto dto)
        {
            var book = new Book
            {
                Title = dto.Title,
                Author = dto.Author,
                ISBN = dto.ISBN,
                PublishedYear = dto.PublishedYear,
                IsAvailable = true
            };

            await _repository.AddAsync(book);
            return MapToDto(book);
        }

        /// <summary>
        /// Attempts to check out a book with the specified identifier.
        /// </summary>
        /// <remarks>This method marks the specified book as checked out if it exists and is currently
        /// available. If the book is already checked out or does not exist, the method returns <see
        /// langword="null"/>.</remarks>
        /// <param name="id">The unique identifier of the book to check out.</param>
        /// <returns>A <see cref="BookDto"/> representing the checked-out book if the operation succeeds; otherwise, <see
        /// langword="null"/> if the book does not exist or is not available.</returns>
        public async Task<BookDto?> CheckoutAsync(int id)
        {
            var book = await _repository.GetByIdAsync(id);
            if (book is null || !book.IsAvailable)
                return null;

            book.IsAvailable = false;
            await _repository.UpdateAsync(book);

            return MapToDto(book);
        }

        /// <summary>
        /// Marks a borrowed book as returned and updates its availability status asynchronously.
        /// </summary>
        /// <param name="id">The unique identifier of the book to return.</param>
        /// <returns>A <see cref="BookDto"/> representing the updated book if the return operation is successful; otherwise, <see
        /// langword="null"/> if the book does not exist or is already available.</returns>
        public async Task<BookDto?> ReturnAsync(int id)
        {
            var book = await _repository.GetByIdAsync(id);
            if (book is null || book.IsAvailable)
                return null;

            book.IsAvailable = true;
            await _repository.UpdateAsync(book);

            return MapToDto(book);
        }

        /// <summary>
        /// Maps a <see cref="Book"/> entity to a corresponding <see cref="BookDto"/>.
        /// </summary>
        /// <param name="b">The <see cref="Book"/> instance to map. Cannot be <c>null</c>.</param>
        /// <returns>A <see cref="BookDto"/> containing the data from the specified <see cref="Book"/>.</returns>
        private static BookDto MapToDto(Book b) => new()
        {
            Id = b.Id,
            Title = b.Title,
            Author = b.Author,
            ISBN = b.ISBN,
            PublishedYear = b.PublishedYear,
            IsAvailable = b.IsAvailable
        };
    }
}
