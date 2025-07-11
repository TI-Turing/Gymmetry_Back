using FitGymApp.Application.Services.Interfaces;
using FitGymApp.Domain.DTO.Auth.Request;
using FitGymApp.Domain.DTO.Auth.Response;
using GymFitApp.Repository.Services.Interfaces;
using FitGymApp.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FitGymApp.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPasswordService _passwordService;
        private readonly ILogLoginService _logLoginService;
        private readonly ILogErrorService _logErrorService;

        public AuthService(IAuthRepository authRepository, IUserRepository userRepository, IPasswordService passwordService, ILogLoginService logLoginService, ILogErrorService logErrorService)
        {
            _authRepository = authRepository;
            _userRepository = userRepository;
            _passwordService = passwordService;
            _logLoginService = logLoginService;
            _logErrorService = logErrorService;
        }

        public LoginResponse? Login(LoginRequest request)
        {
            try
            {
                
                var users = _userRepository.FindUsersByFields(new Dictionary<string, object> { { "Email", request.UserNameOrEmail } });
                var user = users.FirstOrDefault();
                if (user == null)
                {
                    users = _userRepository.FindUsersByFields(new Dictionary<string, object> { { "UserName", request.UserNameOrEmail } });
                    user = users.FirstOrDefault();
                }
                if (user == null)
                {
                    return null;
                }
                
                var verifyResult = _passwordService.VerifyPassword(request.Password, user.Password);
                if (!verifyResult.Success || !verifyResult.Data)
                {
                    _logLoginService.LogLogin(user.Id, false, null, "Login fallido: password incorrecto");
                    return null;
                }
                
                var response = _authRepository.Login(request);
                if (response != null)
                {
                    response.Token = JwtTokenGenerator.GenerateToken(response.UserId, response.UserName, response.Email);
                    _logLoginService.LogLogin(response.UserId, true, null, "Login exitoso");
                }
                else
                {
                    _logLoginService.LogLogin(user.Id, false, null, "Login fallido: usuario inactivo o error en repositorio");
                }
                return response;
            }
            catch (Exception ex)
            {
                _logLoginService.LogLogin(null, false, null, "Login fallido: excepción interna");
                _logErrorService.LogError(ex);
                return null;
            }
        }
    }
}