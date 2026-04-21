using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TireInventory.Data;
using TireInventory.Models;
using TireInventory;
using TireInventory.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDBContext>(options =>
options.UseSqlServer(
    builder.Configuration.GetConnectionString("ApplicationDBContextConnection"))
                    );

//Identity Service
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDBContext>()
    .AddDefaultTokenProviders();
/////////////

builder.Services.AddTransient<IJsonWebTokenService, JsonWebTokenService>();

//// JWT Token Generation Symmetric Key and Services
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("this-is-signing-key this-is-signing-key this-is-signing-key")); // This should be the key given during token creation
var tokenValidationParameter = new TokenValidationParameters()
{
    IssuerSigningKey = signingKey,
    ValidateIssuer = false,
    ValidateAudience = false
};

// Validate the incoming JWT here
builder.Services.AddAuthentication
    (x => x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(jwt =>
        {
            jwt.TokenValidationParameters = tokenValidationParameter;
        }
        );


//////////////////////////////////////////////////


//////////////////////////////////////////////////
///

//// For putting bearer token through swagger

builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "MyAPI", Version = "v1" });
    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });

    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});


//////////////////////////////////////////////
///


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();


/**********************************************/
var startup = new Startup(builder.Configuration); // My custom startup class.
startup.Configure(app, app.Environment, app.Services); // Configure the HTTP request pipeline.

////////////////////////////////////////////////////////////

app.Run();
