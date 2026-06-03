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

        public async Task<bool> IsAdminExistsAsync()
        {
            int adminRoleId = (int)Core.Enums.UserRoles.Administrator;

            return await dataContext.Users
                .AnyAsync(u => u.UserRoleId == adminRoleId);
        }

        public async Task<UserDto> SetupAdminAsync(AdminSetupDto dto)
        {
            int adminRoleId = (int)Core.Enums.UserRoles.Administrator;

            bool adminExists = await dataContext.Users
                .AnyAsync(u => u.UserRoleId == adminRoleId);

            if (adminExists)
            {
                throw new InvalidOperationException("Администратор уже существует");
            }

            var isNotUniqueEmail = dataContext.Users
                .Any(u => u.Email.Equals(dto.Email));

            if (isNotUniqueEmail)
            {
                throw new InvalidOperationException("Пользователь с таким Email уже существует");
            }

            Guid userId = Guid.NewGuid();

            User user = new()
            {
                Id = userId,
                Email = dto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                UserRoleId = adminRoleId,
                Created = DateTimeOffset.UtcNow,
            };

            UserDetails userDetails = new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Patronymic = dto.Patronymic,
                IsDeleted = false,
                Created = DateTimeOffset.UtcNow,
            };

            await dataContext.Users.AddAsync(user);
            await dataContext.UserDetails.AddAsync(userDetails);
            await dataContext.SaveChangesAsync();

            UserDto userDto = new()
            {
                Id = user.Id,
                Email = user.Email,
                UserRoleId = (Core.Enums.UserRoles)user.UserRoleId,
                UserDetails = new UserDetailsDto
                {
                    Id = userDetails.Id,
                    FirstName = userDetails.FirstName,
                    LastName = userDetails.LastName,
                    Patronymic = userDetails.Patronymic,
                    Position = userDetails.Position,
                    AdditionalInfo = userDetails.AdditionalInfo,
                    IsDeleted = userDetails.IsDeleted,
                    Created = userDetails.Created,
                    Updated = userDetails.Updated,
                },
                Created = user.Created,
                Updated = user.Updated,
            };

            return userDto;
        }
    }
}
