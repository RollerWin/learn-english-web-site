namespace LearnEnglish.Models
{
    public class Block
    {
        public int Id { get; set; }
        public int ArticleId { get; set; }
        public Article Article { get; set; }
        public BlockType Type { get; set; } 
        public string Content { get; set; } 
        public int Order { get; set; }
    }

    public enum BlockType
    {
        Text,
        Image, 
        Video
    }
}
