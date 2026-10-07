using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Domain.Entities;
using TaskManager.Infrastructure.Persistence;

namespace TaskManager.Api.Controllers;

public record TaskRequest(
    [Required, StringLength(100, MinimumLength = 3)] string Title,
    [StringLength(500)] string? Description,
    [Required] Guid CategoryId,
    bool IsCompleted);

[ApiController]
[Authorize]                         // sin token válido → 401
[Route("api/tasks")]
public class TasksController(AppDbContext db) : ControllerBase
{
    // El Id del usuario sale del token (claim "sub"), nunca del body
    private Guid CurrentUserId => Guid.Parse(User.FindFirst("sub")!.Value);

    [HttpGet("/api/categories")]    // GET /api/categories
    public async Task<IActionResult> GetCategories() => Ok(await db.Categories.ToListAsync());

    [HttpGet]                       // GET /api/tasks?search=texto
    public async Task<IActionResult> GetAll(string? search)
    {
        var userId = CurrentUserId;
        var query = db.Tasks.Where(t => t.UserId == userId);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(t => t.Title.Contains(search));

        return Ok(await query.OrderByDescending(t => t.CreatedAt).ToListAsync());
    }

    [HttpGet("{id:guid}")]          // GET /api/tasks/{id}
    public async Task<IActionResult> GetById(Guid id)
    {
        var task = await db.Tasks.FindAsync(id);
        if (task is null || !task.BelongsTo(CurrentUserId)) return NotFound();
        return Ok(task);
    }

    [HttpPost]                      // POST /api/tasks
    public async Task<IActionResult> Create(TaskRequest req)
    {
        if (!await db.Categories.AnyAsync(c => c.Id == req.CategoryId))
            return BadRequest("La categoría no existe.");

        try
        {
            var task = new TaskItem(CurrentUserId, req.CategoryId, req.Title, req.Description);
            db.Tasks.Add(task);
            await db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = task.Id }, task); // 201
        }
        catch (DomainException ex) { return BadRequest(ex.Message); }
    }

    [HttpPut("{id:guid}")]          // PUT /api/tasks/{id}
    public async Task<IActionResult> Update(Guid id, TaskRequest req)
    {
        var task = await db.Tasks.FindAsync(id);
        if (task is null || !task.BelongsTo(CurrentUserId)) return NotFound();

        if (!await db.Categories.AnyAsync(c => c.Id == req.CategoryId))
            return BadRequest("La categoría no existe.");          

        try
        {
            task.Update(req.CategoryId, req.Title, req.Description, req.IsCompleted);
            await db.SaveChangesAsync();
            return Ok(task);
        }
        catch (DomainException ex) { return BadRequest(ex.Message); }
    }

    [HttpDelete("{id:guid}")]       // DELETE /api/tasks/{id}
    public async Task<IActionResult> Delete(Guid id)
    {
        var task = await db.Tasks.FindAsync(id);
        if (task is null || !task.BelongsTo(CurrentUserId)) return NotFound();

        db.Tasks.Remove(task);
        await db.SaveChangesAsync();
        return NoContent(); // 204
    }
}
