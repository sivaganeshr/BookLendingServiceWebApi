using BookLendingService.Api.Data;
using BookLendingService.Api.Models;
using BookLendingService.Api.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BookLendingService.Tests.Repositories
{
    public class BookRepositoryTests
    {
        private readonly AppDbContext _context;
        private readonly BookRepository _repository;

        public BookRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _repository = new BookRepository(_context);
        }

        [Fact]
        public async Task AddAsync_Should_Add_Book()
        {
            var book = new Book
            {
                Title = "Repo Test",
                Author = "Tester",
                ISBN = "111",
                PublishedYear = 2024,
                IsAvailable = true
            };

            await _repository.AddAsync(book);
            
            await _context.SaveChangesAsync();
            _context.Books.Count().Should().Be(1);
            _context.Entry(book).State.Should().Be(EntityState.Unchanged);
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_All_Books()
        {
            _context.Books.Add(new Book { Title = "Abc" });
            _context.Books.Add(new Book { Title = "xyz" });
            await _context.SaveChangesAsync();

            var result = await _repository.GetAllAsync();

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Book()
        {
            var book = new Book { Title = "Find Me" };
            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            var result = await _repository.GetByIdAsync(book.Id);

            result.Should().NotBeNull();
            result!.Title.Should().Be("Find Me");
        }

        [Fact]
        public async Task UpdateAsync_Should_Update_Book()
        {
            var book = new Book { Title = "Old Title" };
            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            book.Title = "New Title";
            await _repository.UpdateAsync(book);
            await _repository.SaveChangesAsync();

            var updated = await _context.Books.FindAsync(book.Id);
            updated!.Title.Should().Be("New Title");
        }

        [Fact]
        public async Task SaveChangesAsync_Should_Save_Data()
        {
            _context.Books.Add(new Book { Title = "Unsaved" });

            await _repository.SaveChangesAsync();

            _context.Books.Count().Should().Be(1);
        }

        [Fact]
        public async Task AddAsync_Should_Throw_When_Book_Is_Null()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.AddAsync(null!));
        }
    }
}
