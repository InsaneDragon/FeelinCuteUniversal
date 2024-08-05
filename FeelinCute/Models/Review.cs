using System.ComponentModel.DataAnnotations;

namespace FeelinCute.Models
{
    public class Review
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Text { get; set; }
        [Required]
        public string AccountId { get; set; }
    }
}
