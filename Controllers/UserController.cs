using AutoMapper;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NuGet.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Train_D.DTO;
using Train_D.DTO.UserDtos;
using Train_D.Models;
using Train_D.Services;

namespace Train_D.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {

        private readonly IAuth _auth; 
       

        public UserController(IAuth auth)
        {
            _auth = auth;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            var Result = await _auth.Register(model);
            
            if (!Result.IsAuthenticated)
                return BadRequest(Result.Message);
            
            return Ok(new {Result.Token});
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
           
            var Result = await _auth.Login(model);
            
            if (!Result.IsAuthenticated)
                return BadRequest(Result.Message);
           
            return Ok(new {Result.Token});
        }

        [HttpPost("LoginWithGoogle")]
        public async Task<IActionResult> LoginWithGoogle([FromBody] string credential)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var Result = await _auth.LoginGoogle(credential);

            if (!Result.IsAuthenticated)
                return BadRequest(Result.Message);

            return Ok(new { Result.Token });

        }

        [HttpPost("AddRole")]
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> AddRole([FromBody] AddRoleModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

           var result =  await _auth.AddRole(model);

            if(!string.IsNullOrEmpty(result))
                return BadRequest(result);
          
            return Ok(model);
        }

        [HttpGet("GetData")]
        [Authorize]
        public async Task<IActionResult> GetForProfile()
        {
            var UserName = HttpContext.User.FindFirstValue("UserName");

            var user = await _auth.GetDataForProfile(UserName);
            return Ok(user);
        }

        [HttpPut("Edit")]
        [Authorize]
        public  async Task<IActionResult> UpdateProfile ([FromBody]UserDTO DTO)
        {
            var username = HttpContext.User.FindFirstValue("UserName");

            var user = await _auth.GetUser(username);
            
            user.FirstName = DTO.FirstName;
            user.LastName = DTO.LastName;
            user.UserName = DTO.UserName;
            user.Email = DTO.Email;
            user.PhoneNumber = DTO.PhoneNumber;
            user.City = DTO.City;

            _auth.UpdateDataForProfile(user);
            return Ok(new { user.FirstName, user.LastName ,user.UserName, user.Email,user.PhoneNumber , user.City });
        }
    }
}
