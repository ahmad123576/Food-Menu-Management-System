using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Food_Menu_Management_System.Models
{
    public class Cart
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = "";

        public List<CartItem> Items { get; set; } = new();
    }
}