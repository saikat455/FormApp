using FormApp.DTO;

namespace FormApp.DTO
{

    public class TemplateDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Topic { get; set; }
        public string ImageUrl { get; set; }
        public bool IsPublic { get; set; }
        public bool CanEdit { get; set; }
        public string CreatedBy { get; set; }
        public List<QuestionDto> Questions { get; set; }
    }
}
