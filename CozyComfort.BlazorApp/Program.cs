using Blazored.LocalStorage;
using CozyComfort.BlazorApp;
using CozyComfort.BlazorApp.Components;
using CozyComfort.BlazorApp.Services;
using CozyComfort.BlazorApp.Services.ApiServices;
using CozyComfort.BlazorApp.Services.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Components.Server.Circuits;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Enable detailed errors for debugging
if (builder.Environment.IsDevelopment())
{
    builder.Services.Configure<CircuitOptions>(options =>
    {
        options.DetailedErrors = true;
    });
}

// Add Blazored LocalStorage
builder.Services.AddBlazoredLocalStorage();

// Add Authorization Core
builder.Services.AddAuthorizationCore();

// Add CascadingAuthenticationState
builder.Services.AddCascadingAuthenticationState();

// Register CustomAuthStateProvider
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

// Register ManufacturerService - expects HttpClient, IAuthService, ILogger
builder.Services.AddHttpClient<IManufacturerService, ManufacturerService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7001/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Register DistributorService - expects HttpClient, IAuthService, ILogger
builder.Services.AddHttpClient<IDistributorService, DistributorService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7002/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Register SellerService - expects IHttpClientFactory, ILogger, ILocalStorageService
builder.Services.AddScoped<ISellerService, SellerService>();

// Add Session support for cart
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Add SimpleSessionService
builder.Services.AddSingleton<SimpleSessionService>();

// Add Logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
    if (builder.Environment.IsDevelopment())
    {
        logging.SetMinimumLevel(LogLevel.Debug);
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Session must come before authorization
app.UseSession();

// Authorization middleware
app.UseAuthorization();

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();