using BookLendingService.Api.DTOs;
using BookLendingService.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookLendingService.Api.Controllers
{
    [ApiController]
    [Route("books")]
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
        public async Task<ActionResult<IEnumerable<BookDto>>> GetAll()
        {
            var books = await _service.GetAllAsync();
            return Ok(books);
        }

        // GET /books/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<BookDto>> GetById(int id)
        {
            var book = await _service.GetByIdAsync(id);
            if (book is null)
                return NotFound();

            return Ok(book);
        }

        // POST /books
        [HttpPost]
        public async Task<ActionResult<BookDto>> Create([FromBody] CreateBookDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            _logger.LogInformation("Create book request received. Title: {Title}", dto.Title);

            var created = await _service.CreateAsync(dto);

            _logger.LogInformation("Book created with Id {Id}", created.Id);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // POST /books/{id}/checkout
        [HttpPost("{id:int}/checkout")]
        public async Task<ActionResult<BookDto>> Checkout(int id)
        {
            var result = await _service.CheckoutAsync(id);
            if (result is null)
                return BadRequest(new { message = "Book not found or already checked out." });

            return Ok(result);
        }

        // POST /books/{id}/return
        [HttpPost("{id:int}/return")]
        public async Task<ActionResult<BookDto>> Return(int id)
        {
            var result = await _service.ReturnAsync(id);
            if (result is null)
                return BadRequest(new { message = "Book not found or not checked out." });

            return Ok(result);
        }
    }
}
