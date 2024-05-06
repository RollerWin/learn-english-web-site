using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LearnEnglish.Controllers
{
    public class RolesController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;

        public RolesController(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public ActionResult Index() => View();

        public async Task<IActionResult> Assign()
        {
            // Получаем список пользователей и ролей
            var users = await _userManager.Users.ToListAsync();
            var roles = await _roleManager.Roles.ToListAsync();

            // Подготавливаем SelectListItem для пользователей и ролей
            var usersSelectList = new SelectList(users, "Id", "UserName");
            var rolesSelectList = new SelectList(roles, "Name", "Name");

            // Передаем списки в представление
            ViewData["Users"] = usersSelectList;
            ViewData["Roles"] = rolesSelectList;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(string roleName)
        {
            if (!string.IsNullOrWhiteSpace(roleName))
            {
                var roleExist = await _roleManager.RoleExistsAsync(roleName);

                if (!roleExist)
                {
                    var result = await _roleManager.CreateAsync(new IdentityRole(roleName));

                    if (result.Succeeded)
                        return RedirectToAction("Index");
                    else
                        // Обработка ошибок при создании роли
                        foreach (var error in result.Errors)
                            ModelState.AddModelError(string.Empty, error.Description);
                }
                else
                {
                    // Обработка ситуации, когда роль с таким именем уже существует
                    ModelState.AddModelError(string.Empty, "Role with this name already exists.");
                }
            }
            else
            {
                // Обработка ситуации, когда имя роли не указано
                ModelState.AddModelError(string.Empty, "Role name cannot be empty.");
            }

            // Если возникли ошибки, возвращаем представление снова для ввода данных
            return View("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Assign(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound();

            var result = await _userManager.AddToRoleAsync(user, roleName);

            if (result.Succeeded)
            {
                // Роль успешно назначена
                return RedirectToAction("Index", "Home");
            }
            else
            {
                // Обработка ошибки при назначении роли
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                return View();
            }
        }
    }
}
