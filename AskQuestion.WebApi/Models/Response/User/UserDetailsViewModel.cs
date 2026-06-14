namespace AskQuestion.WebApi.Models.Response.User
{
    public class UserDetailsViewModel
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
