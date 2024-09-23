using System.ComponentModel;

namespace CheeseAndThankYou.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        public string Name { get; set; }

        [DisplayName("Stink Rating")]
        public int StinkRating { get; set; }

        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Image { get; set; }
        public string Size { get; set; }
    }
}
