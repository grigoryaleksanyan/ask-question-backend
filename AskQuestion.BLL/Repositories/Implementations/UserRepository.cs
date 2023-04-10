﻿using AskQuestion.BLL.DTO.User;
using AskQuestion.BLL.Repositories.Interfaces;
using AskQuestion.DAL;
using AskQuestion.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace AskQuestion.BLL.Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _dataContext;

        public UserRepository(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<UserDto?> AuthorizeUser(UserAuthDto userAuthDto)
        {
            User? user = await _dataContext.Users
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
                UserRoleId = (Core.Enums.UserRole)user.UserRoleId,
                Сreated = user.Сreated,
            };

            return userDto;
        }

        public async Task<UserDto?> GetById(Guid id)
        {
            User? user = await _dataContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(user => user.Id == id);

            if (user == null)
            {
                return null;
            }

            UserDto userDto = new()
            {
                Id = user.Id,
                Login = user.Login,
                UserRoleId = (Core.Enums.UserRole)user.UserRoleId,
                Сreated = user.Сreated,
                Updated = user.Updated,
            };

            return userDto;
        }

        public async Task ChangePassword(UserPasswordUpdateDto userPasswordUpdateDto)
        {
            User? user = await _dataContext.Users
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

            await _dataContext.SaveChangesAsync();
        }
    }
}
