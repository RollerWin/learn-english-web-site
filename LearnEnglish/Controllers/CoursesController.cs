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
using Microsoft.AspNetCore.Authorization;

namespace LearnEnglish.Controllers
{
    [Authorize]
    public class CoursesController : Controller
    {
        private readonly EnglishDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public CoursesController(EnglishDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var englishDbContext = _context.Courses.Include(c => c.User);
            return View(await englishDbContext.ToListAsync());
        }

        public async Task<IActionResult> ShowCourses()
        {
            var englishDbContext = _context.Courses.Include(c => c.User);
            return View(await englishDbContext.ToListAsync());
        }

        [Authorize(Roles = "admin, teacher")]
        public async Task<IActionResult> Create() => View();

        [HttpPost]
        [Authorize(Roles = "admin, teacher")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] string title, [FromForm] string description, [FromForm] string imgPath)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(this.User);

                var course = new Course
                {
                    Title = title,
                    Description = description,
                    UserId = userId,
                    ImgPath = imgPath
                };

                _context.Add(course);
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

            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            var users = await _userManager.Users.ToListAsync();
            var usersSelectList = new SelectList(users, "Id", "UserName");

            ViewData["CourseId"] = course.Id;
            ViewData["Users"] = usersSelectList;
            return View(course);
        }

        [HttpPost]
        [Authorize(Roles = "admin, teacher")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromForm] int Id, [FromForm] string title, [FromForm] string description, [FromForm] string userId, [FromForm] string imgPath)
        {
            if(Id != null && title != null && description != null && userId != null && imgPath != null)
            {
                var updateCourse = await _context.Courses.FindAsync(Id);
                updateCourse.Title = title;
                updateCourse.Description = description;
                updateCourse.UserId = userId;
                updateCourse.ImgPath = imgPath;
                _context.Update(updateCourse);
            }
           
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

            var course = await _context.Courses
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "admin, teacher")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                _context.Courses.Remove(course);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CourseExists(int id)
        {
            return _context.Courses.Any(e => e.Id == id);
        }
    }
}
