using System.Linq;
using FitGymApp.Domain.DTO.Auth.Request;
using FitGymApp.Domain.DTO.Auth.Response;
using FitGymApp.Domain.Models;
using GymFitApp.Repository.Services.Interfaces;

namespace GymFitApp.Repository.Services
{
    public class AuthRepository : IAuthRepository
    {
        private readonly GymFitAppContext _context;
        public AuthRepository(GymFitAppContext context)
        {
            _context = context;
        }

        public LoginResponse? Login(LoginRequest request)
        {
            var user = _context.Users.FirstOrDefault(u =>
                u.IsActive &&
                (u.UserName == request.UserNameOrEmail || u.Email == request.UserNameOrEmail)
                && u.Password == request.Password
            );
            if (user == null) return null;
            return new LoginResponse
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email
            };
        }
    }
}