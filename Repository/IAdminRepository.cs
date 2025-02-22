using FormApp.DTO;
using FormApp.Models;
using System.Collections.Generic;

namespace FormApp.Repositories
{
    public interface IAdminRepository
    {
        Task<List<UserDto>> GetAllUsersAsync();
        Task<bool> ToggleUserBlockStatusAsync(int userId);
        Task<bool> DeleteUserAsync(int userId);
        Task<bool> ToggleAdminStatusAsync(int userId);
        Task<bool> SaveAnswersBulkAsync(List<Answer> answers, int userId);
        Task<List<AnswerDetailsDto>> GetAnswersWithDetailsAsync();
        Task<(bool wasAdded, int totalLikes)> ToggleLikeAsync(int templateId, int userId);
        Task<int> GetLikeCountAsync(int templateId);
        Task<List<CommentDto>> GetCommentsAsync(int templateId);
        Task<(bool Success, CommentDto Comment)> AddCommentAsync(Comment comment);
        Task<List<SearchResultDto>> SearchAsync(string query);
    }
}

