using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TaskManager.Web;
using TaskManager.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(typeof(DragDropService<>));

builder.Services.AddBlazoredLocalStorage();

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped<TaskApiService>();
builder.Services.AddScoped<UserApiService>();
builder.Services.AddScoped<AuthService>();

var baseAddress = builder.Configuration["Api:BaseAddress"];
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(baseAddress!) });

await builder.Build().RunAsync();
