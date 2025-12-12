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
            _mockRepo = new Mock<IBookRepository>();
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

            // Act
            var result = await _service.CreateAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Be(dto.Title);
            result.IsAvailable.Should().BeTrue();
            _mockRepo.Verify(r => r.AddAsync(It.IsAny<Book>()), Times.Once);
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

            // Act
            var result = await _service.CheckoutAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.IsAvailable.Should().BeFalse();
            _mockRepo.Verify(r => r.UpdateAsync(book), Times.Once);
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
        }
        [Fact]
        public async Task ReturnAsync_Should_Set_IsAvailable_To_True()
        {
            var book = new Book
            {
                Id = 1,
                IsAvailable = false
            };

            _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(book);

            var result = await _service.ReturnAsync(1);

            result.Should().NotBeNull();
            result.IsAvailable.Should().BeTrue();
            _mockRepo.Verify(r => r.UpdateAsync(book), Times.Once);
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
        }
    }
}
