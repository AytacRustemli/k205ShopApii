using System.IdentityModel.Tokens.Jwt;
using Business.Abstract;
using Core.Entity.Models;
using Core.Security.Hasing;
using Core.Security.TokenHandler;
using Entitties.DTOs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthManager _authManager;
        private readonly TokenGenerator _tokenGenerator;
        private readonly HasingHandler _hasingHandler;
        private readonly IRoleManager _roleManager;
        public AuthController(IAuthManager authManager, TokenGenerator tokenGenerator, HasingHandler hasingHandler, IRoleManager roleManager)
        {
            _authManager = authManager;
            _tokenGenerator = tokenGenerator;
            _hasingHandler = hasingHandler;
            _roleManager = roleManager;
        }

        [HttpPost("login")]
        public async Task<object> LoginUser(LoginDTO model)
        {
            var user = _authManager.Login(model.Email);

            if (user == null) return Ok(new { status = 404,message = "Bele bir istifaddeci tapilmadi." });

            if (user.Email == model.Email && user.Password == _hasingHandler.PasswordHash(model.Password))
            {
                var role =  _roleManager.GetRole(user.Id);
                var resultUser = new UserDTO(user.Id,user.FullName, user.Email);
                resultUser.Token = _tokenGenerator.Token(user,role.Name);

                return Ok(new { status = 200, message = resultUser });
            }
            return Ok(new { status = 404, message = "Email ve ya sifre sehvdir." });
            
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTO model)
        {
            var user = _authManager.GetUserByEmail(model.Email);
            if (user != null)
            {
                return Ok(new { status = 200, message = "Email is exist." });
            }
            var pass = model.Password;
            if (pass.Length >= 5)
            {
                _authManager.Register(model);
                return Ok(new { status = 200, message = "okeydi her sey" });
            }
            else
            {
                return Ok(new { status = 404, message = "Sizin sifreniz en az 5 simvoldan ibaret olmalidi." });

            }
        }

        [Authorize]
        [HttpGet("getbyemail")]
        public async Task<object> GetByEmail()
        {
            var _bearer_token = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(_bearer_token);
            var email = jwtSecurityToken.Claims.FirstOrDefault(x=>x.Type=="email").Value;
            var user = _authManager.GetUserByEmail(email);
            var result = new UserDTO(user.Id,user.FullName,user.Email);
            return Ok(result);
        }


        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(Roles = "Admin")]
        [HttpGet("allusers")]
        public List<K205User> GetUsers()
        {
            return _authManager.GetUsers();
        }

        [HttpGet("getuserbyrole/{userId}")] 
        public async Task<Role> GetUserByRole(int userId)
        {
            return _roleManager.GetRole(userId);
        }
    }
}
