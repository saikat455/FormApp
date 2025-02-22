using FormApp.Data;
using FormApp.DTO;
using FormApp.Models;
using Microsoft.EntityFrameworkCore;

namespace FormApp.Repositories
{
    public class AdminRepository : IAdminRepository
    {
        private readonly ApplicationDbContext _context;

        public AdminRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            return await _context.Users
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    IsAdmin = u.IsAdmin,
                    IsBlocked = u.IsBlocked
                })
                .ToListAsync();
        }

        public async Task<bool> ToggleUserBlockStatusAsync(int userId)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return false;

            user.IsBlocked = !user.IsBlocked;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleAdminStatusAsync(int userId)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return false;

            user.IsAdmin = !user.IsAdmin;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SaveAnswersBulkAsync(List<Answer> answers, int userId)
        {
            // Set UserId for all answers in memory
            foreach (var answer in answers)
            {
                answer.UserId = userId;
            }

            // Bulk insert
            await _context.Answers.AddRangeAsync(answers);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<AnswerDetailsDto>> GetAnswersWithDetailsAsync()
        {
            return await _context.Answers
                .Select(a => new AnswerDetailsDto
                {
                    TemplateTitle = a.Question.Template.Title,
                    QuestionTitle = a.Question.Title,
                    Answer = a.Value,
                    Username = a.User.Email
                })
                .ToListAsync();
        }

        public async Task<(bool wasAdded, int totalLikes)> ToggleLikeAsync(int templateId, int userId)
        {
            var existingLike = await _context.Likes
                .FirstOrDefaultAsync(l => l.TemplateId == templateId && l.UserId == userId);

            if (existingLike == null)
            {
                await _context.Likes.AddAsync(new Like
                {
                    UserId = userId,
                    TemplateId = templateId
                });
                await _context.SaveChangesAsync();
                return (true, await GetLikeCountAsync(templateId));
            }

            _context.Likes.Remove(existingLike);
            await _context.SaveChangesAsync();
            return (false, await GetLikeCountAsync(templateId));
        }

        public async Task<int> GetLikeCountAsync(int templateId)
        {
            return await _context.Likes.CountAsync(l => l.TemplateId == templateId);
        }

        public async Task<List<CommentDto>> GetCommentsAsync(int templateId)
        {
            return await _context.Comments
                .Where(c => c.TemplateId == templateId)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new CommentDto
                {
                    Text = c.Text,
                    UserEmail = c.User.Email,
                    CreatedAt = c.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
                })
                .ToListAsync();
        }

        public async Task<(bool Success, CommentDto Comment)> AddCommentAsync(Comment comment)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Id == comment.UserId);
            var templateExists = await _context.Templates.AnyAsync(t => t.Id == comment.TemplateId);

            if (!userExists || !templateExists)
            {
                return (false, null);
            }

            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();

            var commentDto = new CommentDto
            {
                Text = comment.Text,
                UserEmail = (await _context.Users.FirstAsync(u => u.Id == comment.UserId)).Email,
                CreatedAt = comment.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
            };

            return (true, commentDto);
        }
        public async Task<List<SearchResultDto>> SearchAsync(string query)
        {
            var results = new List<SearchResultDto>();

            // Search in templates
            var templateResults = await _context.Templates
                .Where(t => t.Title.Contains(query) || t.Description.Contains(query))
                .Select(t => new SearchResultDto
                {
                    Type = "Template",
                    Title = t.Title,
                    Description = t.Description
                })
                .ToListAsync();
            results.AddRange(templateResults);

            // Search in forms (answers)
            var formResults = await _context.Answers
                .Where(a => a.Value.Contains(query))
                .Select(a => new SearchResultDto
                {
                    Type = "Form",
                    Title = a.Question.Title,
                    Description = a.Value
                })
                .ToListAsync();
            results.AddRange(formResults);

            // Search in comments
            var commentResults = await _context.Comments
                .Where(c => c.Text.Contains(query))
                .Select(c => new SearchResultDto
                {
                    Type = "Comment",
                    Title = "Comment",
                    Description = c.Text,
                    UserEmail = c.User.Email
                })
                .ToListAsync();
            results.AddRange(commentResults);

            // Search in questions
            var questionResults = await _context.Questions
                .Where(q => q.Title.Contains(query) || q.Description.Contains(query))
                .Select(q => new SearchResultDto
                {
                    Type = "Question",
                    Title = q.Title,
                    Description = q.Description,
                    TemplateTitle = q.Template.Title
                })
                .ToListAsync();
            results.AddRange(questionResults);

            return results;
        }
    }
}