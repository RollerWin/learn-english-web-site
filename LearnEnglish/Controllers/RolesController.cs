using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LearnEnglish.Controllers
{
    [Authorize(Roles="admin")]
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
            var users = await _userManager.Users.ToListAsync();
            var roles = await _roleManager.Roles.ToListAsync();

            var usersSelectList = new SelectList(users, "Id", "UserName");
            var rolesSelectList = new SelectList(roles, "Name", "Name");

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
                        foreach (var error in result.Errors)
                            ModelState.AddModelError(string.Empty, error.Description);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Role with this name already exists.");
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Role name cannot be empty.");
            }

            return View("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Assign(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);

            var removeRolesResult = await _userManager.RemoveFromRolesAsync(user, userRoles);

            if (!removeRolesResult.Succeeded)
            {
                foreach (var error in removeRolesResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                return View();
            }

            var result = await _userManager.AddToRoleAsync(user, roleName);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                return View();
            }
        }


    }
}
