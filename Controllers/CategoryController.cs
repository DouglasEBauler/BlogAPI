using Blog.Data;
using Blog.Models;
using BlogAPI.Extensions;
using BlogAPI.ViewModels;
using BlogAPI.ViewModels.Categories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace BlogAPI.Controllers;

[ApiController]
public class CategoryController : ControllerBase
{
    [HttpGet("v1/categories")]
    public async Task<IActionResult> GetAsync(
        [FromServices] BlogDataContext context,
        [FromServices] IMemoryCache cache)
    {
        try
        {
            var categories = await cache.GetOrCreateAsync("CategoriesCache", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                return await context.Categories.ToListAsync();
            });

            return Ok(new ResultViewModel<List<Category>>(categories));
        }
        catch (Exception e)
        {
            return StatusCode(500, new ResultViewModel<List<Category>>("05XE1 - Falha interna no servidor" + Environment.NewLine + "Stack Trace: " + e.StackTrace + Environment.NewLine + "Mensagem: " + e.Message));
        }
    }
        

    [HttpGet("v1/categories/{id:int}")]
    public async Task<IActionResult> GetById(
        [FromRoute] int id,
        [FromServices] BlogDataContext context)
    {
        try
        {
            var category = await context.Categories.FirstOrDefaultAsync(r => r.Id == id);

            if (category == null)
                return NotFound(new ResultViewModel<Category>("Conteúdo não encontrado"));

            return Ok(new ResultViewModel<Category>(category));

        }
        catch
        {
            return StatusCode(500, new ResultViewModel<Category>("05XE2 - Falha interna no servidor"));
        }
    }

    [HttpPost("v1/categories")]
    public async Task<IActionResult> PostAsync(
        [FromBody] EditorCategoryViewModel model,
        [FromServices] BlogDataContext context)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ResultViewModel<Category>(ModelState.GetErrors()));
        
        try
        {
            var category = new Category
            {
                Name = model.Name,
                Slug = model.Slug.ToLower()
            };

            await context.Categories.AddAsync(category);
            await context.SaveChangesAsync();

            return Created($"v1/categories/{category.Id}", new ResultViewModel<Category>(category));
        }
        catch (DbUpdateException)
        {
            return StatusCode(500, new ResultViewModel<Category>("05XE9 - Não foi possível inserir a categoria"));
        }
        catch
        {
            return StatusCode(500, new ResultViewModel<Category>("05X10 - Falha interna no servidor"));
        }
    }

    [HttpPut("v1/categories/{id:int}")]
    public async Task<IActionResult> PutAsync(
        [FromRoute] int id,
        [FromBody] EditorCategoryViewModel model,
        [FromServices] BlogDataContext context)
    {
        try
        {
            var category = await context.Categories.FirstOrDefaultAsync(r => r.Id == id);

            if (category == null)
                return NotFound();

            category.Name = model.Name;
            category.Slug = model.Slug;

            context.Categories.Update(category);
            await context.SaveChangesAsync();

            return Created(
                $"v1/categories/{category.Id}", new ResultViewModel<Category>(category));
        }
        catch (DbUpdateException)
        {
            return StatusCode(500, new ResultViewModel<Category>("05X11 - Não foi possível atualizar a categoria"));
        }
        catch
        {
            return StatusCode(500, new ResultViewModel<Category>("05X12 - Falha interna no servidor"));
        }
    }

    [HttpDelete("v1/categories/{id:int}")]
    public async Task<IActionResult> DeleteAsync(
        [FromRoute] int id,
        [FromServices] BlogDataContext context)
    {
        try
        {
            var category = await context.Categories.FirstOrDefaultAsync(r => r.Id == id);

            if (category == null)
                return NotFound(new ResultViewModel<Category>("Não foi possível remover a categoria informada!"));

            context.Categories.Remove(category);
            await context.SaveChangesAsync();

            return Ok(new ResultViewModel<Category>(category));
        }
        catch (DbUpdateException)
        {
            return StatusCode(500, new ResultViewModel<Category>("05X13 - Não foi possível remover a categoria"));
        }
        catch
        {
            return StatusCode(500, new ResultViewModel<Category>("05X14 - Falha interna no servidor"));
        }
    }
}
