using Archive.API.Core.Contracts;
using Archive.API.Exceptions;
using Archive.Books;
using Archive.API.Repositories;
using Archive.API.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// ── Exception handling ────────────────────────────────────────────────────────
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// ── Repositories ──────────────────────────────────────────────────────────────
builder.Services.AddSingleton<IUserRepository, JsonUserRepository>();
builder.Services.AddSingleton<IItemRepository, JsonItemRepository>();

// ── Services ──────────────────────────────────────────────────────────────────
builder.Services.AddSingleton<IAuthService, AuthService>();
builder.Services.AddSingleton<ICatalogService, CatalogService>();
builder.Services.AddSingleton<IWishlistService, WishlistService>();
builder.Services.AddSingleton<IFavoriteGroupsService, FavoriteGroupsService>();
builder.Services.AddSingleton<IPriceAlertService, PriceAlertService>();
builder.Services.AddSingleton<ISeasonalAnalysisService, SeasonalAnalysisService>();
builder.Services.AddSingleton<IProductSchema, BookProductSchema>();

// ── Pontos flexíveis instanciados pela aplicação Books ─────────────────────────
// Fonte de dados própria, em C# puro — nenhuma dependência de Python.
builder.Services.AddSingleton<IPriceScraper, CuratedBookScraper>();
// Books não usa busca por imagem: registra o no-op (IsAvailable => false).
builder.Services.AddSingleton<IImageSearchProvider, NoOpImageSearchProvider>();

// ── Authentication ────────────────────────────────────────────────────────────
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "archive.session";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;

        options.Events.OnRedirectToLogin = ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization();

// ── Controllers & Swagger ─────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Archivé API",
        Version = "v1",
        Description = "API de monitoramento de preços — domínio Books."
    });
});

// ── CORS ──────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy
            .WithOrigins(
                builder.Configuration["AllowedOrigins"] ?? "http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

var app = builder.Build();

// ── Middleware pipeline ───────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Archivé API v1"));
}

app.UseHttpsRedirection();
app.UseExceptionHandler();

app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
