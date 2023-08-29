using Api.Data;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<Context>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// be able to inject jwt service calss inside our controllers
builder.Services.AddScoped<JWTServices>();

// defining our identityCore service
builder.Services.AddIdentityCore<User>(Options =>
{
    // requirements for passwords, if we dont specify these it will have a default requirements
    //password configuration
    Options.Password.RequiredLength = 6;
    Options.Password.RequireDigit = false;
    Options.Password.RequireLowercase = false;
    Options.Password.RequireUppercase = false;
    Options.Password.RequireNonAlphanumeric = false;
    //for email confirmation
    Options.SignIn.RequireConfirmedEmail = true;

})
    .AddRoles<IdentityRole>()   //be able to add roles
    .AddRoleManager<RoleManager<IdentityRole>>()    //be able to make use of RoleManager
    .AddEntityFrameworkStores<Context>()            //providing our context
    .AddSignInManager<SignInManager<User>>()        //make use of signin manager
    .AddUserManager<UserManager<User>>()            //make use of UserManager to create users
    .AddDefaultTokenProviders();                    // be able to create tokens for email confirmation

// be able to authenticate users using JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(Options =>
    {
        Options.TokenValidationParameters = new TokenValidationParameters
        {
            //validate the token based on the key we kave provided inside appsettings.development.json JWT:Key
            ValidateIssuerSigningKey = true,
            // the issuer singning key based on JWT:Key
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"])),
            // the issue which in here is the api project url we are using
            ValidIssuer = builder.Configuration["JWT:Issuer"],
            //validate the issuer (who ever is issuing the JWT
            ValidateIssuer = true,
            //dont validate audience (angular side)
            ValidateAudience = false,
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

//adding UseAuthentication into our pipeline and this should come befone useAuthorization
//Authentication varifies the identity of a user or service, & authorization determines their acess rights.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
