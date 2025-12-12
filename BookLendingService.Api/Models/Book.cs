namespace BookLendingService.Api.Models
{
    public class Book
    {
        /// <summary>
        /// Gets or sets the unique identifier for the entity.
        /// </summary>
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string ISBN { get; set; } = string.Empty;
        public int PublishedYear { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the resource is currently available for use once it is picked it is set to be false.
        /// </summary>
        public bool IsAvailable { get; set; } = true;
    }
}
