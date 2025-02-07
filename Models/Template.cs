namespace FormApp.Models
{
    public class Template
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Topic { get; set; }
        public string ImageUrl { get; set; }
        public bool IsPublic { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public List<Question> Questions { get; set; } = new();
        public List<Form> Forms { get; set; } = new();
    }
}