namespace FeelinCute.Models
{
    public class ProductForLiked
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public ProductForLiked(int Id, string Name, string Image)
        {
            this.Id = Id;
            this.Name = Name;
            this.Image = Image;
        }
    }

}
