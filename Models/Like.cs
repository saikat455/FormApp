namespace FormApp.Models
{
    public class Like
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int TemplateId { get; set; }
        public Template Template { get; set; }
    }
}
