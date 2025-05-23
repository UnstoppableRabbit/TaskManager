// Program.cs
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TaskManager.API.Services;
using Domain.Model;
using Domain.Enums;
using Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<Infrastructure.AppDbContext>(opt =>
    opt.UseSqlite("Data Source=../Infrastructure/task.db"));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<TaskRepository>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<TaskService>();

builder.Services.AddAuthentication("Basic")
    .AddScheme<AuthenticationSchemeOptions, BasicAuthHandler>("Basic", null);

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseCors("AllowAll");
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/users", [Authorize] async (UserService userService) =>
{
    var users = await userService.GetAllUsers();
    return Results.Ok(users);
});

app.MapPost("/register", async (RegisterUserCommand cmd, UserService userService) =>
{
    if (await userService.UserExists(cmd.Login)) return Results.BadRequest("User exists");
    await userService.Register(cmd);
    return Results.Ok("Registered");
});

app.MapPost("/login", async (LoginUserCommand cmd, UserService userService) =>
{
    var user = await userService.Login(cmd);
    return user is null ? Results.Unauthorized() : Results.Ok(user);
});

app.MapGet("/currentUser", [Authorize] async (ClaimsPrincipal user, UserService userService) =>
{
    return Results.Ok(await userService.GetCurrentUser(user.Identity!.Name!));
});

app.MapGet("/allUsers", [Authorize] async (UserService userService) =>
{
    return Results.Ok(await userService.GetAllUsers());
});

app.MapPost("/tasks", [Authorize] async (TaskItem task, TaskService taskService, ClaimsPrincipal user) =>
{
    var resultTask = await taskService.Create(task, user.Identity!.Name!);
    return Results.Ok(resultTask);
});

app.MapDelete("/tasks/{id}", [Authorize] async (Guid id, TaskService taskService, ClaimsPrincipal user) =>
{
    var result = await taskService.Delete(id, user.Identity!.Name!);
    return result ? Results.Ok() : Results.NotFound();
});

app.MapPut("/tasks/{id}", [Authorize] async (Guid id, UpdateTaskCommand cmd, TaskService taskService, ClaimsPrincipal user) =>
{
    var result = await taskService.Update(id, cmd, user.Identity!.Name!);
    return result is null ? Results.NotFound() : Results.Ok(result);
});

app.MapGet("/tasks", async ([FromQuery] StatusTask? status, [FromQuery] Guid? createdBy,
                             [FromQuery] DateTime? from, [FromQuery] DateTime? to,
                             [FromQuery] string? sortBy,
                             TaskService taskService) =>
{
    var tasks = await taskService.GetFiltered(status, createdBy, from, to, sortBy);
    return Results.Ok(tasks);
});

app.Run();


// Models/User.cs
public record RegisterUserCommand(string Login, string Password, string Name);
public record LoginUserCommand(string Login, string Password);
public record AuthenticatedUser(string Login, string Name);

public record UpdateTaskCommand(short Status, string? Comment, TimeSpan? ActualTime);

// Services/UserService.cs
public class UserService
{
    private readonly Infrastructure.AppDbContext _context;
    public UserService(Infrastructure.AppDbContext context) => _context = context;

    public async Task<bool> UserExists(string login) =>
        await _context.Users.AnyAsync(u => u.Login == login);
    public async Task<List<User>> GetAllUsers() =>
        await _context.Users.Select(x => new User { Id = x.Id, Name = x.Name, Login = x.Login }).ToListAsync();

    public async Task<User> GetCurrentUser(string login) {
        var user = await _context.Users.FirstAsync(x => x.Login == login);
        user.Password = string.Empty;
        return user;
    }

    public async Task Register(RegisterUserCommand cmd)
    {
        var user = new User { Login = cmd.Login, Password = cmd.Password, Name = cmd.Name };
        user.Id = Guid.NewGuid();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }

    public async Task<AuthenticatedUser?> Login(LoginUserCommand cmd)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Login == cmd.Login && u.Password == cmd.Password);
        return user is null ? null : new AuthenticatedUser(user.Login, user.Name);
    }

    public async Task<bool> Validate(string login, string password) =>
        await _context.Users.AnyAsync(u => u.Login == login && u.Password == password);
}


// Auth/BasicAuthHandler.cs
public class BasicAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly UserService _userService;

    public BasicAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        UserService userService)
        : base(options, logger, encoder, clock)
    {
        _userService = userService;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
            return AuthenticateResult.Fail("Missing Authorization Header");

        try
        {
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(authHeader.Parameter!)).Split(':');
            var login = credentials[0];
            var password = credentials[1];

            if (!await _userService.Validate(login, password))
                return AuthenticateResult.Fail("Invalid Username or Password");

            var claims = new[] {
                new Claim(ClaimTypes.Name, login)
            };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
        catch
        {
            return AuthenticateResult.Fail("Invalid Authorization Header");
        }
    }
}
