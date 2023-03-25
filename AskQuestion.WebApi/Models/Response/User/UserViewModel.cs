namespace AskQuestion.WebApi.Models.Response.User
{
    public class UserViewModel
    {
        public Guid Id { get; set; }
        public string Login { get; set; } = null!;
        public int UserRoleId { get; set; }
        public DateTimeOffset Сreated { get; set; }
        public DateTimeOffset? Updated { get; set; }
    }
}
