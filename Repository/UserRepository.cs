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

        //public List<Template> GetTemplatesByUserId(int userId)
        //{
        //    return _context.Templates
        //        .Include(t => t.Questions)
        //        .Where(t => t.UserId == userId || t.IsPublic)
        //        .ToList();
        //}

        public List<Template> GetTemplatesByUserId(int userId)
        {
            return _context.Templates
                .Include(t => t.Questions)
                .Include(t => t.Likes)
                .Where(t => t.UserId == userId || t.IsPublic)
                .ToList();
        }

        public Template GetTemplateById(int id)
        {
            return _context.Templates
                .Include(t => t.Questions)
                .FirstOrDefault(t => t.Id == id);
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

        public void DeleteTemplate(int id)
        {
            var template = _context.Templates
                .Include(t => t.Questions)
                .FirstOrDefault(t => t.Id == id);

            if (template != null)
            {
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
        public List<Template> GetAllTemplates()
        {
            return _context.Templates
                .Include(t => t.Questions)
                .Include(t => t.User)
                .Include(t => t.Likes)
                .ToList();
        }

        public (bool isLiked, int likeCount) ToggleLike(int templateId, int userId)
        {
            var existingLike = _context.Likes
                .FirstOrDefault(l => l.TemplateId == templateId && l.UserId == userId);

            if (existingLike == null)
            {
                // Add new like
                _context.Likes.Add(new Like
                {
                    TemplateId = templateId,
                    UserId = userId
                });
                _context.SaveChanges();
                var likeCount = _context.Likes.Count(l => l.TemplateId == templateId);
                return (true, likeCount);
            }
            else
            {
                // Remove existing like
                _context.Likes.Remove(existingLike);
                _context.SaveChanges();
                var likeCount = _context.Likes.Count(l => l.TemplateId == templateId);
                return (false, likeCount);
            }
        }

        public (bool isLiked, int likeCount) GetTemplateLikeStatus(int templateId, int userId)
        {
            var isLiked = _context.Likes.Any(l => l.TemplateId == templateId && l.UserId == userId);
            var likeCount = _context.Likes.Count(l => l.TemplateId == templateId);
            return (isLiked, likeCount);
        }
    }
}
