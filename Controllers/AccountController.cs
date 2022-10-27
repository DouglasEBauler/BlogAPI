using Blog.Data;
using Blog.Models;
using BlogAPI.Extensions;
using BlogAPI.Services;
using BlogAPI.ViewModels;
using BlogAPI.ViewModels.Accounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureIdentity.Password;
using System.Text.RegularExpressions;

namespace BlogAPI.Controllers;

[ApiController]
public class AccountController : ControllerBase
{
    [HttpPost("v1/accounts")]
    public async Task<IActionResult> Post(
        [FromBody] RegisterViewModel model,
        [FromServices] EmailService emailService,
        [FromServices] BlogDataContext context)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ResultViewModel<string>(ModelState.GetErrors()));

        var user = new User
        {
            Email = model.Email,
            Name = model.Name,
            Slug = model.Email
                .Replace("@", "-")
                .Replace(".", "-"),
        };

        var password = PasswordGenerator.Generate(25);
        user.PasswordHash = PasswordHasher.Hash(password);

        try
        {
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            emailService.Send(
                user.Name,
                user.Email,
                "Bem vindo ao blog!",
                $"Sua senha é <strong>{password}</strong>");

            return Ok(new ResultViewModel<dynamic>(new
            {
                user = user.Email,
                password
            }));
        }
        catch (DbUpdateException)
        {
            return StatusCode(400, new ResultViewModel<string>("05X99 - Este e-mail já está cadastrado"));
        }
        catch
        {
            return StatusCode(500, new ResultViewModel<string>("05X20 - Não foi possível cadastrar o usuário"));
        }
    }

    [HttpPost("v1/login")]
    public async Task<IActionResult> Login(
        [FromServices] TokenService tokenService,
        [FromBody] LoginViewModel model,
        [FromServices] BlogDataContext context)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ResultViewModel<string>(ModelState.GetErrors()));

        var user = await context
            .Users
            .AsNoTracking()
            .Include(x => x.Roles)
            .FirstOrDefaultAsync(r => r.Email == model.Email);

        if (user == null)
            return StatusCode(401, new ResultViewModel<string>("Usuário ou senha inválidos"));
        
        if (!PasswordHasher.Verify(user.PasswordHash, model.Password))
            return StatusCode(401, new ResultViewModel<string>("Usuário ou senha inválidos"));

        try
        {
            var token = tokenService.GenerateToken(user);
            return Ok(new ResultViewModel<string>(token, null));
        }
        catch
        {
            return StatusCode(500, new ResultViewModel<string>("05X21 - Falha interna no servidor"));
        }
    }

    [Authorize]
    [HttpPost("v1/accounts/upload_image")]
    public async Task<IActionResult> UploadImage(
        [FromBody] UploadImageViewModel model,
        [FromServices] BlogDataContext context)
    {
        var fileName = $"{Guid.NewGuid().ToString()}.jpg";
        var data = new Regex(@"^data:image V[a-z]+;base64,")
            .Replace(model.Base64Image, string.Empty);

        var bytes = Convert.FromBase64String(data);

        try
        {
            await System.IO.File.WriteAllBytesAsync($"wwwroot/Images/{fileName}", bytes);

            var user = await context
                .Users
                .FirstOrDefaultAsync(r => r.Email == User.Identity.Name);

            if (user == null)
                return NotFound(new ResultViewModel<User>("Usuário não encontrado"));

            user.Image = $"https://localhost:0000/Images/{fileName}";

            context.Users.Update(user);
            await context.SaveChangesAsync();

            return Ok(new ResultViewModel<string>("Imagem alterada com sucesso"));
        }
        catch
        {
            return StatusCode(500, new ResultViewModel<string>("05X04 - Falha interna no servidor"));
        }
    }

    //[Authorize(Roles = "user")]
    //[HttpGet("v1/user")]
    //public IActionResult GetUser() => Ok(User.Identity.Name);

    //[Authorize(Roles = "author")]
    //[HttpGet("v1/author")]
    //public IActionResult GetAuthor() => Ok(User.Identity.Name);

    //[Authorize(Roles = "admin")]
    //[HttpGet("v1/admin")]
    //public IActionResult GetAdmin() => Ok(User.Identity.Name);
}
