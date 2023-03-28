using AskQuestion.BLL.DTO.User;

namespace AskQuestion.BLL.Repositories.Interfaces
{
    public interface IUserRepository
    {
       Task<UserDto?> AuthorizeUser(UserAuthDto userAuthDto);
       Task<UserDto?> GetById(Guid id);
    }
}
