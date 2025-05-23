using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TaskManager.API.Services;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
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
    }
}