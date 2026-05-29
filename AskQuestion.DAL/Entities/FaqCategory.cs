namespace AskQuestion.DAL.Entities
{
    public class FaqCategory : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public int Order { get; set; }

        #region Навигационные свойства

        public IEnumerable<FaqEntry> FaqEntries { get; set; }

        #endregion
    }
}
