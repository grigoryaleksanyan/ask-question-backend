namespace AskQuestion.BLL.DTO.User
{
    public class UserDetailsDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? Patronymic { get; set; }
        public string? Position { get; set; }
        public string Email { get; set; } = null!;
        public string? AdditionalInfo { get; set; }
        public bool IsDeleted { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset? Updated { get; set; }

        public string FullName => string.Join(" ", new[] { LastName, FirstName, Patronymic }.Where(s => !string.IsNullOrEmpty(s)));
    }
}
