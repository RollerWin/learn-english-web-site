using LearnEnglish.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearnEnglish.Controllers
{
    public class UsersController : Controller
    {
        private readonly EnglishDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public UsersController(EnglishDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var currentUserId = _userManager.GetUserId(this.User);
            IdentityUser currentUser = await _userManager.FindByIdAsync(currentUserId);
            var currentUserRole = await _userManager.GetRolesAsync(currentUser);
            
            ViewData["user"] = currentUser.UserName;
            ViewData["email"] = currentUser.Email;
            ViewData["role"] = currentUserRole.FirstOrDefault();

            return View();
        }

        public async Task<IActionResult> GetResults()
        {
            var userId = _userManager.GetUserId(this.User);
            var results = await _context.TestResults
                .Include(r => r.Quiz)
                .Where(r => r.UserId == userId)
                .ToListAsync();

            var quizTitles = results.Select(r => r.Quiz.Title).ToList();
            var qIDS = results.Select(r => r.Quiz).ToList();
            var aIDS = qIDS.Select(a => a.Article).ToList();

            ViewData["quizTitles"] = quizTitles;
            ViewData["results"] = results;

            return View();
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(this.User);
            var testResult = await _context.TestResults.FindAsync(id);
            var quiz = await _context.Quizzes.FindAsync(testResult.QuizId);

            var userAnswers = await _context.UserAnswers
                .Include(ua => ua.Question)
                .Where(
                    ua => ua.UserId == userId && 
                    ua.Question.QuizId == quiz.Id && 
                    ua.Attempt == testResult.Attempt
                    )
                .OrderBy(ua => ua.Id)
                .ToListAsync();

            var correctAnswers = await _context.Answers
                .Include(ca => ca.Question)
                .Where(
                    ca => userAnswers
                    .Select(ua => ua.QuestionId)
                    .Contains(ca.QuestionId) && 
                    ca.IsCorrect == true
                    )
                .OrderBy(ca => ca.Question.Order)
                .ToListAsync();

            var questions = await _context.Questions
                .Include(q => q.Quiz)
                .OrderBy(q => q.Order)
                .ToListAsync();


            ViewData["questions"] = questions;
            ViewData["userAnswers"] = userAnswers;
            ViewData["correctAnswers"] = correctAnswers;

            return View();
        }

       
    }
}
