using AskQuestion.BLL.DTO.User;

namespace AskQuestion.BLL.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<UserDto?> AuthorizeUser(UserAuthDto userAuthDto);
        Task<UserDto?> GetById(Guid id);
        Task ChangePassword(UserPasswordUpdateDto userPasswordUpdateDto);
        Task ForgotPasswordAsync(ForgotPasswordDto dto);
        Task ResetPasswordAsync(ResetPasswordDto dto);
        Task<bool> IsAdminExistsAsync();
        Task<UserDto> SetupAdminAsync(AdminSetupDto dto);
        Task SetActiveAsync(Guid id, bool isActive, Core.Enums.UserRoles role);
    }
}
