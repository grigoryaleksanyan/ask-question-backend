using AskQuestion.BLL.DTO.Speaker;
using AskQuestion.BLL.Email;
using AskQuestion.BLL.Helpers;
using AskQuestion.BLL.Repositories.Interfaces;
using AskQuestion.DAL;
using AskQuestion.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AskQuestion.BLL.Repositories.Implementations
{
    public class SpeakerRepository(
        DataContext dataContext,
        IEmailSender emailSender,
        IOptions<SmtpSettings> smtpSettings,
        IHtmlSanitizerService htmlSanitizer) : ISpeakerRepository
    {
        private readonly SmtpSettings _smtpSettings = smtpSettings.Value;
        public async Task<IEnumerable<SpeakerDto>> GetAllAsync()
        {
            int speakerRoleId = (int)Core.Enums.UserRoles.Speaker;

            IEnumerable<SpeakerDto> speakers = await dataContext.Users
                .AsNoTracking()
                .Include(u => u.UserDetails)
                .Where(u => u.UserRoleId == speakerRoleId
                    && u.UserDetails != null)
                .OrderBy(u => u.UserDetails.Order)
                .Select(u => new SpeakerDto
                {
                    Id = u.Id,
                    FirstName = u.UserDetails.FirstName,
                    LastName = u.UserDetails.LastName,
                    Patronymic = u.UserDetails.Patronymic,
                    Position = u.UserDetails.Position,
                    Email = u.Email,
                    AdditionalInfo = u.UserDetails.AdditionalInfo,
                    Order = u.UserDetails.Order,
                    IsActive = u.IsActive,
                })
                .ToListAsync();

            return speakers;
        }

        public async Task<IEnumerable<SpeakerDto>> GetAllPublicAsync()
        {
            int speakerRoleId = (int)Core.Enums.UserRoles.Speaker;

            IEnumerable<SpeakerDto> speakers = await dataContext.Users
                .AsNoTracking()
                .Include(u => u.UserDetails)
                .Where(u => u.UserRoleId == speakerRoleId
                    && u.UserDetails != null
                    && u.IsActive)
                .OrderBy(u => u.UserDetails.Order)
                .Select(u => new SpeakerDto
                {
                    Id = u.Id,
                    FirstName = u.UserDetails.FirstName,
                    LastName = u.UserDetails.LastName,
                    Patronymic = u.UserDetails.Patronymic,
                    Position = u.UserDetails.Position,
                    Email = u.Email,
                    AdditionalInfo = u.UserDetails.AdditionalInfo,
                    Order = u.UserDetails.Order,
                    IsActive = u.IsActive,
                })
                .ToListAsync();

            return speakers;
        }

        public async Task<SpeakerDto?> GetByIdAsync(Guid id)
        {
            int speakerRoleId = (int)Core.Enums.UserRoles.Speaker;

            SpeakerDto? speaker = await dataContext.Users
                .AsNoTracking()
                .Include(u => u.UserDetails)
                .Where(u => u.Id == id
                    && u.UserRoleId == speakerRoleId
                    && u.UserDetails != null
                    && u.IsActive)
                .Select(u => new SpeakerDto
                {
                    Id = u.Id,
                    FirstName = u.UserDetails.FirstName,
                    LastName = u.UserDetails.LastName,
                    Patronymic = u.UserDetails.Patronymic,
                    Position = u.UserDetails.Position,
                    Email = u.Email,
                    AdditionalInfo = u.UserDetails.AdditionalInfo,
                    Order = u.UserDetails.Order,
                    IsActive = u.IsActive,
                })
                .FirstOrDefaultAsync();

            return speaker;
        }

        public async Task<SpeakerCreatedDto> CreateAsync(SpeakerCreateDto speakerCreateDto)
        {
            var isNotUniqueEmail = dataContext.Users
                .Any(u => u.Email.Equals(speakerCreateDto.Email));

            if (isNotUniqueEmail)
            {
                throw new InvalidOperationException("Пользователь с таким Email уже существует");
            }

            string password = GeneratePassword();

            Guid userId = Guid.NewGuid();

            User user = new()
            {
                Id = userId,
                Email = speakerCreateDto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(password),
                UserRoleId = (int)Core.Enums.UserRoles.Speaker,
                Created = DateTimeOffset.UtcNow,
            };

            UserDetails userDetails = new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                FirstName = htmlSanitizer.Sanitize(speakerCreateDto.FirstName),
                LastName = htmlSanitizer.Sanitize(speakerCreateDto.LastName),
                Patronymic = htmlSanitizer.Sanitize(speakerCreateDto.Patronymic),
                Position = htmlSanitizer.Sanitize(speakerCreateDto.Position),
                Order = speakerCreateDto.Order,
                Created = DateTimeOffset.UtcNow,
            };

            await dataContext.Users.AddAsync(user);
            await dataContext.UserDetails.AddAsync(userDetails);
            await dataContext.SaveChangesAsync();

            var credentialsEmail = EmailTemplateBuilder.BuildSpeakerCredentials(
                toEmail: user.Email,
                toName: userDetails.GetFullName(),
                password: password,
                loginUrl: $"{_smtpSettings.BaseUrl}/login");

            await emailSender.EnqueueAsync(credentialsEmail);

            SpeakerCreatedDto speakerCreatedDto = new()
            {
                Id = user.Id,
                FirstName = userDetails.FirstName,
                LastName = userDetails.LastName,
                Patronymic = userDetails.Patronymic,
                Position = userDetails.Position,
                Email = user.Email,
                Order = userDetails.Order,
                IsActive = user.IsActive,
                GeneratedPassword = password,
            };

            return speakerCreatedDto;
        }

        public async Task<SpeakerDto> UpdateAsync(SpeakerUpdateDto speakerUpdateDto)
        {
            int speakerRoleId = (int)Core.Enums.UserRoles.Speaker;

            User? user = await dataContext.Users
                .Include(u => u.UserDetails)
                .FirstOrDefaultAsync(u => u.Id == speakerUpdateDto.Id
                    && u.UserRoleId == speakerRoleId);

            if (user == null || user.UserDetails == null)
            {
                throw new InvalidOperationException("Спикер не найден");
            }

            var isNotUniqueEmail = dataContext.Users
                .Any(u => u.Email.Equals(speakerUpdateDto.Email) && u.Id != speakerUpdateDto.Id);

            if (isNotUniqueEmail)
            {
                throw new InvalidOperationException("Пользователь с таким Email уже существует");
            }

            user.Email = speakerUpdateDto.Email;
            user.Updated = DateTimeOffset.UtcNow;
            user.UserDetails.FirstName = htmlSanitizer.Sanitize(speakerUpdateDto.FirstName);
            user.UserDetails.LastName = htmlSanitizer.Sanitize(speakerUpdateDto.LastName);
            user.UserDetails.Patronymic = htmlSanitizer.Sanitize(speakerUpdateDto.Patronymic);
            user.UserDetails.Position = htmlSanitizer.Sanitize(speakerUpdateDto.Position);
            user.UserDetails.AdditionalInfo = htmlSanitizer.Sanitize(speakerUpdateDto.AdditionalInfo);
            user.UserDetails.Updated = DateTimeOffset.UtcNow;

            await dataContext.SaveChangesAsync();

            return new SpeakerDto
            {
                Id = user.Id,
                FirstName = user.UserDetails.FirstName,
                LastName = user.UserDetails.LastName,
                Patronymic = user.UserDetails.Patronymic,
                Position = user.UserDetails.Position,
                Email = user.Email,
                AdditionalInfo = user.UserDetails.AdditionalInfo,
                Order = user.UserDetails.Order,
                IsActive = user.IsActive,
            };
        }

        public async Task SetActiveAsync(Guid id, bool isActive)
        {
            int speakerRoleId = (int)Core.Enums.UserRoles.Speaker;

            User? user = await dataContext.Users
                .Include(u => u.UserDetails)
                .FirstOrDefaultAsync(u => u.Id == id
                    && u.UserRoleId == speakerRoleId);

            if (user == null || user.UserDetails == null)
            {
                throw new InvalidOperationException("Спикер не найден");
            }

            if (!isActive)
            {
                int activeCount = await dataContext.Users
                    .CountAsync(u => u.UserRoleId == speakerRoleId && u.IsActive);

                if (activeCount <= 1)
                {
                    throw new InvalidOperationException(
                        "Нельзя деактивировать последнего активного спикера");
                }
            }

            user.IsActive = isActive;
            user.Updated = DateTimeOffset.UtcNow;

            await dataContext.SaveChangesAsync();
        }

        public async Task SetOrderAsync(Guid[] ids)
        {
            var userDetails = await dataContext.UserDetails.ToListAsync();

            for (int i = 0; i < ids.Length; i++)
            {
                var detail = userDetails.Find(d => d.UserId == ids[i]);

                if (detail == null)
                {
                    continue;
                }

                detail.Order = i;
            }

            await dataContext.SaveChangesAsync();
        }

        private static string GeneratePassword()
        {
            const string lowercase = "abcdefghijklmnopqrstuvwxyz";
            const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string digits = "0123456789";
            const string all = lowercase + uppercase + digits;

            Random random = new();
            char[] password = new char[8];

            password[0] = lowercase[random.Next(lowercase.Length)];
            password[1] = uppercase[random.Next(uppercase.Length)];
            password[2] = digits[random.Next(digits.Length)];

            for (int i = 3; i < 8; i++)
            {
                password[i] = all[random.Next(all.Length)];
            }

            for (int i = password.Length - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (password[i], password[j]) = (password[j], password[i]);
            }

            return new string(password);
        }
    }
}
