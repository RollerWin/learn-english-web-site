using Microsoft.AspNetCore.Identity;

namespace LearnEnglish.Models
{
    public class Article
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public Course Course { get; set; }
        public string AuthorId { get; set; }
        public IdentityUser Author { get; set; }
        public string Title { get; set; }
        public DateTime CreatedDate { get; set; }
        public ICollection<Block> Blocks { get; set; }
    }
}
