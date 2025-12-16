using BookLendingService.Api.DTOs;
using BookLendingService.Api.Models;
using BookLendingService.Api.Repositories;
using BookLendingService.Api.Services;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLendingService.Tests.Services
{
    public class BookServiceTests
    {
        private readonly Mock<IBookRepository> _mockRepo;
        private readonly BookService _service;

        public BookServiceTests()
        {
            _mockRepo = new Mock<IBookRepository>(MockBehavior.Strict);
            _service = new BookService(_mockRepo.Object);
        }

        [Fact]
        public async Task CreateAsync_Should_Create_A_New_Book()
        {
            // Arrange
            var dto = new CreateBookDto
            {
                Title = "Test Title",
                Author = "Test Author",
                ISBN = "12345",
                PublishedYear = 2024
            };

            _mockRepo.Setup(r => r.AddAsync(It.IsAny<Book>())).Returns(Task.CompletedTask);
            _mockRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
            // Act
            var result = await _service.CreateAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Be(dto.Title);
            result.IsAvailable.Should().BeTrue();
            _mockRepo.Verify(r => r.AddAsync(It.Is<Book>(b =>
                b.Title == dto.Title &&
                b.Author == dto.Author &&
                b.ISBN == dto.ISBN &&
                b.PublishedYear == dto.PublishedYear &&
                b.IsAvailable == true
            )), Times.Once);

            _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
            _mockRepo.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task CheckoutAsync_Should_Set_IsAvailable_To_False()
        {
            // Arrange
            var book = new Book
            {
                Id = 1,
                Title = "Test",
                Author = "A",
                ISBN = "111",
                PublishedYear = 2020,
                IsAvailable = true
            };

            _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(book);

            _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<Book>())).Returns(Task.CompletedTask);
            _mockRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _service.CheckoutAsync(1);

            // Assert
            result.Should().NotBeNull();
            result!.IsAvailable.Should().BeFalse();
            book.IsAvailable.Should().BeFalse();

            _mockRepo.Verify(r => r.UpdateAsync(It.Is<Book>(b => b.Id == 1 && b.IsAvailable == false)), Times.Once);
            _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
            _mockRepo.VerifyAll();
        }
        [Fact]
        public async Task CheckoutAsync_Should_Return_Null_When_Already_CheckedOut()
        {
            var book = new Book
            {
                Id = 1,
                IsAvailable = false
            };

            _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(book);

            // Act
            var result = await _service.CheckoutAsync(1);

            // Assert
            result.Should().BeNull();

            _mockRepo.Verify(r => r.UpdateAsync(It.IsAny<Book>()), Times.Never);
            _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
            _mockRepo.VerifyAll();
        }
        [Fact]
        public async Task ReturnAsync_Should_Set_IsAvailable_To_True_And_Save()
        {
            var book = new Book { Id = 1, Title = "X", Author = "Y", ISBN = "1-1", PublishedYear = 2020, IsAvailable = false };

            _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(book);
            _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<Book>())).Returns(Task.CompletedTask);
            _mockRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var result = await _service.ReturnAsync(1);

            result.Should().NotBeNull();
            result!.IsAvailable.Should().BeTrue();
            book.IsAvailable.Should().BeTrue();

            _mockRepo.Verify(r => r.UpdateAsync(It.Is<Book>(b => b.Id == 1 && b.IsAvailable == true)), Times.Once);
            _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
            _mockRepo.VerifyAll();
        }

        [Fact]
        public async Task ReturnAsync_Should_Return_Null_When_Not_CheckedOut()
        {
            var book = new Book
            {
                Id = 1,
                IsAvailable = true
            };

            _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(book);

            var result = await _service.ReturnAsync(1);

            result.Should().BeNull();

            _mockRepo.Verify(r => r.UpdateAsync(It.IsAny<Book>()), Times.Never);
            _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
            _mockRepo.VerifyAll();
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_MappedDtos()
        {
            var books = new List<Book>
            {
                new() { Id = 1, Title = "A", Author = "AA", ISBN = "1-1", PublishedYear = 2020, IsAvailable = true },
                new() { Id = 2, Title = "B", Author = "BB", ISBN = "2-2", PublishedYear = 2021, IsAvailable = false }
            };

            _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(books);

            var result = (await _service.GetAllAsync()).ToList();

            result.Should().HaveCount(2);
            result[0].Id.Should().Be(1);
            result[1].IsAvailable.Should().BeFalse();

            _mockRepo.Verify(r => r.GetAllAsync(), Times.Once);
            _mockRepo.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetByIdAsync_When_Id_Invalid_Should_Return_Null()
        {
            var result = await _service.GetByIdAsync(0);
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_When_NotFound_Should_Return_Null()
        {
            _mockRepo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync((Book?)null);

            var result = await _service.GetByIdAsync(10);

            result.Should().BeNull();
            _mockRepo.VerifyAll();
        }

        [Fact]
        public async Task CreateAsync_Should_Throw_When_Dto_Is_Null()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.CreateAsync(null!));
        }

        [Fact]
        public async Task CheckoutAsync_When_Id_Invalid_Should_Return_Null()
        {
            var result = await _service.CheckoutAsync(0);
            result.Should().BeNull();
        }

        [Fact]
        public async Task ReturnAsync_When_Id_Invalid_Should_Return_Null()
        {
            var result = await _service.ReturnAsync(0);
            result.Should().BeNull();
        }
    }
}
