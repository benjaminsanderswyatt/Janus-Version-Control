﻿using backend.Auth;
using backend.DataTransferObjects;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace backend.Controllers
{
    [Route("api/web/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly JanusDbContext _janusDbContext;
        private readonly UserService _userService;
        private readonly JwtHelper _jwtHelper;

        public UsersController(JanusDbContext janusDbContext, UserService userService, JwtHelper jwtHelper)
        {
            _janusDbContext = janusDbContext;
            _userService = userService;
            _jwtHelper = jwtHelper;
        }


        // POST: api/web/Users/Register
        [HttpPost("Register")]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterUserDto newUser)
        {
            Console.WriteLine("1 - zoe - Made it");
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Console.WriteLine("2 - zoe - Made it");
            try
            {
                await _userService.RegisterUserAsync(newUser.Username, newUser.Email, newUser.Password);// Create user
                Console.WriteLine("3 - zoe - Made it");
                return Created("api/User/Register", new { message = "User registered successfully" });
            }
            catch (Exception ex)
            {
                Console.WriteLine("4 - zoe - Error");
                return BadRequest(new { error = ex.Message });
            }
        }

        // POST: api/web/Users/Login
        [HttpPost("Login")]
        public async Task<IActionResult> LoginUser([FromBody] LoginUserDto loginUser)
        {

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var user = await _janusDbContext.Users.FirstOrDefaultAsync(u => u.Email == loginUser.Email); // Get the user

                if (user != null && PasswordSecurity.VerifyPassword(loginUser.Password, user.PasswordHash, user.Salt)) // Validate the password
                {
                    JwtSecurityToken token = _jwtHelper.GenerateJwtToken(user.UserId, user.Username); // Generate a Jwt Token

                    return Ok(new { Token = new JwtSecurityTokenHandler().WriteToken(token) });
                }

                return Unauthorized(new { message = "Invalid credentials" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }



    }
}
