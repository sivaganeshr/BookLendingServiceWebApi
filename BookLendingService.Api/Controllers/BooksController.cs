using BookLendingService.Api.DTOs;
using BookLendingService.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookLendingService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class BooksController : ControllerBase
    {
        private readonly IBookService _service;
        private readonly ILogger<BooksController> _logger;

        public BooksController(IBookService service, ILogger<BooksController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // GET /books
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<BookDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<BookDto>>> GetAll()
        {
            _logger.LogInformation("Retrieving all books");
            var books = await _service.GetAllAsync();
            return Ok(books);
        }

        // GET /books/{id}
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(BookDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BookDto>> GetById(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("GetById called with invalid id {Id}", id);
                return BadRequest("Id must be greater than zero.");
            }

            var book = await _service.GetByIdAsync(id);
            if (book == null)
            {
                _logger.LogWarning("Book with id {Id} not found", id);
                return NotFound();
            }

            return Ok(book);
        }

        // POST /books
        [HttpPost]
        [ProducesResponseType(typeof(BookDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BookDto>> Create([FromBody] CreateBookDto dto)
        {
            if (!ModelState.IsValid)
            {

                _logger.LogWarning("Invalid model state for CreateBook");
                return BadRequest(ModelState);
            }
            _logger.LogInformation("Create book request received. Title: {Title}", dto.Title);

            var created = await _service.CreateAsync(dto);

            _logger.LogInformation("Book created with Id {Id}", created.Id);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // POST /books/{id}/checkout
        [HttpPost("{id:int}/checkout")]
        [ProducesResponseType(typeof(BookDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BookDto>> Checkout(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Checkout called with invalid id {Id}", id);
                return BadRequest("Id must be greater than zero.");
            }
            var existing = await _service.GetByIdAsync(id);
            if (existing == null)
            {
                _logger.LogWarning("Checkout failed for id {Id}. Book not found.", id);
                return NotFound();
            }

            var result = await _service.CheckoutAsync(id);
            if (result == null)
            {
                _logger.LogWarning("Checkout failed for id {Id}. Book not found or not available.", id);
                return BadRequest("Book is already checked out.");
            }

            _logger.LogInformation("Book {Id} checked out successfully", id);
            return Ok(result);
        }

        // POST /books/{id}/return
        [HttpPost("{id:int}/return")]
        [ProducesResponseType(typeof(BookDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BookDto>> Return(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Return called with invalid id {Id}", id);
                return BadRequest("Id must be greater than zero.");
            }

            // Check existence first so NotFound is correct
            var existing = await _service.GetByIdAsync(id);
            if (existing == null)
            {
                _logger.LogWarning("Return failed for id {Id}. Book not found.", id);
                return NotFound();
            }

            var result = await _service.ReturnAsync(id);
            if (result == null)
            {
                _logger.LogWarning("Return failed for id {Id}. Book already available.", id);
                return BadRequest("Book is already returned.");
            }

            _logger.LogInformation("Book {Id} returned successfully", id);
            return Ok(result);
        }
    }
}
