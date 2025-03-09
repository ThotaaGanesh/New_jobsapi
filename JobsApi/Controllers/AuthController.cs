using JobsApi.Helpers;
using JobsApi.Models;
using JobsApi.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JobsApi.Controllers
{
    //[ApiVersion("1.0")]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly AppSettings _appSettings;
        IAuthService _authService;

        public AuthController(
            ILogger<AuthController> logger,
            IAuthService authService,

            IOptions<AppSettings> appSettings)
        {
            _logger = logger;
            _authService = authService;
            _appSettings = appSettings.Value;
        }

        [AllowAnonymous]
        [HttpPost, Route("login")]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            try
            {
                var user = await _authService.Login(model);
                if (user != null)
                {
                    var token = GenerateJwtToken(user);
                    return Ok(new ResponseViewModel<AuthResultViewModel>
                    {
                        Success = true,
                        Data = token,
                        Message = "Login successful"
                    });
                }

                return BadRequest(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while login");
                string message = $"An error occurred while login- " + ex.Message;

                return StatusCode(StatusCodes.Status500InternalServerError, new ResponseViewModel
                {
                    Success = false,
                    Message = message,
                    Error = new ErrorViewModel
                    {
                        Code = "LOGIN_ERROR",
                        Message = message
                    }
                });
            }
        }

        [HttpPost, Route("logout")]
        public async Task<IActionResult> Logout()
        {
            await Task.Delay(500);
            return Ok();
        }

        private AuthResultViewModel GenerateJwtToken(User user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.JwtConfig.Secret);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Aud, _appSettings.JwtConfig.ValidAudience),
                new Claim(JwtRegisteredClaimNames.Iss, _appSettings.JwtConfig.ValidIssuer),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, user.Role!.RoleName.ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddMinutes(Convert.ToDouble(_appSettings.JwtConfig.TokenExpirationMinutes)),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = jwtTokenHandler.WriteToken(token);

            return new AuthResultViewModel()
            {
                token = jwtToken,
                Success = true,
                username = user.Name,
                userId = user.Id,
                organisationName = user.OrganisationName,
                role = user.Role.RoleName,
                location=user.Location
            };
        }

    }
}
