using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NiquiBackend.Application.Interfaces.Infrastructure;
using NiquiBackend.Application.Interfaces.Services;
using NiquiBackend.Application.Services;
using NiquiBackend.Infrastructure.Persistence.Generated;
using NiquiBackend.Infrastructure.Security;

var builder = WebApplication.CreateBuilder(args);

//DbContext (SQL Server)
builder.Services.AddDbContext<NiquiDbContext>(options => 
    options.UseSqlServer(builder.Configuration.GetConnectionString("NiquiConnection")));


//Controllers
builder.Services.AddControllers();


//JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"]!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

//Authorization (politicas por rol)

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanBulkImportCustomers",
        policy => policy.RequireRole("SuperAdmin", "Admin"));

    options.AddPolicy("DeveloperOnly",
        policy => policy.RequireRole("Developer"));

    options.AddPolicy("SuperAdminOnly",
        policy => policy.RequireRole("SuperAdmin"));    
});

//Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen( options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title= "NIQUI API",
        Version = "v1"
    });

    //Configuración para que swagger permita enviar el toquen JWT (Boton Authorize)
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Ingresa el token así: Bearer {tu_token}"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

//CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

//Servicios propios (Se activan a medida que se vayan creando)
// builder.Services.AddScoped<IExcelReaderService, ExcelReaderService>();
// builder.Services.AddScoped<IBulkInsertService, SqlBulkInsertService>();
// builder.Services.AddScoped<ICustomerBulkImportService, CustomerBulkImportService>();
// builder.Services.AddScoped<IAuthService, AuthService>();
// builder.Services.AddScoped<IDeveloperRepository, DeveloperRepository>();
// builder.Services.AddScoped<ISuperAdminRepository, SuperAdminRepository>();
// builder.Services.AddScoped<IAdminRepository, AdminRepository>();
// builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

//Pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


app.Run();