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

// Add Authentication and Authorization - IMPORTANT: Order matters!
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
})
.AddCookie("Cookies", options =>
{
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
    options.AccessDeniedPath = "/access-denied";
});

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();

// Register CustomAuthStateProvider BEFORE AuthenticationStateProvider
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
    provider.GetRequiredService<CustomAuthStateProvider>());

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

// Register Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IManufacturerService, ManufacturerService>();
builder.Services.AddScoped<IDistributorService, DistributorService>();
builder.Services.AddScoped<ISellerService, SellerService>();

// Add Distributed Memory Cache - IMPORTANT: Add this before AddSession
builder.Services.AddDistributedMemoryCache();

// Add Session for cart
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add HttpContextAccessor for session
builder.Services.AddHttpContextAccessor();

// Add SessionService if you have one
builder.Services.AddScoped<SessionService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();
app.UseSession();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();