using API.Errors;
using Application.Abstractions.DbContexts;
using Domain.Primitives;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using Shared.Results;

namespace API.Controllers.BaseControllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public abstract class ODataBaseController<T>(IApplicationDbContext context)
    : ControllerBase where T : BaseEntity
{
    private readonly DbSet<T> TSet = context.Set<T>();

    public virtual bool IsAddable => true;

    [EnableQuery]
    [HttpGet]
    public virtual IQueryable<T> Get() => TSet;

    [EnableQuery]
    [HttpGet("{key}")]
    public virtual async Task<T> Get([FromODataUri] int key)
    {
        T? item = await TSet.FirstOrDefaultAsync(x => x.Id == key);
        return item!;
    }

    [HttpPost]
    public async Task<IActionResult> Add(T item)
    {
        if (!IsAddable)
            return BadRequest(Result.Failure(ODataBaseErrors.CantAdd));

        await TSet.AddAsync(item);
        await context.SaveChangesAsync();

        return Ok(Result.Success(item));
    }
}