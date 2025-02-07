namespace FormApp.Models
{
    public class Form
    {
        public int Id { get; set; }
        public int TemplateId { get; set; }
        public Template Template { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<Answer> Answers { get; set; } = new();
    }
}