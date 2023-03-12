namespace AskQuestion.DAL.Entities
{
    public class FaqEntry : BaseEntity
    {
        public Guid FaqCategoryId { get; set; }
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public int Order { get; set; }

        #region Навигационные свойства

        public FaqCategory FaqCategory { get; set; }

        #endregion
    }
}
