using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class LoginController : ControllerBase
{
    private readonly JwtService _jwtService;
    private readonly ApplicationDbContext _context;
    public LoginController(ApplicationDbContext context, JwtService jwtService) 
    {   
        _context = context; // 注入ApplicationDbContext實例
        _jwtService = jwtService;
    }

    [HttpGet("Login")]
    public IActionResult Login(string account,string Password)
    {
        string token = _jwtService.GenerateToken(account);
        return Ok(token);
    }
}
