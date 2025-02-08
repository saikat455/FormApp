namespace FormApp.Models
{
    public class Answer
    {
        public int Id { get; set; }
        public string Value { get; set; }
        public int QuestionId { get; set; }
        public Question Question { get; set; }
        public int UserId { get; set; }  // Storing user ID who submits the answer
        public User User { get; set; }
    }
}