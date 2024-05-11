using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LearnEnglish.Data;
using LearnEnglish.Models;
using Microsoft.AspNetCore.Identity;
using static System.Reflection.Metadata.BlobBuilder;
using Microsoft.AspNetCore.Authorization;

namespace LearnEnglish.Controllers
{
    [Authorize]
    public class QuizsController : Controller
    {
        private readonly EnglishDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public QuizsController(EnglishDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var englishDbContext = _context.Quizzes.Include(q => q.Article);
            return View(await englishDbContext.ToListAsync());
        }

        public async Task<IActionResult> PassQuiz(int? id)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Article)
                .FirstOrDefaultAsync(m => m.Id == id);

            var questions = await _context.Questions
                .Include(q => q.Answers)
                .Where(q => q.QuizId == id)
                .OrderBy(q => q.Order)
                .ToListAsync();

            var answers = await _context.Answers.ToListAsync();

            ViewData["quiz"] = quiz;
            ViewData["questions"] = questions;
            ViewData["answers"] = answers;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PassQuiz([FromForm] Dictionary<int, string> answers)
        {
            const double NormalGradePercentage = 0.56;
            const double GoodGradePercentage = 0.71;
            const double GreatGradePercentage = 0.86;

            const int BadGradeValue = 2;
            const int NormalGradeValue = 3;
            const int GoodGradeValue = 4;
            const int GreatGradeValue = 5;

            double userGradePercentage = 0;
            int totalScore = 0;

            var filteredAnswers = answers
                .Where(a => a.Value != null);

            var userAnswers = filteredAnswers
                .Select(fa => fa.Value)
                .ToList();
            
            var questions = await _context.Questions
                .Where(q => filteredAnswers
                .Select(fa => fa.Key)
                .Contains(q.Id))
                .OrderBy(q => q.Order)
                .ToListAsync();

            int numberOfQuestions = questions.Count;
            int numberOfCorrectUserAnswers = 0;

            var correctAnswers = await _context.Answers
                .Where(ca => ca.IsCorrect == true)
                .ToListAsync();

            var filteredCorrectAnswers = correctAnswers
                .Where(ca => questions
                .Any(q => q.Id == ca.QuestionId))
                .OrderBy(ca => ca.Question.Order)
                .ToList();

            var userId = _userManager.GetUserId(this.User);

            int newAttempt = 1;
            var previousAttempts = await _context.TestResults.Where(b => b.QuizId == questions[0].QuizId && b.UserId == userId).ToListAsync();

            if (previousAttempts.Any())
            {
                newAttempt = previousAttempts.Max(b => b.Attempt) + 1;
            }

            foreach(var filteredAnswer in filteredAnswers)
            {
                var tempUserAnswer = new UserAnswer
                {
                    UserId = userId,
                    QuestionId = filteredAnswer.Key, 
                    TextAnswer = filteredAnswer.Value,
                    Attempt = newAttempt
                };

                _context.Add(tempUserAnswer);
                await _context.SaveChangesAsync();
            }

            for (int i = 0; i < numberOfQuestions; i++)
            {
                if (userAnswers[i].ToLower() == filteredCorrectAnswers[i].Text.ToLower())
                {
                    numberOfCorrectUserAnswers++;
                }
            }

            userGradePercentage = (double)numberOfCorrectUserAnswers / (double)numberOfQuestions;

            if(userGradePercentage < NormalGradePercentage)
            {
                totalScore = BadGradeValue;
            }
            else if(userGradePercentage >= NormalGradePercentage && userGradePercentage < GoodGradePercentage)
            {
                totalScore = NormalGradeValue;
            }
            else if(userGradePercentage >= GoodGradePercentage && userGradePercentage < GreatGradePercentage)
            {
                totalScore = GoodGradeValue;
            }
            else
            {
                totalScore = GreatGradeValue;
            }

            var testResult = new TestResult
            {
                QuizId = questions[0].QuizId,
                UserId = userId,
                Score = totalScore,
                Attempt = newAttempt
            };

            _context.Add(testResult);
            await _context.SaveChangesAsync();

            return RedirectToAction("ShowFinalResult", new { id = testResult.Id });
        }

        public async Task<IActionResult> ShowFinalResult(int id)
        {
            var finalResult = await _context.TestResults.FindAsync(id);
            return View(finalResult);
        }

        [Authorize(Roles = "admin, teacher")]
        public async Task<IActionResult> Create()
        {
            var articles = await _context.Articles.ToListAsync();
            ViewData["Articles"] = new SelectList(articles, "Id", "Title");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] int articleId, [FromForm] string title)
        {
            if (ModelState.IsValid)
            {
                var quiz = new Quiz
                {
                    ArticleId = articleId,
                    Title = title,
                };

                _context.Add(quiz);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "admin, teacher")]
        public async Task<IActionResult> EditQuiz(int id)
        {
            
            var questions = await _context.Questions.ToListAsync();
            var answers = await _context.Answers.ToListAsync();

            var quiz = await _context.Quizzes
                .Include(a => a.Article)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (quiz == null)
            {
                return NotFound();
            }

            ViewData["QuizId"] = quiz.Id;
            ViewData["QuizTitle"] = quiz.Title;

            ViewData["quiz"] = quiz;
            ViewData["questions"] = questions;
            ViewData["answers"] = answers;

            return View();
        }

        [Authorize(Roles = "admin, teacher")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quiz = await _context.Quizzes.FindAsync(id);
            if (quiz == null)
            {
                return NotFound();
            }
            ViewData["ArticleId"] = new SelectList(_context.Articles, "Id", "Id", quiz.ArticleId);
            return View(quiz);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromForm] int Id, [FromForm] string title)
        {
            var updateQuiz = await _context.Quizzes.FindAsync(Id);
            updateQuiz.Title = title;

            _context.Update(updateQuiz);
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "admin, teacher")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quiz = await _context.Quizzes
                .Include(q => q.Article)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (quiz == null)
            {
                return NotFound();
            }

            return View(quiz);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var quiz = await _context.Quizzes.FindAsync(id);
            if (quiz != null)
            {
                _context.Quizzes.Remove(quiz);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool QuizExists(int id)
        {
            return _context.Quizzes.Any(e => e.Id == id);
        }
    }
}
