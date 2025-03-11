using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Enum;

namespace Infrastructure.Model
{
    public class User
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }
        [Required]
        [StringLength(40)]
        public string? Username { get; set; }

        [Required]
        [StringLength(40)]
        public string? Email { get; set; }

        [Required]
        [StringLength(1000)]
        public string? Password { get; set; }

        [Required]
        public Role Role { get; set; }

        public virtual Refreshtoken? Token { get; set; }

    }
}
