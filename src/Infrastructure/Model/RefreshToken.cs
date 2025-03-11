using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Model
{
    public class Refreshtoken
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TokenId { get; set; }

        [Required]
        [StringLength(1000)]
        public string? RefreshToken { get; set; }

        [Required]
        public DateTime? Expiration { get; set; }
        public int UserId { get; set; }
        public virtual User? User { get; set; } = null!;

    }
}
