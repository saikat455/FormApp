using FormApp.Models;
using System.Collections.Generic;

namespace FormApp.Repositories
{
    public interface IAdminRepository
    {
        List<User> GetAllUsers();
        void UpdateUser(User user);
        void DeleteUser(int userId);
        User GetUserById(int userId);
        void SaveChanges();
        bool ToggleAdminStatus(int userId);
        void SaveAnswer(Answer answer);
        List<Answer> GetAllAnswersWithDetails();
        Task<(bool wasAdded, int totalLikes)> ToggleLike(int templateId, int userId);
        Task<int> GetLikeCount(int templateId);
        Task<bool> HasUserLiked(int templateId, int userId);
    }
}
