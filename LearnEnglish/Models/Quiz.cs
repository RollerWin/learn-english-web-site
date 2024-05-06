namespace LearnEnglish.Models
{
    public class Quiz
    {
        public int Id { get; set; }
        public int ArticleId { get; set; }
        public Article Article { get; set; }
        public string Title { get; set; }
        public ICollection<Question> Questions { get; set; }
    }
}
