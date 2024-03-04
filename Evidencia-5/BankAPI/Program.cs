using System.Text;
using BankAPI.Data;
using BankAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//DBContext
builder.Services.AddSqlServer<BankContext>(
    builder.Configuration.GetConnectionString("BankConnetion")
);

//Service Layer
builder.Services.AddScoped<ClientService>();
builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<AccountTypeService>();
builder.Services.AddScoped<LoginService>();
builder.Services.AddScoped<LoginClientService>();
builder.Services.AddScoped<BankTransactionService>();

builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"])
            ),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

builder
    .Services.AddAuthentication("ClientToken")
    .AddJwtBearer(
        "ClientToken",
        options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(builder.Configuration["JWT:ClientKey"])
                ),
                ValidateIssuer = false,
                ValidateAudience = false
            };
        }
    );

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SuperAdmin", policy => policy.RequireClaim("AdminType", "Super"));
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        "ClientPolicy",
        policy =>
        {
            policy.AuthenticationSchemes.Add("ClientToken");
            policy.RequireAuthenticatedUser();
            policy.RequireRole("Client");
        }
    );
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
