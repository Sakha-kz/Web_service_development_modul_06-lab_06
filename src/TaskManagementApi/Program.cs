using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Подключение контроллеров
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 2. Настройка JWT-аутентификации согласно Заданию 5
var jwtSection = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSection["Key"] ?? "SecretKeyForTaskManagementApiAuthenticationModule06Lab06_SuperSecure!";
var issuer = jwtSection["Issuer"] ?? "TaskManagementApi";
var audience = jwtSection["Audience"] ?? "TaskManagementClient";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// 3. Настройка Swagger UI с поддержкой Bearer JWT токена
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Task Management API - Module 06 Laboratory / SRSP",
        Version = "v1",
        Description = "ASP.NET Core Web API для управления задачами с ограничением доступа через JWT Bearer токены и Claims."
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Введите JWT-токен, полученный в методе /api/auth/login, в формате: Bearer {токен}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment() || true)
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Task Management API v1");
        c.RoutePrefix = "swagger";
    });
}

// Задание 5: Обязательное подключение middleware в строгом порядке
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
