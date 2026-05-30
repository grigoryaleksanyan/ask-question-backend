using AskQuestion.BLL.DTO.Speaker;
using AskQuestion.BLL.Repositories.Interfaces;
using AskQuestion.DAL;
using AskQuestion.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace AskQuestion.BLL.Repositories.Implementations
{
    public class SpeakerRepository(DataContext dataContext) : ISpeakerRepository
    {
        public async Task<IEnumerable<SpeakerDto>> GetAllAsync()
        {
            int speakerRoleId = (int)Core.Enums.UserRoles.Speaker;

            IEnumerable<SpeakerDto> speakers = await dataContext.Users
                .AsNoTracking()
                .Include(u => u.UserDetails)
                .Where(u => u.UserRoleId == speakerRoleId
                    && u.UserDetails != null
                    && !u.UserDetails.IsDeleted)
                .Select(u => new SpeakerDto
                {
                    Id = u.Id,
                    FirstName = u.UserDetails.FirstName,
                    LastName = u.UserDetails.LastName,
                    Patronymic = u.UserDetails.Patronymic,
                    Position = u.UserDetails.Position,
                    Email = u.UserDetails.Email,
                    AdditionalInfo = u.UserDetails.AdditionalInfo,
                    Login = u.Login,
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
                    && !u.UserDetails.IsDeleted)
                .Select(u => new SpeakerDto
                {
                    Id = u.Id,
                    FirstName = u.UserDetails.FirstName,
                    LastName = u.UserDetails.LastName,
                    Patronymic = u.UserDetails.Patronymic,
                    Position = u.UserDetails.Position,
                    Email = u.UserDetails.Email,
                    AdditionalInfo = u.UserDetails.AdditionalInfo,
                    Login = u.Login,
                })
                .FirstOrDefaultAsync();

            return speaker;
        }

        public async Task<SpeakerCreatedDto> CreateAsync(SpeakerCreateDto speakerCreateDto)
        {
            var isNotUniqueEmail = dataContext.UserDetails
                .Any(ud => ud.Email.Equals(speakerCreateDto.Email));

            if (isNotUniqueEmail)
            {
                throw new InvalidOperationException("Пользователь с таким Email уже существует");
            }

            string login = await GenerateLoginAsync(speakerCreateDto.Email);
            string password = GeneratePassword();

            Guid userId = Guid.NewGuid();

            User user = new()
            {
                Id = userId,
                Login = login,
                Password = BCrypt.Net.BCrypt.HashPassword(password),
                UserRoleId = (int)Core.Enums.UserRoles.Speaker,
                Created = DateTimeOffset.UtcNow,
            };

            UserDetails userDetails = new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                FirstName = speakerCreateDto.FirstName,
                LastName = speakerCreateDto.LastName,
                Patronymic = speakerCreateDto.Patronymic,
                Position = speakerCreateDto.Position,
                Email = speakerCreateDto.Email,
                IsDeleted = false,
                Created = DateTimeOffset.UtcNow,
            };

            await dataContext.Users.AddAsync(user);
            await dataContext.UserDetails.AddAsync(userDetails);
            await dataContext.SaveChangesAsync();

            SpeakerCreatedDto speakerCreatedDto = new()
            {
                Id = user.Id,
                FirstName = userDetails.FirstName,
                LastName = userDetails.LastName,
                Patronymic = userDetails.Patronymic,
                Position = userDetails.Position,
                Email = userDetails.Email,
                Login = user.Login,
                GeneratedPassword = password,
            };

            return speakerCreatedDto;
        }

        public async Task UpdateAsync(SpeakerUpdateDto speakerUpdateDto)
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

            var isNotUniqueEmail = dataContext.UserDetails
                .Any(ud => ud.Email.Equals(speakerUpdateDto.Email) && ud.UserId != speakerUpdateDto.Id);

            if (isNotUniqueEmail)
            {
                throw new InvalidOperationException("Пользователь с таким Email уже существует");
            }

            user.UserDetails.FirstName = speakerUpdateDto.FirstName;
            user.UserDetails.LastName = speakerUpdateDto.LastName;
            user.UserDetails.Patronymic = speakerUpdateDto.Patronymic;
            user.UserDetails.Position = speakerUpdateDto.Position;
            user.UserDetails.Email = speakerUpdateDto.Email;
            user.UserDetails.AdditionalInfo = speakerUpdateDto.AdditionalInfo;
            user.UserDetails.Updated = DateTimeOffset.UtcNow;

            await dataContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
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

            user.UserDetails.IsDeleted = true;
            user.UserDetails.Updated = DateTimeOffset.UtcNow;

            await dataContext.SaveChangesAsync();
        }

        private async Task<string> GenerateLoginAsync(string email)
        {
            string baseLogin = email.Split('@')[0];
            string login = baseLogin;
            int suffix = 1;

            while (await dataContext.Users.AnyAsync(u => u.Login == login))
            {
                login = $"{baseLogin}{suffix}";
                suffix++;
            }

            return login;
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
