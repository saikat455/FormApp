using FormApp.Data;
using FormApp.DTO;
using FormApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace FormApp.Repositories
{
    public class AdminRepository : IAdminRepository
    {
        private readonly ApplicationDbContext _context;

        public AdminRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public void SaveChanges()
        {
            _context.SaveChanges(); 
        }
        public List<User> GetAllUsers()
        {
            return _context.Users.ToList();
        }

        public User GetUserById(int userId)
        {
            return _context.Users.FirstOrDefault(u => u.Id == userId);
        }

        public void UpdateUser(User user)
        {
            _context.Users.Update(user);
            _context.SaveChanges();
        }

        public void DeleteUser(int userId)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }
        }

        public bool ToggleAdminStatus(int userId)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user != null)
            {
                user.IsAdmin = !user.IsAdmin; // Toggle the admin status
                _context.SaveChanges(); // Save the change to the database
                return true;
            }
            return false;
        }

        public void SaveAnswer(Answer answer)
        {
            _context.Answers.Add(answer);
            _context.SaveChanges();
        }

        public List<Answer> GetAllAnswersWithDetails()
        {
            return _context.Answers
                .Include(a => a.Question)
                .ThenInclude(q => q.Template)
                .Include(a => a.User)
                .ToList();
        }

        public async Task<(bool wasAdded, int totalLikes)> ToggleLike(int templateId, int userId)
        {
            try
            {
                // Check if template exists
                var template = await _context.Templates.FindAsync(templateId);
                if (template == null)
                {
                    throw new Exception("Template not found");
                }

                var existingLike = await _context.Likes
                    .FirstOrDefaultAsync(l => l.TemplateId == templateId && l.UserId == userId);

                if (existingLike == null)
                {
                    // Add new like
                    var like = new Like
                    {
                        UserId = userId,
                        TemplateId = templateId
                    };
                    _context.Likes.Add(like);
                    await _context.SaveChangesAsync();
                    return (true, await GetLikeCount(templateId));
                }
                else
                {
                    // Remove existing like
                    _context.Likes.Remove(existingLike);
                    await _context.SaveChangesAsync();
                    return (false, await GetLikeCount(templateId));
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error toggling like: {ex.Message}");
            }
        }

        public async Task<int> GetLikeCount(int templateId)
        {
            return await _context.Likes.CountAsync(l => l.TemplateId == templateId);
        }

        public async Task<bool> HasUserLiked(int templateId, int userId)
        {
            return await _context.Likes.AnyAsync(l => l.TemplateId == templateId && l.UserId == userId);
        }
    }
}
