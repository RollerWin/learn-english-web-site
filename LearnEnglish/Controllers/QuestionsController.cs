using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LearnEnglish.Data;
using LearnEnglish.Models;

namespace LearnEnglish.Controllers
{
    public class QuestionsController : Controller
    {
        private readonly EnglishDbContext _context;

        public QuestionsController(EnglishDbContext context) => _context = context;

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

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var question = await _context.Questions.FindAsync(id);

            if (question == null)
                return NotFound();

            ViewData["QuizId"] = new SelectList(_context.Quizzes, "Id", "Id", question.QuizId);
            //ViewData["QuizId"] = question.QuizId;
            return View(question);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,QuizId,Text,Type,Order")] Question question)
        {
            if (id != question.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(question);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!QuestionExists(question.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["QuizId"] = new SelectList(_context.Quizzes, "Id", "Id", question.QuizId);
            return View(question);
        }

        public async Task<IActionResult> RaiseBlockUp(int? id)
        {
            var movableQuestion = await _context.Questions.FindAsync(id);
            await MoveBlock(movableQuestion, -1);
            return RedirectToAction("EditQuiz", "Quizs", new { id = movableQuestion.QuizId });
        }

        public async Task<IActionResult> LowerBlockDown(int? id)
        {
            var movableQuestion = await _context.Questions.FindAsync(id);
            await MoveBlock(movableQuestion, 1);
            return RedirectToAction("EditQuiz", "Quizs", new { id = movableQuestion.QuizId });
        }

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
