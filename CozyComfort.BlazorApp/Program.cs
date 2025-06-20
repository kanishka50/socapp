using Blazored.LocalStorage;
using CozyComfort.BlazorApp;
using CozyComfort.BlazorApp.Components;
using CozyComfort.BlazorApp.Services;
using CozyComfort.BlazorApp.Services.ApiServices;
using CozyComfort.BlazorApp.Services.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add Blazored LocalStorage
builder.Services.AddBlazoredLocalStorage();

// Add HttpContext accessor
builder.Services.AddHttpContextAccessor();

// Configure HttpClient for each API
builder.Services.AddHttpClient("ManufacturerAPI", client =>
{
    client.BaseAddress = new Uri("https://localhost:7001/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddHttpClient("DistributorAPI", client =>
{
    client.BaseAddress = new Uri("https://localhost:7002/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddHttpClient("SellerAPI", client =>
{
    client.BaseAddress = new Uri("https://localhost:7003/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Add Authentication and Authorization
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
    options.DefaultChallengeScheme = "Cookies";
})
.AddCookie("Cookies", options =>
{
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
    options.AccessDeniedPath = "/access-denied";
    options.ExpireTimeSpan = TimeSpan.FromHours(1);
});

builder.Services.AddAuthorizationCore();

// Register CustomAuthStateProvider as Scoped
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
    provider.GetRequiredService<CustomAuthStateProvider>());

// Add CascadingAuthenticationState
builder.Services.AddCascadingAuthenticationState();

// Register Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IManufacturerService, ManufacturerService>();
builder.Services.AddScoped<IDistributorService, DistributorService>();
builder.Services.AddScoped<SessionService>();
builder.Services.AddScoped<ISellerService, SellerService>();

// Add distributed memory cache for session
builder.Services.AddDistributedMemoryCache();

// Add Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Add authentication middleware
app.UseAuthentication();
app.UseAuthorization();

// Use session before antiforgery
app.UseSession();

// Use antiforgery
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();