using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.JsonPatch;

public interface IBaseController<TEntity>where TEntity : class
{
    Task<ActionResult<IEnumerable<TEntity>>> GetByField(string fieldName, string searchValue);
    Task<ActionResult<TEntity>> GetById(Guid id);
    Task<ActionResult<TEntity>> Create(TEntity entity);
    Task<IActionResult> Update(TEntity entity);
    Task<IActionResult> Patch(Guid id, JsonPatchDocument<TEntity> patchDoc);
    Task<ActionResult<TEntity>> Delete(Guid id);
    Task<ActionResult<TEntity>> Disable(Guid id);
}