using FormApp.DTO;
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
            var templates = _userRepository.GetTemplatesByUserId(userId.Value)
                .Select(t => new TemplateDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    Topic = t.Topic,
                    ImageUrl = t.ImageUrl,
                    IsPublic = t.IsPublic,
                    Questions = t.Questions.Select(q => new QuestionDto
                    {
                        Id = q.Id,
                        Title = q.Title,
                        Description = q.Description,
                        Type = q.Type,
                        ShowInTable = q.ShowInTable
                    }).ToList()
                }).ToList();

            return Json(templates);
        }

        [HttpGet]
        public JsonResult GetForms()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            return Json(_userRepository.GetFormsByUserId(userId.Value));
        }

        [HttpGet]
        public JsonResult GetTemplate(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var template = _userRepository.GetTemplateById(id);
            if (template?.UserId != userId)
            {
                return Json(new { success = false, message = "Template not found" });
            }
            return Json(template);
        }

        [HttpPost]
        public JsonResult SaveTemplate([FromBody] Template template)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "Not authenticated" });
            }

            template.UserId = userId.Value;
            if (template.Id == 0)
            {
                _userRepository.AddTemplate(template);
            }
            else
            {
                var existingTemplate = _userRepository.GetTemplateById(template.Id);
                if (existingTemplate?.UserId != userId)
                {
                    return Json(new { success = false, message = "Template not found" });
                }
                _userRepository.UpdateTemplate(template);
            }

            return Json(new { success = true, message = "Template saved successfully" });
        }

        [HttpPost]
        public JsonResult DeleteTemplate(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var template = _userRepository.GetTemplateById(id);
            if (template?.UserId != userId)
            {
                return Json(new { success = false, message = "Template not found" });
            }

            _userRepository.DeleteTemplate(id);
            return Json(new { success = true, message = "Template deleted successfully" });
        }

        [HttpPost]
        public JsonResult DeleteForm(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var form = _userRepository.GetFormById(id);
            if (form?.UserId != userId)
            {
                return Json(new { success = false, message = "Form not found" });
            }

            _userRepository.DeleteForm(id);
            return Json(new { success = true, message = "Form deleted successfully" });
        }

        [HttpGet]
        public JsonResult GetTemplateQuestions(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var template = _userRepository.GetTemplateById(id);
            if (template?.UserId != userId)
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


    }

    public class LoginViewModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}