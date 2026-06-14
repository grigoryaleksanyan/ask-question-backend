namespace AskQuestion.BLL.DTO.User
{
    public class UserDetailsDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? Position { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset? Updated { get; set; }

        public string FullName => string.Join(" ", new[] { LastName, FirstName }.Where(s => !string.IsNullOrEmpty(s)));
    }
}
