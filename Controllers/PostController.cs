using Blog.Data;
using Blog.Models;
using BlogAPI.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogAPI.Controllers
{
    [ApiController]
    public class PostController : ControllerBase
    {
        [HttpGet("v1/posts")]
        public async Task<IActionResult> GetAsync(
            [FromServices] BlogDataContext context,
            [FromQuery] int page = 0,
            [FromQuery] int pageSize = 25)
        {
            try
            {
                var count = await context.Posts.AsNoTracking().CountAsync();

                var posts = await context
                    .Posts
                    .AsNoTracking()
                    .Include(r => r.Category)
                    .Include(r => r.Author)
                    .Select(r => new ListPostsViewModel
                    {
                        Id = r.Id,
                        Title = r.Title,
                        Slug = r.Slug,
                        LastUpdateDate = r.LastUpdateDate,
                        Category = r.Category.Name,
                        Author = $"{r.Author.Name} ({r.Author.Email})",
                    })
                    .Skip(page * pageSize)
                    .Take(pageSize)
                    .OrderByDescending(r => r.LastUpdateDate)
                    .ToListAsync();

                return Ok(new ResultViewModel<dynamic>(new
                {
                    total = count,
                    page,
                    pageSize,
                    posts,
                }));
            }
            catch
            {
                return StatusCode(500, new ResultViewModel<string>("05X30 - Falha interna no servidor"));
            }
        }

        [HttpGet("v1/posts/{id:int}")]
        public async Task<IActionResult> DetailsAsync(
                [FromServices] BlogDataContext context,
                [FromRoute] int id)
        {
            try
            {
                var post = await context
                    .Posts
                    .AsNoTracking()
                    .Include(x => x.Author)
                    .ThenInclude(x => x.Roles)
                    .Include(x => x.Category)
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (post == null)
                    return NotFound(new ResultViewModel<Post>("Conteúdo não encontrado"));

                return Ok(new ResultViewModel<Post>(post));
            }
            catch
            {
                return StatusCode(500, new ResultViewModel<Post>("05X04 - Falha interna no servidor"));
            }
        }

        [HttpGet("v1/posts/category/{category}")]
        public async Task<IActionResult> GetByCategoryAsync(
            [FromRoute] string category,
            [FromServices] BlogDataContext context,
            [FromQuery] int page = 0,
            [FromQuery] int pageSize = 25)
        {
            try
            {
                var count = await context.Posts.AsNoTracking().CountAsync();
                var posts = await context
                    .Posts
                    .AsNoTracking()
                    .Include(x => x.Author)
                    .Include(x => x.Category)
                    .Where(x => x.Category.Slug == category)
                    .Select(x => new ListPostsViewModel
                    {
                        Id = x.Id,
                        Title = x.Title,
                        Slug = x.Slug,
                        LastUpdateDate = x.LastUpdateDate,
                        Category = x.Category.Name,
                        Author = $"{x.Author.Name} ({x.Author.Email})"
                    })
                    .Skip(page * pageSize)
                    .Take(pageSize)
                    .OrderByDescending(x => x.LastUpdateDate)
                    .ToListAsync();
                return Ok(new ResultViewModel<dynamic>(new
                {
                    total = count,
                    page,
                    pageSize,
                    posts
                }));
            }
            catch
            {
                return StatusCode(500, new ResultViewModel<List<Post>>("05X04 - Falha interna no servidor"));
            }
        }
    }
}
