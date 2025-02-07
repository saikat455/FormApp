using FormApp.Models;
using FormApp.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace FormApp.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAdminRepository _adminRepository;

        public AdminController(IAdminRepository adminRepository)
        {
            _adminRepository = adminRepository;
        }

        public IActionResult Dashboard()
        {
            var isAdmin = HttpContext.Session.GetInt32("UserId") != null && HttpContext.Session.GetString("IsAdmin") == "true";
            if (!isAdmin)
            {
                return RedirectToAction("Login", "User");
            }

            return View();
        }

        [HttpGet]
        public JsonResult GetUsers()
        {
            var users = _adminRepository.GetAllUsers();
            return Json(users);
        }

        [HttpPost]
        public JsonResult ToggleBlockUser(int userId)
        {
            var user = _adminRepository.GetUserById(userId);
            if (user != null)
            {
                user.IsBlocked = !user.IsBlocked;
                _adminRepository.UpdateUser(user);
                return Json(new { success = true, message = "User status updated" });
            }
            return Json(new { success = false, message = "User not found" });
        }

        [HttpPost]
        public JsonResult DeleteUser(int userId)
        {
            _adminRepository.DeleteUser(userId);
            return Json(new { success = true, message = "User deleted successfully" });
        }

        [HttpPost]
        public IActionResult ToggleAdminStatus(int userId)
        {
            var user = _adminRepository.GetUserById(userId);
            if (user != null)
            {
                user.IsAdmin = !user.IsAdmin; // Toggle IsAdmin
                _adminRepository.SaveChanges(); // Save changes to the database
                return Json(new { success = true, message = "Status updated" });
            }
            return Json(new { success = false, message = "User not found" });
        }
    }
}
