using CustomerApi.Data;
using CustomerApi.Dtos;
using CustomerApi.Security;
using CustomerApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CustomerApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserRepository _userRepository;
    private readonly TokenService _tokenService;

    public AuthController(UserRepository userRepository, TokenService tokenService)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
    }

    // POST api/auth/login
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username.Trim());

        if(user is null || !user.IsActive || !PasswordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            return Unauthorized(new ProblemDetails
            {
                Title = "Login failed",
                Status = StatusCodes.Status401Unauthorized,
                Detail = "Invalid username or password."
            });
        }

        var (token, expiresAt) = _tokenService.CreateToken(user);
        return Ok(new LoginResponse
        {
            Token = token,
            Username = user.Username,
            ExpiresAt = expiresAt,
        });
    }
}
