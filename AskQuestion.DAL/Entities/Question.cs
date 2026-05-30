namespace AskQuestion.DAL.Entities
{
    public class Question : BaseEntity
    {
        public string Text { get; set; } = string.Empty;
        public string? Author { get; set; }
        public Guid? AreaId { get; set; }
        public Guid? SpeakerId { get; set; }
        public int Views { get; set; }
        public int Likes { get; set; }
        public int Dislikes { get; set; }
        public int Status { get; set; } = 0;
        public DateTimeOffset? Answered { get; set; }

        #region Навигационные свойства

        public User? SpeakerUser { get; set; }
        public Area? AreaEntity { get; set; }

        #endregion
    }
}
