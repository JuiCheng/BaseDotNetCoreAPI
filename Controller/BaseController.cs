using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.JsonPatch;

// 定義用於 API 回應的通用模型
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T Data { get; set; }
    public string Message { get; set; }
}

// 泛型基礎控制器，封裝了常見的 CRUD 操作
public class BaseController<TEntity, TContext> : ControllerBase, IBaseController<TEntity>
    where TEntity : class
    where TContext : DbContext
{
    protected readonly TContext _context;

    // 控制器建構函式，注入 DbContext 實例
    public BaseController(TContext context)
    {
        _context = context;
    }

    // 方法：用於生成統一的 API 回應模型
    protected ApiResponse<T> ApiResponse<T>(T data, bool success = true, string message = "")
    {
        return new ApiResponse<T>
        {
            Success = success,
            Data = data,
            Message = message
        };
    }

    // HTTP GET：根據字段名和搜索值進行查詢
    [HttpGet]
    public async virtual Task<ActionResult<IEnumerable<TEntity>>> GetByField([FromQuery] string fieldName, [FromQuery] string searchValue)
    {
        if (string.IsNullOrEmpty(fieldName))
            return BadRequest("搜尋欄位不能為空");

        var property = typeof(TEntity).GetProperty(fieldName);
        if (property == null)
            return BadRequest("無效的搜尋欄位");

        try
        {
            var entities = await _context.Set<TEntity>().ToListAsync();
            entities = entities.Where(e => property.GetValue(e).ToString().Contains(searchValue)).ToList();

            if (entities == null || entities.Count == 0)
                return NotFound("沒有找到相符的資料");

            return Ok(entities);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "查詢時發生了錯誤：" + ex.Message);
        }
    }

    // HTTP GET：根據 ID 查詢實體
    [HttpGet("{id}")]
    public async virtual Task<ActionResult<TEntity>> GetById(Guid id)
    {
        var result = await GetEntityById(id);
        return Ok(result);
    }

    // HTTP POST：創建新實體
    [HttpPost]
    public async virtual Task<ActionResult<TEntity>> Create(TEntity entity)
    {
        var idProperty = typeof(TEntity).GetProperty("Id");
        if (idProperty != null && idProperty.PropertyType == typeof(Guid))
            idProperty.SetValue(entity, Guid.NewGuid());

        RestoreOldPropertyValue<DateTime>("CreatedAt", null, entity, DateTime.UtcNow);

        _context.Set<TEntity>().Add(entity);
        await _context.SaveChangesAsync();

        return Ok(ApiResponse(entity));
    }

    // HTTP PUT：更新現有實體
    [HttpPut]
    public async virtual Task<IActionResult> Update(TEntity entity)
    {
        var result = await GetEntityById(typeof(TEntity).GetProperty("Id").GetValue(entity) as Guid?);
        if (!result.Success)
            return Ok(result);

        var oldEntity = result.Data;
        RestoreOldPropertyValue<DateTime>("UpdatedAt", oldEntity, entity, DateTime.UtcNow);
        RestoreOldPropertyValue<DateTime>("CreatedAt", oldEntity, entity, null);
        RestoreOldPropertyValue<DateTime>("Password", oldEntity, entity, null);

        _context.Entry(oldEntity).CurrentValues.SetValues(entity);

        try
        {
            await _context.SaveChangesAsync();
            return Ok(ApiResponse<TEntity>(null));
        }
        catch (DbUpdateConcurrencyException e)
        {
            return Ok(ApiResponse<TEntity>(null, false, e.Message));
        }
    }

    // HTTP PATCH：根據 ID 部分更新實體
    // JSON Patch 的說明，其中包含了操作（operationType）、路徑（path）、操作類型（op）、來源（from）和值（value）字段。以下是這些字段的解釋：
    // operationType: 表示操作的類型。通常使用數字來表示，0 表示添加（Add）、1 表示刪除（Remove）、2 表示替換（Replace）、3 表示移動（Move）和 4 表示拷貝（Copy）等。
    // path: 表示要應用操作的目標屬性的路徑。這可以是實體屬性的路徑，例如 "address.street"，或者是索引的路徑，例如 "items/0"。
    // op: 表示要執行的操作。通常使用字符串來表示，例如 "add"、"remove"、"replace"、"move" 和 "copy"。
    // from: 僅用於 "move" 和 "copy" 操作，表示要移動或拷貝的原始位置。
    // value: 表示要應用的值。這個字段通常在 "add" 和 "replace" 操作中使用，表示要添加或替換的新值。
    // [
    //   {
    //     "operationType": 2,
    //     "path": "Email",
    //     "op": "replace",
    //     "value": "new_email@example.com"
    //   }
    // ]
    // 在這個 JSON Patch 中：
    // operationType 是 2，代表這是一個替換操作。
    // path 是 mail，指定了要替換的目標屬性的路徑，這裡是 mail 屬性。
    // op 是 "replace"，表示要執行的操作是替換。
    // value 是 "new_email@example.com"，表示要替換成的新郵件地址。
    [HttpPatch("{id}")]
    public async virtual Task<IActionResult> Patch(Guid id, [FromBody] JsonPatchDocument<TEntity> patchDoc)
    {
        if (patchDoc == null)
            return BadRequest();

        var result = await GetEntityById(id);
        if (!result.Success)
            return Ok(result);

        var entity = result.Data;
        patchDoc.ApplyTo(entity, ModelState);

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            RestoreOldPropertyValue<DateTime>("UpdatedAt", null, entity, DateTime.UtcNow);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException e)
        {
            return Ok(ApiResponse<TEntity>(null, false, e.Message));
        }

        return Ok(ApiResponse<TEntity>(null));
    }

    // HTTP DELETE：根據 ID 刪除實體
    [HttpDelete("{id}")]
    public async virtual Task<ActionResult<TEntity>> Delete(Guid id)
    {
        var result = await RemoveEntity(id);
        return Ok(result);
    }

    // HTTP DELETE：根據 ID 禁用實體
    [HttpDelete("Disable/{id}")]
    public async virtual Task<ActionResult<TEntity>> Disable(Guid id)
    {
        var result = await GetEntityById(id);
        if (!result.Success)
            return Ok(result);

        var entity = result.Data;
        var isEnableProperty = typeof(TEntity).GetProperty("IsEnable");
        if (isEnableProperty != null)
            isEnableProperty.SetValue(entity, false);

        await _context.SaveChangesAsync();

        return Ok(ApiResponse(entity));
    }

    // 方法：根據 ID 查詢實體
    private async Task<ApiResponse<TEntity>> GetEntityById(Guid? id)
    {
        if (id == null)
            return ApiResponse<TEntity>(null, false, "ID 不能為空");

        var entity = await _context.Set<TEntity>().FindAsync(id);
        if (entity == null)
            return ApiResponse<TEntity>(null, false, "未找到相符的資料");

        return ApiResponse(entity);
    }

    // 方法：根據 ID 刪除實體
    private async Task<ApiResponse<TEntity>> RemoveEntity(Guid id)
    {
        var entity = await _context.Set<TEntity>().FindAsync(id);
        if (entity == null)
            return ApiResponse<TEntity>(null, false, "未找到相符的資料");

        _context.Set<TEntity>().Remove(entity);
        await _context.SaveChangesAsync();

        return ApiResponse(entity);
    }

    // 方法：還原舊屬性值
    private void RestoreOldPropertyValue<T>(string propertyName, TEntity oldEntity, TEntity entity, T? data) where T : struct
    {
        var property = typeof(TEntity).GetProperty(propertyName);
        if (property != null)
        {
            if (data.HasValue)
                property.SetValue(entity, data);
            else
            {
                var oldPropertyValue = property.GetValue(oldEntity);
                property.SetValue(entity, oldPropertyValue);
            }
        }
    }
}
