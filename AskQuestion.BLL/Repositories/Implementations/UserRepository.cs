using AskQuestion.BLL.Helpers;
using AskQuestion.BLL.DTO.User;
using AskQuestion.BLL.Email;
using AskQuestion.BLL.Repositories.Interfaces;
using AskQuestion.DAL;
using AskQuestion.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace AskQuestion.BLL.Repositories.Implementations
{
    public class UserRepository(
        DataContext dataContext,
        IHtmlSanitizerService htmlSanitizer,
        IEmailSender emailSender,
        IOptions<SmtpSettings> smtpSettings) : IUserRepository
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

            if (!user.IsActive)
            {
                return null;
            }

            UserDto userDto = new()
            {
                Id = user.Id,
                Email = user.Email,
                UserRoleId = (Core.Enums.UserRoles)user.UserRoleId,
                IsActive = user.IsActive,
                UserDetails = user.UserDetails is not null ? new UserDetailsDto
                {
                    Id = user.UserDetails.Id,
                    FirstName = user.UserDetails.FirstName,
                    LastName = user.UserDetails.LastName,
                    Patronymic = user.UserDetails.Patronymic,
                    Position = user.UserDetails.Position,
                    AdditionalInfo = user.UserDetails.AdditionalInfo,
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
                IsActive = user.IsActive,
                UserDetails = user.UserDetails is not null ? new UserDetailsDto
                {
                    Id = user.UserDetails.Id,
                    FirstName = user.UserDetails.FirstName,
                    LastName = user.UserDetails.LastName,
                    Patronymic = user.UserDetails.Patronymic,
                    Position = user.UserDetails.Position,
                    AdditionalInfo = user.UserDetails.AdditionalInfo,
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

            var isNotUniqueEmail = await dataContext.Users
                .AnyAsync(u => u.Email.Equals(dto.Email));

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
                FirstName = htmlSanitizer.Sanitize(dto.FirstName),
                LastName = htmlSanitizer.Sanitize(dto.LastName),
                Patronymic = htmlSanitizer.Sanitize(dto.Patronymic),
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
                IsActive = user.IsActive,
                UserDetails = new UserDetailsDto
                {
                    Id = userDetails.Id,
                    FirstName = userDetails.FirstName,
                    LastName = userDetails.LastName,
                    Patronymic = userDetails.Patronymic,
                    Position = userDetails.Position,
                    AdditionalInfo = userDetails.AdditionalInfo,
                    Created = userDetails.Created,
                    Updated = userDetails.Updated,
                },
                Created = user.Created,
                Updated = user.Updated,
            };

            return userDto;
        }

        public async Task ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            User? user = await dataContext.Users
                .Include(u => u.UserDetails)
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
            {
                return;
            }

            if (!user.IsActive)
            {
                return;
            }

            List<PasswordResetToken> activeTokens = await dataContext.PasswordResetTokens
                .Where(t => t.UserId == user.Id && !t.IsUsed && t.ExpiresAt > DateTimeOffset.UtcNow)
                .ToListAsync();

            foreach (PasswordResetToken token in activeTokens)
            {
                token.IsUsed = true;
            }

            byte[] tokenBytes = RandomNumberGenerator.GetBytes(32);
            string rawToken = Convert.ToBase64String(tokenBytes)
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');

            string tokenHash = Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(rawToken))).ToLowerInvariant();

            PasswordResetToken resetToken = new()
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = tokenHash,
                ExpiresAt = DateTimeOffset.UtcNow.AddHours(1),
                IsUsed = false,
                Created = DateTimeOffset.UtcNow,
            };

            await dataContext.PasswordResetTokens.AddAsync(resetToken);
            await dataContext.SaveChangesAsync();

            string toName = user.UserDetails?.GetFullName() ?? user.Email;
            string resetUrl = $"{smtpSettings.Value.BaseUrl}/reset-password?token={rawToken}";

            EmailMessage emailMessage = EmailTemplateBuilder.BuildPasswordResetEmail(
                user.Email, toName, resetUrl);

            await emailSender.EnqueueAsync(emailMessage);
        }

        public async Task ResetPasswordAsync(ResetPasswordDto dto)
        {
            string tokenHash = Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(dto.Token))).ToLowerInvariant();

            PasswordResetToken? resetToken = await dataContext.PasswordResetTokens
                .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);

            if (resetToken == null || resetToken.IsUsed || resetToken.ExpiresAt < DateTimeOffset.UtcNow)
            {
                throw new InvalidOperationException("Ссылка для сброса пароля недействительна или истекла.");
            }

            User? user = await dataContext.Users
                .FirstOrDefaultAsync(u => u.Id == resetToken.UserId);

            if (user == null)
            {
                throw new InvalidOperationException("Ссылка для сброса пароля недействительна или истекла.");
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.Updated = DateTimeOffset.UtcNow;

            resetToken.IsUsed = true;
            resetToken.Updated = DateTimeOffset.UtcNow;

            await dataContext.SaveChangesAsync();
        }

        public async Task SetActiveAsync(Guid id, bool isActive, Core.Enums.UserRoles role)
        {
            User? user = await dataContext.Users.FindAsync(id);

            if (user == null || user.UserRoleId != (int)role)
            {
                throw new KeyNotFoundException("Пользователь не найден");
            }

            if (!isActive)
            {
                int activeCount = await dataContext.Users
                    .CountAsync(u => u.UserRoleId == (int)role && u.IsActive);

                if (activeCount <= 1)
                {
                    string roleName = role == Core.Enums.UserRoles.Administrator
                        ? "администратор"
                        : "спикер";
                    throw new InvalidOperationException(
                        $"Нельзя деактивировать последнего активного {roleName}");
                }
            }

            user.IsActive = isActive;
            user.Updated = DateTimeOffset.UtcNow;

            await dataContext.SaveChangesAsync();
        }
    }
}
