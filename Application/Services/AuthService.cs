using Data.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services
{
    public class AuthService
    {
        private readonly AuthRepository _repository;

        public AuthService(AuthRepository repository)
        {
            _repository = repository;
        }

        public async Task<string> CreateUserAsync(string Username, string Password)
        {
            var user = new User { Username = Username, PasswordHash = Password, IsBlocked = false, Role = "student" };
            return await _repository.CreateUserAsync(user);
        }
    }
}
