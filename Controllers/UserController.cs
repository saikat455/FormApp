using FormApp.DTO;
using FormApp.Helpers;
using FormApp.Models;
using FormApp.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace FormApp.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserRepository _userRepository;

        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register([FromBody] User model)
        {
            // Validate password on server side
            if (!IsPasswordValid(model.PasswordHash))
            {
                return Json(new { success = false, message = "Password must contain at least one number, one uppercase letter, one lowercase letter, and be at least 8 characters long." });
            }

            if (_userRepository.GetByEmail(model.Email) != null)
            {
                return Json(new { success = false, message = "Email already exists" });
            }

            if (_userRepository.GetByUsername(model.Username) != null)
            {
                return Json(new { success = false, message = "Username already exists" });
            }

            model.PasswordHash = HashPassword(model.PasswordHash);
            model.IsAdmin = false;
            model.IsBlocked = false;

            _userRepository.Add(model);
            _userRepository.SaveChanges();

            return Json(new { success = true, message = "Registration successful!" });
        }

        // Function to validate password
        private bool IsPasswordValid(string password)
        {
            const int minLength = 8;
            bool hasNumber = password.Any(char.IsDigit);
            bool hasUpperCase = password.Any(char.IsUpper);
            bool hasLowerCase = password.Any(char.IsLower);

            return password.Length >= minLength && hasNumber && hasUpperCase && hasLowerCase;
        }


        [HttpPost]
        public IActionResult Login([FromBody] LoginViewModel model)
        {
            var user = _userRepository.GetByEmail(model.Email);

            if (user == null || !VerifyPassword(model.Password, user.PasswordHash))
            {
                return Json(new { success = false, message = "Invalid email or password" });
            }

            if (user.IsBlocked)
            {
                return Json(new { success = false, message = "You're blocked!!!" });
            }

            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Email", user.Email);
            HttpContext.Session.SetString("IsAdmin", user.IsAdmin ? "true" : "false");

            return Json(new { success = true, message = "Login successful!", redirectUrl = user.IsAdmin ? "/Admin/Dashboard" : "/User/Profile" });
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }

        [HttpGet]
        public IActionResult Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login");
            }
            return View();
        }

        [HttpGet]
        public JsonResult GetTemplates()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var isAdmin = HttpContext.Session.GetString("IsAdmin") == "true";

            var templates = isAdmin
                ? _userRepository.GetAllTemplates()
                : _userRepository.GetTemplatesByUserId(userId.Value);

            var templateDtos = templates.Select(t => new TemplateDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Topic = t.Topic,
                ImageUrl = t.ImageUrl,
                IsPublic = t.IsPublic,
                CreatedBy = t.User?.Username ?? "Unknown",
                CanEdit = isAdmin || t.UserId == userId,
                Questions = t.Questions.Select(q => new QuestionDto
                {
                    Id = q.Id,
                    Title = q.Title,
                    Description = q.Description,
                    Type = q.Type,
                    ShowInTable = q.ShowInTable
                }).ToList()
            }).ToList();

            return Json(templateDtos);
        }

        [HttpGet]
        public JsonResult GetTemplate(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var isAdmin = HttpContext.Session.GetString("IsAdmin") == "true";

            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "Not authenticated" });
            }

            var template = _userRepository.GetTemplateById(id);
            if (template == null || (!isAdmin && template.UserId != userId.Value))
            {
                return Json(new { success = false, message = "Template not found" });
            }

            var templateDto = new TemplateDto
            {
                Id = template.Id,
                Title = template.Title,
                Description = template.Description,
                Topic = template.Topic,
                ImageUrl = template.ImageUrl,
                IsPublic = template.IsPublic,
                CreatedBy = template.User?.Username ?? "Unknown",
                CanEdit = isAdmin || template.UserId == userId,
                Questions = template.Questions.Select(q => new QuestionDto
                {
                    Id = q.Id,
                    Title = q.Title,
                    Description = q.Description,
                    Type = q.Type,
                    ShowInTable = q.ShowInTable
                }).ToList()
            };

            return Json(templateDto);
        }

        [HttpPost]
        public JsonResult SaveTemplate([FromBody] Template template)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var isAdmin = HttpContext.Session.GetString("IsAdmin") == "true";

            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "Not authenticated" });
            }

            if (template.Id == 0)
            {
                template.UserId = userId.Value;
                _userRepository.AddTemplate(template);
            }
            else
            {
                var existingTemplate = _userRepository.GetTemplateById(template.Id);
                if (existingTemplate == null || (!isAdmin && existingTemplate.UserId != userId))
                {
                    return Json(new { success = false, message = "Template not found or unauthorized" });
                }
                _userRepository.UpdateTemplate(template);
            }

            return Json(new { success = true, message = "Template saved successfully" });
        }

        [HttpPost]
        public JsonResult DeleteTemplate(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var isAdmin = HttpContext.Session.GetString("IsAdmin") == "true";

            var template = _userRepository.GetTemplateById(id);
            if (template == null || (!isAdmin && template.UserId != userId))
            {
                return Json(new { success = false, message = "Template not found or unauthorized" });
            }

            _userRepository.DeleteTemplate(id);
            return Json(new { success = true, message = "Template deleted successfully" });
        }

        [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return Json(new { success = false, message = "No file uploaded" });
                }

                var imageUrl = await CloudinaryHelper.UploadImage(file);
                return Json(new { success = true, imageUrl = imageUrl });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Failed to upload image" });
            }
        }

        [HttpPost]
        public JsonResult ToggleLike(int templateId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "Not authenticated" });
            }

            try
            {
                var result = _userRepository.ToggleLike(templateId, userId.Value);
                return Json(new
                {
                    success = true,
                    isLiked = result.isLiked,
                    likeCount = result.likeCount,
                    message = result.isLiked ? "Template liked" : "Template unliked"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error toggling like" });
            }
        }

        [HttpGet]
        public JsonResult GetTemplateLikeStatus(int templateId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "Not authenticated" });
            }

            var result = _userRepository.GetTemplateLikeStatus(templateId, userId.Value);
            return Json(new
            {
                success = true,
                isLiked = result.isLiked,
                likeCount = result.likeCount
            });
        }

        [HttpGet]
        public IActionResult ViewTemplateQuestions(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var isAdmin = HttpContext.Session.GetString("IsAdmin") == "true";

            if (!userId.HasValue)
            {
                return RedirectToAction("Login");
            }

            var template = _userRepository.GetTemplateById(id);
            if (template == null || (!isAdmin && template.UserId != userId && !template.IsPublic))
            {
                return NotFound();
            }

            // Filter questions for non-admin and non-creator users
            if (!isAdmin && template.UserId != userId)
            {
                template.Questions = template.Questions
                    .Where(q => q.ShowInTable) // Only include questions with ShowInTable = true
                    .ToList();
            }

            return View(template);
        }
    }

    public class LoginViewModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}