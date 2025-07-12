using System.Linq;
using FitGymApp.Domain.DTO.Auth.Request;
using FitGymApp.Domain.DTO.Auth.Response;
using FitGymApp.Domain.Models;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Repository.Services
{
    public class AuthRepository : IAuthRepository
    {
        private readonly FitGymAppContext _context;
        public AuthRepository(FitGymAppContext context)
        {
            _context = context;
        }

        public LoginResponse? Login(LoginRequest request)
        {
            var user = _context.Users.FirstOrDefault(u =>
                (u.IsActive ?? false) &&
                (u.UserName == request.UserNameOrEmail || u.Email == request.UserNameOrEmail)
                && u.Password == request.Password
            );
            if (user == null) return null;
            return new LoginResponse
            {
                UserId = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email
            };
        }
    }
}