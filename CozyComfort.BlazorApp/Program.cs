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

// Add HttpContext accessor (important for header management)
builder.Services.AddHttpContextAccessor();

// Configure HttpClient for each API (before authentication services)
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

// Add Authorization - IMPORTANT: Order matters!
builder.Services.AddAuthorizationCore();

// Register CustomAuthStateProvider as Scoped (not Singleton) because it depends on ILocalStorageService
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
    provider.GetRequiredService<CustomAuthStateProvider>());

// Add CascadingAuthenticationState after registering AuthenticationStateProvider
builder.Services.AddCascadingAuthenticationState();

// Register Services as Scoped
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IManufacturerService, ManufacturerService>();
builder.Services.AddScoped<IDistributorService, DistributorService>();
builder.Services.AddScoped<ISellerService, SellerService>();

// Add Session for cart (configure before using)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// Add logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
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

// Use session before antiforgery
app.UseSession();

// Use antiforgery
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();