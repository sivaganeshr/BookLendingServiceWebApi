using BookLendingService.Api.DTOs;

namespace BookLendingService.Api.Services
{
    public interface IBookService
    {
        Task<IEnumerable<BookDto>> GetAllAsync();
        Task<BookDto?> GetByIdAsync(int id);
        Task<BookDto> CreateAsync(CreateBookDto dto);
        Task<BookDto?> CheckoutAsync(int id);
        Task<BookDto?> ReturnAsync(int id);
    }
}
