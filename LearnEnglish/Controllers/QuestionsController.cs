using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LearnEnglish.Data;
using LearnEnglish.Models;
using NuGet.Versioning;
using Microsoft.AspNetCore.Authorization;

namespace LearnEnglish.Controllers
{
    [Authorize]
    public class QuestionsController : Controller
    {
        private readonly EnglishDbContext _context;

        public QuestionsController(EnglishDbContext context) => _context = context;

        [Authorize(Roles = "admin, teacher")]
        public async Task<IActionResult> Create(int id)
        {

            ViewData["QuizId"] = id;

            Quiz quiz = await _context.Quizzes.FindAsync(id);

            if(quiz == null)
                return NotFound();

            var questions = await _context.Questions.ToListAsync();
            int currentOrder = 1;
            var QuestionsWithId = questions.Where(q => q.QuizId == id);

            if (QuestionsWithId.Any())
                currentOrder = QuestionsWithId.Max(q => q.Order) + 1;

            ViewData["CurrentOrder"] = currentOrder;
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "admin, teacher")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] int quizId, [FromForm] string text, [FromForm] QuestionType type, [FromForm] int order)
        {
            if (ModelState.IsValid)
            {
                Question question = new Question
                {
                    QuizId = quizId,
                    Text = text,
                    Type = type,
                    Order = order
                };

                _context.Add(question);
                await _context.SaveChangesAsync();

                if(question.Id != null)
                    return RedirectToAction("AddAnswers", new { id = question.Id });
            }

            return NotFound();
        }

        [Authorize(Roles = "admin, teacher")]
        public async Task<IActionResult> AddAnswers(int id)
        {
            var question = await _context.Questions
                .Include(q => q.Quiz)
                .FirstOrDefaultAsync(m => m.Id == id);

            var answers = await _context.Answers.
                Include(a => a.Question)
                .ToListAsync();

            ViewData["CreatedQuestion"] = question;
            ViewData["Answers"] = answers;

            return View();
        }

        [HttpPost]
        [Authorize(Roles = "admin, teacher")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAnswers(List<Answer> answers)
        {
            var questionId = answers[0].QuestionId;
            var question = await _context.Questions.FindAsync(questionId);
            var quizId = question.QuizId;

            foreach (var answer in answers)
            {
                _context.Answers.Add(answer);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("EditQuiz", "Quizs", new { id = quizId });

        }

        [Authorize(Roles = "admin, teacher")]
        public async Task<IActionResult> EditTitle(int? id)
        {
            if (id == null)
                return NotFound();

            var question = await _context.Questions.FindAsync(id);

            if (question == null)
                return NotFound();

            ViewData["QuizId"] = new SelectList(_context.Quizzes, "Id", "Id", question.QuizId);
            return View(question);
        }

        [HttpPost]
        [Authorize(Roles = "admin, teacher")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTitle([FromForm] int Id, [FromForm] string text)
        {
            var updateQuestionTitle = await _context.Questions.FindAsync(Id);
            updateQuestionTitle.Text = text;

            _context.Update(updateQuestionTitle);
            await _context.SaveChangesAsync();
            return RedirectToAction("EditQuiz", "Quizs", new { id = updateQuestionTitle.QuizId } );
        }

        [Authorize(Roles = "admin, teacher")]
        public async Task<IActionResult> EditAnswer(int? id)
        {
            if (id == null)
                return NotFound();

            var question = await _context.Questions.FindAsync(id);
            var answers = await _context.Answers.Where(a => a.QuestionId == id).ToListAsync();

           ViewData["QuizId"] = question.QuizId;
            ViewData["Question"] = question;
            return View(answers);
        }

        [HttpPost]
        [Authorize(Roles = "admin, teacher")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAnswer([FromForm] List<Answer> answers)
        {
            if(answers.Count == 0)
            {
                return RedirectToAction("Index");
            }
            var questionId = answers[0].QuestionId;
            var question = await _context.Questions.FindAsync(questionId);
            var quizId = question.QuizId;

            foreach (var answer in answers)
            {
                var answerInDB = await _context.Answers.FindAsync(answer.Id);
                answerInDB.Text = answer.Text;
                answerInDB.IsCorrect = answer.IsCorrect;

                _context.Answers.Update(answerInDB);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("EditQuiz", "Quizs", new { id = quizId });
        }

       [Authorize(Roles = "admin, teacher")]
        public async Task<IActionResult> RaiseBlockUp(int? id)
        {
            var movableQuestion = await _context.Questions.FindAsync(id);
            await MoveBlock(movableQuestion, -1);
            return RedirectToAction("EditQuiz", "Quizs", new { id = movableQuestion.QuizId });
        }

        [Authorize(Roles = "admin, teacher")]
        public async Task<IActionResult> LowerBlockDown(int? id)
        {
            var movableQuestion = await _context.Questions.FindAsync(id);
            await MoveBlock(movableQuestion, 1);
            return RedirectToAction("EditQuiz", "Quizs", new { id = movableQuestion.QuizId });
        }

        [Authorize(Roles = "admin, teacher")]
        public async Task<IActionResult> Delete(int? id)
        {
            List<Question> questions = await _context.Questions.ToListAsync();
            Question deletedQuestion = questions.FirstOrDefault(b => b.Id == id);

            int quizId = deletedQuestion.QuizId;

            var filteredQuestions = questions.Where(b => b.Order > deletedQuestion.Order);

            if (deletedQuestion != null)
                _context.Questions.Remove(deletedQuestion);

            foreach (var block in filteredQuestions)
                block.Order--;

            await _context.SaveChangesAsync();

            return RedirectToAction("EditQuiz", "Quizs", new { id = quizId });
        }

        private bool QuestionExists(int id)
        {
            return _context.Questions.Any(e => e.Id == id);
        }

        [Authorize(Roles = "admin, teacher")]
        private async Task MoveBlock(Question movableQuestion, int newOrderOffset)
        {
            if (movableQuestion == null)
                return;

            var questions = await _context.Questions.ToListAsync();
            int oldOrder = movableQuestion.Order;
            int newOrder = oldOrder + newOrderOffset;
            int quizId = movableQuestion.QuizId;

            var slidingQuestion = questions.FirstOrDefault(b => b.QuizId == movableQuestion.QuizId && b.Order == newOrder);

            if (slidingQuestion != null)
            {
                slidingQuestion.Order = oldOrder;
                movableQuestion.Order = newOrder;
                await _context.SaveChangesAsync();
            }
        }
    }
}
