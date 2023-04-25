using AutoMapper;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Train_D.Data;
using Train_D.Helper;
using Train_D.Models;
using Train_D.Models.AuthenticationModels;

namespace Train_D.Services
{
    public class Auth : IAuth
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly JWT _jwt;
        private readonly IMapper _mapper;

        public Auth(UserManager<User> userManager, IOptions<JWT> jwt, IMapper mapper, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _jwt = jwt.Value;
            _mapper = mapper;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task<AuthModel> Register(RegisterModel model)
        {
            if (await _userManager.Users.AnyAsync(e => e.Email == model.Email))
                return new AuthModel { Message = "Email is already Registered" };
            if (await _userManager.Users.AnyAsync(u => u.NormalizedUserName == model.UserName))
                return new AuthModel { Message = "UserName is already Registered" };

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

            // var res = _mapper.Map<AuthModel>(model);
            return new AuthModel
            {
                Email = user.Email,
                ExpiresOn = jwtSecurityToken.ValidTo,
                IsAuthenticated = true,
                Roles = new List<string> { "User" },
                Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                UserName = user.UserName
            };
            //return res;
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
            var rolesList = await _userManager.GetRolesAsync(user);

            authModel.IsAuthenticated = true;
            authModel.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            authModel.Email = user.Email;
            authModel.UserName = user.UserName;
            authModel.ExpiresOn = jwtSecurityToken.ValidTo;
            authModel.Roles = rolesList.ToList();

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
            var authModel = new AuthModel();
            var jwtSecurityToken = await CreateJwtToken(user);
            var rolesList = await _userManager.GetRolesAsync(user);

            authModel.IsAuthenticated = true;
            authModel.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            authModel.Email = user.Email;
            authModel.UserName = user.UserName;
            authModel.ExpiresOn = jwtSecurityToken.ValidTo;
            authModel.Roles = rolesList.ToList();

            return authModel;
        }

        public async Task<User> GetUser(string UserName)
        {
            return await _context.Users.SingleOrDefaultAsync(u => u.UserName == UserName);
        }

        public async Task<ProfileModel> GetDataForProfile(string UserName)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.UserName == UserName);
            var Result = new ProfileModel
            {
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName
            };
            return Result; 
        }

        public User UpdateDataForProfile(User user)
        {
            _context.Update(user);
            _context.SaveChanges();
            return user;
        }

        private async Task<JwtSecurityToken> CreateJwtToken(User user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var roleClaims = new List<Claim>();

            foreach (var role in roles)
                roleClaims.Add(new Claim("roles", role));

            var claims = new[]
            {
                new Claim("UserName", user.UserName),
                new Claim("Email", user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),


            }
            .Union(userClaims)
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

            //var user = new User // i'll apply autoMapper in future 
            //{
            //    Email = payload.Email,
            //    UserName = payload.Email.Substring(0, payload.Email.IndexOf("@")),
            //    FirstName = payload.GivenName,
            //    LastName = payload.FamilyName
            //};

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

            // var res = _mapper.Map<AuthModel>(model);
            return new AuthModel
            {
                Email = user.Email,
                ExpiresOn = jwtSecurityToken.ValidTo,
                IsAuthenticated = true,
                Roles = new List<string> { "User" },
                Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                UserName = user.UserName
            };

        }
    }
}