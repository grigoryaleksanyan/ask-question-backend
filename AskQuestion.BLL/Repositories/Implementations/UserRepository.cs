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
                .FirstOrDefaultAsync(user => user.Login == userAuthDto.Login);

            if (user == null)
            {
                return null;
            }

            if (!BCrypt.Net.BCrypt.Verify(userAuthDto.Password, user.Password))
            {
                return null;
            }

            UserDto userDto = new()
            {
                Id = user.Id,
                Login = user.Login,
                UserRoleId = (Core.Enums.UserRoles)user.UserRoleId,
                Created = user.Created,
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
                Login = user.Login,
                UserRoleId = (Core.Enums.UserRoles)user.UserRoleId,
                UserDetails = user.UserDetails is not null ? new UserDetailsDto
                {
                    Id = user.UserDetails.Id,
                    FullName = user.UserDetails.FullName,
                    Email = user.UserDetails.Email,
                    AdditionalInfo = user.UserDetails.AdditionalInfo,
                    Created = user.UserDetails.Created,
                    Updated = user.UserDetails.Updated,
                } : null,
                Created = user.Created,
                Updated = user.Updated,
            };

            return userDto;
        }

        public async Task<UserDto?> CreateSpeaker(UserCreateDto userCreateDto)
        {
            var isNotUniqueLogin = dataContext.Users
                .Any(u => u.Login.Equals(userCreateDto.Login));

            if (isNotUniqueLogin)
            {
                throw new InvalidOperationException("Пользователь с таким логином уже существует");
            }

            var isNotUniqueEmail = dataContext.UserDetails
                .Any(ud => ud.Email.Equals(userCreateDto.UserDetails.Email));

            if (isNotUniqueEmail)
            {
                throw new InvalidOperationException("Пользователь с таким Email уже существует");
            }

            User user = new()
            {
                Login = userCreateDto.Login,
                Password = BCrypt.Net.BCrypt.HashPassword(userCreateDto.Password),
                UserRoleId = (int)Core.Enums.UserRoles.Speaker,
                Created = DateTimeOffset.UtcNow,
            };

            using (var transaction = await dataContext.Database.BeginTransactionAsync())
            {
                try
                {
                    await dataContext.Users.AddAsync(user);
                    await dataContext.SaveChangesAsync();

                    UserDetails userDetails = new()
                    {
                        UserId = user.Id,
                        Email = userCreateDto.UserDetails.Email,
                        FullName = userCreateDto.UserDetails.FullName,
                        AdditionalInfo = userCreateDto.UserDetails.AdditionalInfo,
                        Created = DateTimeOffset.UtcNow,
                    };

                    await dataContext.UserDetails.AddAsync(userDetails);
                    await dataContext.SaveChangesAsync();

                    await transaction.CommitAsync();

                    UserDto userDto = new()
                    {
                        Id = user.Id,
                        Login = user.Login,
                        UserRoleId = (Core.Enums.UserRoles)user.UserRoleId,
                        UserDetails = new UserDetailsDto
                        {
                            Id = userDetails.Id,
                            FullName = userDetails.FullName,
                            Email = userDetails.Email,
                            AdditionalInfo = userDetails.AdditionalInfo
                        },
                        Created = user.Created,
                        Updated = user.Updated,
                    };

                    return userDto;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
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

        public async Task<IEnumerable<SpeakerDto>> GetSpeakersAsync()
        {
            int speakerRoleId = (int)Core.Enums.UserRoles.Speaker;

            List<SpeakerDto> speakers = await dataContext.Users
                .AsNoTracking()
                .Include(u => u.UserDetails)
                .Where(u => u.UserRoleId == speakerRoleId)
                .Select(u => new SpeakerDto
                {
                    Id = u.Id,
                    FullName = u.UserDetails != null ? u.UserDetails.FullName : u.Login,
                })
                .ToListAsync();

            return speakers;
        }
    }
}
