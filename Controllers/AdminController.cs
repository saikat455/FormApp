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

        //[HttpPost]
        //public IActionResult SaveAnswers([FromBody] List<Answer> answers)
        //{
        //    if (answers == null || !answers.Any())
        //    {
        //        return Json(new { success = false, message = "No answers provided" });
        //    }

        //    foreach (var answer in answers)
        //    {
        //        _adminRepository.SaveAnswer(answer);
        //    }

        //    return Json(new { success = true, message = "Answers saved successfully" });
        //}

        [HttpPost]
        public IActionResult SaveAnswers([FromBody] List<Answer> answers)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return Json(new { success = false, message = "User not logged in" });
            }

            if (answers == null || !answers.Any())
            {
                return Json(new { success = false, message = "No answers provided" });
            }

            foreach (var answer in answers)
            {
                answer.UserId = userId.Value; // Assigning UserId
                _adminRepository.SaveAnswer(answer);
            }

            return Json(new { success = true, message = "Answers saved successfully" });
        }

        [HttpGet]
        public IActionResult GetFormAnswers()
        {
            var answers = _adminRepository.GetAllAnswersWithDetails();

            var response = answers.Select(a => new
            {
                TemplateTitle = a.Question.Template.Title,
                QuestionTitle = a.Question.Title,
                Answer = a.Value,
                Username = a.User.Email
            }).ToList();

            return Json(response);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleLike([FromBody] ToggleLikeRequest request)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    return Json(new { success = false, message = "User not logged in" });
                }

                var result = await _adminRepository.ToggleLike(request.TemplateId, userId.Value);
                return Json(new
                {
                    success = true,
                    message = result.wasAdded ? "Like added" : "Like removed",
                    likeCount = result.totalLikes
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error processing like: " + ex.Message });
            }
        }

        public class ToggleLikeRequest
        {
            public int TemplateId { get; set; }
        }

        [HttpGet]
        public async Task<IActionResult> GetLikeCount(int templateId)
        {
            var count = await _adminRepository.GetLikeCount(templateId);
            return Json(new { count = count });
        }

    }
}
