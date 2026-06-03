using AskQuestion.BLL.DTO.User;
using AskQuestion.BLL.Repositories.Interfaces;
using AskQuestion.DAL;
using AskQuestion.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace AskQuestion.BLL.Repositories.Implementations
{
    public class UserRepository(DataContext dataContext) : IUserRepository
    {
        public async Task<UserDto?> AuthorizeUser(UserAuthDto userAuthDto)
        {
            User? user = await dataContext.Users
                .AsNoTracking()
                .Include(u => u.UserDetails)
                .FirstOrDefaultAsync(user => user.Email == userAuthDto.Email);

            if (user == null)
            {
                return null;
            }

            if (!BCrypt.Net.BCrypt.Verify(userAuthDto.Password, user.Password))
            {
                return null;
            }

            if (user.UserDetails is not null && user.UserDetails.IsDeleted)
            {
                return null;
            }

            UserDto userDto = new()
            {
                Id = user.Id,
                Email = user.Email,
                UserRoleId = (Core.Enums.UserRoles)user.UserRoleId,
                UserDetails = user.UserDetails is not null ? new UserDetailsDto
                {
                    Id = user.UserDetails.Id,
                    FirstName = user.UserDetails.FirstName,
                    LastName = user.UserDetails.LastName,
                    Patronymic = user.UserDetails.Patronymic,
                    Position = user.UserDetails.Position,
                    AdditionalInfo = user.UserDetails.AdditionalInfo,
                    IsDeleted = user.UserDetails.IsDeleted,
                    Created = user.UserDetails.Created,
                    Updated = user.UserDetails.Updated,
                } : null,
                Created = user.Created,
                Updated = user.Updated,
            };

            return userDto;
        }

        public async Task<UserDto?> GetById(Guid id)
        {
            User? user = await dataContext.Users
                .AsNoTracking()
                .Include(u => u.UserDetails)
                .FirstOrDefaultAsync(user => user.Id == id);

            if (user == null)
            {
                return null;
            }

            UserDto userDto = new()
            {
                Id = user.Id,
                Email = user.Email,
                UserRoleId = (Core.Enums.UserRoles)user.UserRoleId,
                UserDetails = user.UserDetails is not null ? new UserDetailsDto
                {
                    Id = user.UserDetails.Id,
                    FirstName = user.UserDetails.FirstName,
                    LastName = user.UserDetails.LastName,
                    Patronymic = user.UserDetails.Patronymic,
                    Position = user.UserDetails.Position,
                    AdditionalInfo = user.UserDetails.AdditionalInfo,
                    IsDeleted = user.UserDetails.IsDeleted,
                    Created = user.UserDetails.Created,
                    Updated = user.UserDetails.Updated,
                } : null,
                Created = user.Created,
                Updated = user.Updated,
            };

            return userDto;
        }

        public async Task ChangePassword(UserPasswordUpdateDto userPasswordUpdateDto)
        {
            User? user = await dataContext.Users
                .FirstOrDefaultAsync(user => user.Id == userPasswordUpdateDto.Id);

            if (user == null)
            {
                throw new InvalidOperationException("Ошибка изменения пароля");
            }

            if (!BCrypt.Net.BCrypt.Verify(userPasswordUpdateDto.Password, user.Password))
            {
                throw new InvalidOperationException("Ошибка изменения пароля");
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(userPasswordUpdateDto.NewPassword);
            user.Updated = DateTimeOffset.UtcNow;

            await dataContext.SaveChangesAsync();
        }
    }
}
