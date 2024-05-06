using Microsoft.AspNetCore.Identity;

namespace LearnEnglish.Models
{
    public class News
    {
        public int Id { get; set; }
        public string AuthorId { get; set; }
        public IdentityUser Author { get; set; }
        public string Annotation { get; set; }
        public string Source { get; set; }
        public string Img { get; set; }
    }
}
