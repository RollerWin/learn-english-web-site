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
using System.Dynamic;
using Microsoft.AspNetCore.Authorization;

namespace LearnEnglish.Controllers
{
    [Authorize]
    public class ArticlesController : Controller
    {
        private readonly EnglishDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ArticlesController(EnglishDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int? id)
        {
            IQueryable<Article> englishDbContext = _context.Articles
                .Include(a => a.Author)
                .Include(a => a.Course);

            if (id.HasValue)
            {
                englishDbContext = englishDbContext.Where(a => a.CourseId == id);
            }

            return View(await englishDbContext.ToListAsync());
        }

        public async Task<IActionResult> ShowArticles(int id)
        {
            var englishDbContext = _context.Articles
                .Include(a => a.Author)
                .Include(a => a.Course)
                .Where(a => a.CourseId == id);

            var selectedCourse = await _context.Courses.FirstOrDefaultAsync(ct => ct.Id == id);
            ViewData["CourseTitle"] = selectedCourse.Title;
            ViewData["SelectedCourseId"] = selectedCourse.Id;


            return View(await englishDbContext.ToListAsync());
        }

        public async Task<IActionResult> ShowSelectedArticle(int id)
        {
            var blocks = await _context.Blocks.ToListAsync();

            var article = await _context.Articles
                .Include(a => a.Author)
                .Include(a => a.Course)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (article == null)
            {
                return NotFound();
            }

            var selectedCourse = await _context.Courses.FirstOrDefaultAsync(c => c.Id == article.CourseId);

            ViewData["CourseId"] = selectedCourse.Id;
            ViewData["ArticleId"] = article.Id;
            ViewData["ArticleTitle"] = article.Title;
            ViewData["ArticleAuthor"] = article.Author;

            ViewData["article"] = article;
            ViewData["blocks"] = blocks;
            return View();
        }

        [Authorize(Roles = "admin, teacher")]
        public async Task<IActionResult> Create(int? id)
        {
            var courses = await _context.Courses.ToListAsync();
            var coursesSelectList = new SelectList(courses, "Id", "Title");

            if(id.HasValue)
            {
                ViewData["CourseId"] = id;
            }

            ViewData["Courses"] = coursesSelectList;
            ViewData["AuthorId"] = new SelectList(_context.Set<IdentityUser>(), "Id", "Id");
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "admin, teacher")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] int courseId, [FromForm] string title)
        {
            
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(this.User);
                var dateTime = DateTime.UtcNow;

                var article = new Article
                {
                    CourseId = courseId,
                    Title = title,
                    AuthorId = userId,
                    CreatedDate = dateTime
                };

                _context.Add(article);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "admin, teacher")]
        public async Task<IActionResult> AddBlock(int id)
        {
            var blocks = await _context.Blocks.ToListAsync();

            var article = await _context.Articles
                .Include(a => a.Author)
                .Include(a => a.Course)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (article == null)
            {
                return NotFound();
            }

            ViewData["CourseId"] = article.CourseId;
            ViewData["ArticleId"] = article.Id;
            ViewData["ArticleTitle"] = article.Title;
            ViewData["ArticleAuthor"] = article.Author;

            ViewData["article"] = article;
            ViewData["blocks"] = blocks;
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "admin, teacher")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddBlock(List<Block> blocks)
        {
            if (ModelState.IsValid)
            {
                foreach (var block in blocks)
                    _context.Blocks.Add(block);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "admin, teacher")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var article = await _context.Articles.FindAsync(id);
            if (article == null)
            {
                return NotFound();
            }

            var users = await _userManager.Users.ToListAsync();
            var usersSelectList = new SelectList(users, "Id", "UserName");

            ViewData["Users"] = usersSelectList;
            ViewData["AuthorId"] = new SelectList(_context.Set<IdentityUser>(), "Id", "Id", article.AuthorId);
            ViewData["CourseId"] = article.CourseId;
            ViewData["ArticleId"] = article.Id;
            return View(article);
        }

        [HttpPost]
        [Authorize(Roles = "admin, teacher")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromForm] int Id, [FromForm] string authorId, [FromForm] string title)
        {
            var updateArticle = await _context.Articles.FindAsync(Id);
            updateArticle.AuthorId = authorId;
            updateArticle.Title = title;

            _context.Update(updateArticle);
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

            var article = await _context.Articles
                .Include(a => a.Author)
                .Include(a => a.Course)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (article == null)
            {
                return NotFound();
            }

            var courseId = article.CourseId;
            ViewData["CourseId"] = courseId;

            return View(article);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "admin, teacher")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article != null)
            {
                _context.Articles.Remove(article);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ArticleExists(int id)
        {
            return _context.Articles.Any(e => e.Id == id);
        }

        public class BlockViewModel
        {
            public BlockType Type { get; set; }
            public string Content { get; set; }
            public int Order { get; set; }
        }

    }
}
