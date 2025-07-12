using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u =>
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