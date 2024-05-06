using Microsoft.AspNetCore.Identity;
using static System.Net.Mime.MediaTypeNames;

namespace LearnEnglish.Models
{
    public class TestResult
    {
        public int Id { get; set; }
        public int QuizId { get; set; }
        public Quiz Quiz { get; set; }
        public string UserId { get; set; }
        public IdentityUser User { get; set; }
        public int Score { get; set; }
        public int Attempt { get; set; }
    }
}
