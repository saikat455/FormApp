using FormApp.Models;

namespace FormApp.Repositories
{
    public interface IUserRepository
    {
        User GetByUsername(string username);
        User GetByEmail(string email);
        void Add(User user);
        void SaveChanges();
        List<Template> GetTemplatesByUserId(int userId);
        Template GetTemplateById(int id);
        void AddTemplate(Template template);
        void UpdateTemplate(Template template);
        void DeleteTemplate(int id);
        List<Template> GetAllTemplates();
        (bool isLiked, int likeCount) GetTemplateLikeStatus(int templateId, int userId);
        (bool isLiked, int likeCount) ToggleLike(int templateId, int userId);
    }
}