using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class RoleController : BaseController<Role, ApplicationDbContext> 
{
    public RoleController(ApplicationDbContext context) : base(context)
    {   
    }
}
