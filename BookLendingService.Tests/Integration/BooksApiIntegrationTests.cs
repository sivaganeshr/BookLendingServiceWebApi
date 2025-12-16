using BookLendingService.Api.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;

namespace BookLendingService.Tests.Integration
{
    /// <summary>
    /// Integration tests for the Books API endpoints.
    /// </summary>
    public class BooksApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public BooksApiIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Post_Then_Get_Books_Workflow_Should_Succeed()
        {
            // Arrange
            var createDto = new CreateBookDto
            {
                Title = "Integration Test",
                Author = "Test Author",
                ISBN = "45178",
                PublishedYear = 2025
            };

            // Act: create a book
            var postResponse = await _client.PostAsJsonAsync("/api/books", createDto);

            // Assert create
            Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
            var created = await postResponse.Content.ReadFromJsonAsync<BookDto>();
            Assert.NotNull(created);
            Assert.Equal(createDto.Title, created!.Title);

            // Act: get all books
            var getResponse = await _client.GetAsync("/api/books");
            getResponse.EnsureSuccessStatusCode();

            var books = await getResponse.Content.ReadFromJsonAsync<List<BookDto>>();

            Assert.NotNull(books);
            Assert.Contains(books!, b => b.Id == created.Id);
        }

        [Fact]
        public async Task Checkout_And_Return_Book_Workflow_Should_Succeed()
        {
            // First create a book
            var createDto = new CreateBookDto
            {
                Title = "Test integration",
                Author = "Tester",
                ISBN = "12-545",
                PublishedYear = 2024
            };

            var postResponse = await _client.PostAsJsonAsync("/api/books", createDto);
            postResponse.EnsureSuccessStatusCode();
            
            var created = await postResponse.Content.ReadFromJsonAsync<BookDto>();
            Assert.NotNull(created);

            // Checkout the book
            var checkoutResponse = await _client.PostAsync($"/api/books/{created!.Id}/checkout", null);
            checkoutResponse.EnsureSuccessStatusCode();

            var checkedOut = await checkoutResponse.Content.ReadFromJsonAsync<BookDto>();
            Assert.NotNull(checkedOut);
            Assert.False(checkedOut!.IsAvailable);

            // Return the book
            var returnResponse = await _client.PostAsync($"/api/books/{created.Id}/return", null);
            returnResponse.EnsureSuccessStatusCode();

            var returned = await returnResponse.Content.ReadFromJsonAsync<BookDto>();

            Assert.NotNull(returned);
            Assert.True(returned!.IsAvailable);
        }

        [Fact]
        public async Task GetById_With_Invalid_Id_Should_Return_BadRequest()
        {
            var response = await _client.GetAsync("/api/books/0");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
