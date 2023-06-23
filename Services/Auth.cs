﻿using AutoMapper;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Train_D.Helper;
using Train_D.Models;

namespace Train_D.Services
{
    public class Auth : IAuth
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly JWT _jwt;
        private readonly IMapper _mapper;

        public Auth(UserManager<User> userManager, IOptions<JWT> jwt, IMapper mapper, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _jwt = jwt.Value;
            _mapper = mapper;
            _roleManager = roleManager;
        }

        public async Task<AuthModel> Register(RegisterModel model)
        {
            if (await _userManager.Users.AnyAsync(e => (e.NormalizedUserName == model.UserName) || (e.Email == model.Email)))
                return new AuthModel { Message = "Email or Username are already Registered" };

            var user = _mapper.Map<User>(model);

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                var errors = string.Empty;

                foreach (var error in result.Errors)
                    errors += $"{error.Description},";

                return new AuthModel { Message = errors };
            }

            await _userManager.AddToRoleAsync(user, "User");

            var jwtSecurityToken = await CreateJwtToken(user);


            return new AuthModel
            {
                IsAuthenticated = true,
                Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                Message = "Register Successfully"
            };
        }

        public async Task<AuthModel> Login(LoginModel model)
        {
            var authModel = new AuthModel();

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.NormalizedUserName == model.UserName);

            if (user is null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                authModel.Message = "UserName or Password is incorrect!";
                return authModel;
            }

            var jwtSecurityToken = await CreateJwtToken(user);


            authModel.IsAuthenticated = true;
            authModel.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            authModel.Message = "Login Successfully";

            return authModel;
        }

        public async Task<string> AddRole(AddRoleModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);

            if (user is null || !await _roleManager.RoleExistsAsync(model.RoleName))
                return "Invalid User ID OR Role !";
            if (await _userManager.IsInRoleAsync(user, model.RoleName))
                return "User Alread Assigned This Role";

            var result = await _userManager.AddToRoleAsync(user, model.RoleName);

            return result.Succeeded ? String.Empty : "Somthing Went Wrong";
        }

        public async Task<AuthModel> LoginGoogle(string credential)
        {
            var authModel = new AuthModel();
            try
            {

                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new List<string> { this._jwt.GoogleClientId }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(credential, settings);

                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == payload.Email);

                if (user is null)
                {
                    return (await RegisterGoogle(payload));
                }

                var jwtSecurityToken = await CreateJwtToken(user);

                authModel.IsAuthenticated = true;
                authModel.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
                authModel.Message = "Login Successfully";

                return authModel;
            }
            catch
            {

                authModel.Message = "invaild token ";
                return authModel;
            }

        }

        private async Task<JwtSecurityToken> CreateJwtToken(User user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var roleClaims = new List<Claim>();

            foreach (var role in roles)
                roleClaims.Add(new Claim("roles", role));

            var claims = new[]
            {
                new Claim("UserName", user.UserName),
                new Claim("Email", user.Email),
                new Claim("UserId", user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),


            }
            .Union(roleClaims);

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SecretKey));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.Now.AddDays(_jwt.DurationInDays),
                signingCredentials: signingCredentials);

            return jwtSecurityToken;
        }

        private async Task<AuthModel> RegisterGoogle(GoogleJsonWebSignature.Payload payload)
        {


            var user = _mapper.Map<User>(payload);

            var result = await _userManager.CreateAsync(user);

            if (!result.Succeeded)
            {
                var errors = string.Empty;

                foreach (var error in result.Errors)
                    errors += $"{error.Description},";

                return new AuthModel { Message = errors };
            }

            await _userManager.AddToRoleAsync(user, "User");

            var jwtSecurityToken = await CreateJwtToken(user);


            return new AuthModel
            {
                IsAuthenticated = true,
                Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                Message = "Register Successfully"
            };

        }
    }
}