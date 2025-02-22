using FormApp.DTO;
using FormApp.Models;
using FormApp.Repositories;
using Microsoft.AspNetCore.Mvc;

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
            var isAdmin = HttpContext.Session.GetInt32("UserId") != null &&
                         HttpContext.Session.GetString("IsAdmin") == "true";
            if (!isAdmin)
            {
                return RedirectToAction("Login", "User");
            }
            return View();
        }

        [HttpGet]
        public async Task<JsonResult> GetUsers()
        {
            var users = await _adminRepository.GetAllUsersAsync();
            return Json(users);
        }

        [HttpPost]
        public async Task<JsonResult> ToggleBlockUser(int userId)
        {
            var success = await _adminRepository.ToggleUserBlockStatusAsync(userId);
            return Json(new
            {
                success,
                message = success ? "User status updated" : "User not found"
            });
        }

        [HttpPost]
        public async Task<JsonResult> DeleteUser(int userId)
        {
            var success = await _adminRepository.DeleteUserAsync(userId);
            return Json(new
            {
                success,
                message = success ? "User deleted successfully" : "User not found"
            });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleAdminStatus(int userId)
        {
            var success = await _adminRepository.ToggleAdminStatusAsync(userId);
            return Json(new
            {
                success,
                message = success ? "Status updated" : "User not found"
            });
        }

        [HttpPost]
        public async Task<IActionResult> SaveAnswers([FromBody] List<Answer> answers)
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

            // Bulk save answers
            var success = await _adminRepository.SaveAnswersBulkAsync(answers, userId.Value);
            return Json(new
            {
                success,
                message = success ? "Answers saved successfully" : "Failed to save answers"
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetFormAnswers()
        {
            var answers = await _adminRepository.GetAnswersWithDetailsAsync();
            return Json(answers);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleLike([FromBody] ToggleLikeRequest request)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(new { success = false, message = "User not logged in" });
            }

            try
            {
                var result = await _adminRepository.ToggleLikeAsync(request.TemplateId, userId.Value);
                return Json(new
                {
                    success = true,
                    message = result.wasAdded ? "Like added" : "Like removed",
                    likeCount = result.totalLikes
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error processing like: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetLikeCount(int templateId)
        {
            var count = await _adminRepository.GetLikeCountAsync(templateId);
            return Json(new { count });
        }

        [HttpGet]
        public async Task<JsonResult> GetComments(int templateId)
        {
            try
            {
                var comments = await _adminRepository.GetCommentsAsync(templateId);
                return Json(comments);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Failed to load comments" });
            }
        }

        [HttpPost]
        public async Task<JsonResult> AddComment([FromBody] CommentRequestDto request)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(new { success = false, message = "User not logged in" });
            }

            if (string.IsNullOrEmpty(request.Text))
            {
                return Json(new { success = false, message = "Comment text is required" });
            }

            var comment = new Comment
            {
                Text = request.Text,
                UserId = userId.Value,
                TemplateId = request.TemplateId,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _adminRepository.AddCommentAsync(comment);
            return Json(new
            {
                success = result.Success,
                message = result.Success ? "Comment added successfully" : "Failed to save comment",
                comment = result.Comment
            });
        }

        [HttpGet]
        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return Json(new { success = false, message = "Search query is required" });
            }

            try
            {
                var results = await _adminRepository.SearchAsync(query);
                return Json(new { success = true, results });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }

    public class ToggleLikeRequest
    {
        public int TemplateId { get; set; }
    }
}