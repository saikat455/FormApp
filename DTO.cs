using FormApp.Models;

namespace FormApp.DTO
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsBlocked { get; set; }
    }

    public class AnswerDetailsDto
    {
        public string TemplateTitle { get; set; }
        public string QuestionTitle { get; set; }
        public string Answer { get; set; }
        public string Username { get; set; }
    }

    public class CommentDto
    {
        public string Text { get; set; }
        public string UserEmail { get; set; }
        public string CreatedAt { get; set; }
    }

    public class CommentRequestDto
    {
        public string Text { get; set; }
        public int TemplateId { get; set; }
    }
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
        public int LikeCount { get; set; }
        public bool IsLiked { get; set; }
    }

    public class QuestionDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public bool ShowInTable { get; set; }
    }

    public class LoginViewModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
    public class SearchResultDto
    {
        public string Type { get; set; } // Template, Form, or Comment
        public string Title { get; set; }
        public string Description { get; set; }
        public string UserEmail { get; set; } // For comments
    }

    public class SaveAnswersRequest
    {
        public List<Answer> Answers { get; set; }
        public bool SendEmail { get; set; }
        public int TemplateId { get; set; }
    }
}


