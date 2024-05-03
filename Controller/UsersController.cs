using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

[ApiController]
[Route("api/[controller]")]
public class UsersController : BaseController<User, ApplicationDbContext> 
{
    //private readonly ApplicationDbContext _context;

    public UsersController(ApplicationDbContext context) : base(context)
    {
        //_context = context;
    }

     // 複寫基類中的 Create 方法
    
    public override async Task<ActionResult<User>> Create(User user)
    {
        // 在執行基類的 Create 方法之前，添加你的特定用戶邏輯
        // 例如，驗證用戶是否符合特定條件，或者在創建用戶之前進行特定處理
       
        user.Password = ComputeHash(user.Password, new SHA256CryptoServiceProvider());
        // 調用基類中的 Create 方法來執行通用的創建實體邏輯
        var result = await base.Create(user);

        // 在這裡你可以進一步處理基類 Create 方法返回的結果，或者添加額外的邏輯

        return result;
    }

    public override async Task<IActionResult> Update(User user)
    {
        // 在執行基類的 Create 方法之前，添加你的特定用戶邏輯
        // 例如，驗證用戶是否符合特定條件，或者在創建用戶之前進行特定處理
       
        user.Password = ComputeHash(user.Password, new SHA256CryptoServiceProvider());
        // 調用基類中的 Create 方法來執行通用的創建實體邏輯
        var result = await base.Update(user);

        // 在這裡你可以進一步處理基類 Create 方法返回的結果，或者添加額外的邏輯

        return result;
    }

     public override async Task<IActionResult> Patch(Guid id, [FromBody] JsonPatchDocument<User> patchDoc)
    {
        // 檢查是否存在 "Password" 欄位，如果不存在，返回錯誤消息
        bool hasPassword = patchDoc.Operations.Any(o => o.path == "Password");
        if (!hasPassword)
        {
            return Ok(ApiResponse<User>(null, false, "無法更新密碼"));
        }
        
        // 檢查是否存在 "UpdatedUser" 欄位，如果存在，調用基類的 Patch 方法
        bool hasUpdatedUser = patchDoc.Operations.Any(o => o.path == "UpdatedUser");
        if (hasUpdatedUser)
        {
            return await base.Patch(id, patchDoc);
        }
        else
        {
            return Ok(ApiResponse<User>(null, false, "缺少欄位: 修改人資訊"));
        }
    }

    public static string ComputeHash(string input, HashAlgorithm algorithm)
    {
        byte[] inputBytes = Encoding.UTF8.GetBytes(input);
        byte[] hashBytes = algorithm.ComputeHash(inputBytes);

        StringBuilder builder = new StringBuilder();
        foreach (byte b in hashBytes)
        {
            builder.Append(b.ToString("x2")); // 將哈希字節轉換為十六進制字符串
        }
        return builder.ToString();
    }

    
    // [HttpGet]
    // public ActionResult<IEnumerable<User>> Get()
    // {
    //     return _context.Users.ToList();
    // }

    // [HttpGet("{id}")]
    // public ActionResult<User> Get(int id)
    // {
    //     var user = _context.Users.Find(id);
    //     if (user == null)
    //     {
    //         return NotFound();
    //     }
    //     return user;
    // }

    // [HttpPost]
    // public ActionResult<User> Post(User user)
    // {
    //     _context.Users.Add(user);
    //     _context.SaveChanges();
    //     return CreatedAtAction(nameof(Get), new { id = user.Id }, user);
    // }

    // [HttpPut("{id}")]
    // public IActionResult Put(int id, User user)
    // {
    //     if (id != user.Id)
    //     {
    //         return BadRequest();
    //     }

    //     _context.Entry(user).State = EntityState.Modified;
    //     _context.SaveChanges();

    //     return NoContent();
    // }

    // [HttpDelete("{id}")]
    // public IActionResult Delete(int id)
    // {
    //     var user = _context.Users.Find(id);
    //     if (user == null)
    //     {
    //         return NotFound();
    //     }

    //     _context.Users.Remove(user);
    //     _context.SaveChanges();

    //     return NoContent();
    // }
}
