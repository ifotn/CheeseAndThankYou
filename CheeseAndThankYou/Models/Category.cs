namespace CheeseAndThankYou.Models
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }

        // child ref to Products.  Make child nullable so we can first add Categories
        public List<Product>? Products { get; set; }
    }
}
