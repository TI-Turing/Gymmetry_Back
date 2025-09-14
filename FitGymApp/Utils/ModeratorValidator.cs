using System;
using System.Threading.Tasks;
using Gymmetry.Repository.Services.Interfaces;

namespace FitGymApp.Utils
{
    public static class ModeratorValidator
    {
        public static async Task<bool> IsModeratorAsync(Guid? userId, IUserRepository userRepository, IUserTypeRepository userTypeRepository)
        {
            if (!userId.HasValue) return false;
            
            try
            {
                var user = await userRepository.GetUserByIdAsync(userId.Value);
                if (user == null) return false;
                if (user.UserTypeId == null) return false;
                
                var ut = await userTypeRepository.GetUserTypeByIdAsync(user.UserTypeId.Value);
                if (ut == null) return false;
                
                var name = (ut.Name ?? string.Empty).Trim().ToLowerInvariant();
                return name is "admin" or "moderator" or "soporte" || name.Contains("mod");
            }
            catch
            {
                return false;
            }
        }
    }
}