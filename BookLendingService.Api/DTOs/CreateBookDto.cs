using System.ComponentModel.DataAnnotations;

namespace BookLendingService.Api.DTOs
{
    public class CreateBookDto
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Author is required")]
        [StringLength(100, ErrorMessage = "Author cannot exceed 100 characters")]
        public string Author { get; set; } = string.Empty;

        [Required(ErrorMessage = "ISBN is required")]
        [RegularExpression(@"^[0-9\-]+$", ErrorMessage = "ISBN must contain only digits and hyphens")]
        public string ISBN { get; set; } = string.Empty;

        [Range(1, 9999, ErrorMessage = "PublishedYear must be a valid year")]
        public int PublishedYear { get; set; }
    }
}
