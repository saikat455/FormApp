using FormApp.Data;
using FormApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace FormApp.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public User GetByUsername(string username)
        {
            return _context.Users.FirstOrDefault(u => u.Username == username);
        }

        public User GetByEmail(string email)
        {
            return _context.Users.FirstOrDefault(u => u.Email == email);
        }

        public void Add(User user)
        {
            _context.Users.Add(user);
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        public List<Template> GetTemplatesByUserId(int userId)
        {
            return _context.Templates
                .Include(t => t.Questions)
                .Where(t => t.UserId == userId)
                .ToList();
        }

        public Template GetTemplateById(int id)
        {
            return _context.Templates
                .Include(t => t.Questions)
                .FirstOrDefault(t => t.Id == id);
        }

        public Form GetFormById(int id)
        {
            return _context.Forms
                .Include(f => f.Answers)
                .FirstOrDefault(f => f.Id == id);
        }

        public void AddTemplate(Template template)
        {
            _context.Templates.Add(template);
            _context.SaveChanges();
        }

        public void UpdateTemplate(Template template)
        {
            var existing = _context.Templates
                .Include(t => t.Questions)
                .FirstOrDefault(t => t.Id == template.Id);

            if (existing != null)
            {
                _context.Questions.RemoveRange(existing.Questions);
                existing.Title = template.Title;
                existing.Description = template.Description;
                existing.Topic = template.Topic;
                existing.ImageUrl = template.ImageUrl;
                existing.IsPublic = template.IsPublic;
                existing.Questions = template.Questions;
                _context.SaveChanges();
            }
        }

        // UserRepository.cs (completing the methods)

        public void DeleteTemplate(int id)
        {
            var template = _context.Templates
                .Include(t => t.Questions)
                .Include(t => t.Forms)
                    .ThenInclude(f => f.Answers)
                .FirstOrDefault(t => t.Id == id);

            if (template != null)
            {
                // Delete related answers and forms only if there are any
                if (template.Forms != null && template.Forms.Any())
                {
                    foreach (var form in template.Forms)
                    {
                        if (form.Answers != null && form.Answers.Any())
                        {
                            _context.Answers.RemoveRange(form.Answers);
                        }
                    }
                    _context.Forms.RemoveRange(template.Forms);
                }

                // Delete related questions if there are any
                if (template.Questions != null && template.Questions.Any())
                {
                    _context.Questions.RemoveRange(template.Questions);
                }

                // Delete the template
                _context.Templates.Remove(template);

                _context.SaveChanges();
            }
        }

        public void DeleteForm(int id)
        {
            var form = _context.Forms
                .Include(f => f.Answers)
                .FirstOrDefault(f => f.Id == id);

            if (form != null)
            {
                // Delete related answers
                _context.Answers.RemoveRange(form.Answers);

                // Delete the form
                _context.Forms.Remove(form);

                _context.SaveChanges();
            }
        }

        public List<Template> GetAllTemplates()
        {
            return _context.Templates
                .Include(t => t.Questions)
                .Include(t => t.User)
                .ToList();
        }

    }
}
