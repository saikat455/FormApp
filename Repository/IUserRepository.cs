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
        List<Form> GetFormsByUserId(int userId);
        Template GetTemplateById(int id);
        Form GetFormById(int id);
        void AddTemplate(Template template);
        void UpdateTemplate(Template template);
        void DeleteTemplate(int id);
        void DeleteForm(int id);
    }
}
