using System.ComponentModel.DataAnnotations;

namespace CheeseAndThankYou.Models
{
    public class CartItem
    {
        public int CartItemId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public string CustomerId { get; set; }

        // FK
        [Required]
        public int ProductId { get; set; }

        // parent ref
        public Product Product { get; set; }


    }
}
