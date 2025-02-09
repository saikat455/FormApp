namespace FormApp.Models
{
    public class Question
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Type { get; set; } // e.g., "SingleLine", "MultiLine", "Integer", "Checkbox"
        public bool ShowInTable { get; set; }
        public int TemplateId { get; set; }
        public Template Template { get; set; }
        public List<string> Options { get; set; } = new List<string>();
    }
}