using Microsoft.EntityFrameworkCore;
using SporttiporssiAPI;
using SporttiporssiAPI.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Azure.Identity;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Azure.Security.KeyVault.Secrets;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", builder =>
    {
        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var keyvaultUrl = builder.Configuration.GetSection("KeyVault:KeyVaultURL");
var keyvaultClientId = builder.Configuration.GetSection("KeyVault:ClientId");
var keyvaultClientSecret = builder.Configuration.GetSection("KeyVault:ClientSecret");
var keyvaultDirectoryID = builder.Configuration.GetSection("KeyVault:DirectoryID");

var credential = new ClientSecretCredential(keyvaultDirectoryID.Value!.ToString(), keyvaultClientId.Value!.ToString(), keyvaultClientSecret.Value!.ToString());
builder.Configuration.AddAzureKeyVault(keyvaultUrl.Value!.ToString(), keyvaultClientId.Value!.ToString(), keyvaultClientSecret.Value!.ToString(), new DefaultKeyVaultSecretManager());
var client = new SecretClient(new Uri(keyvaultUrl.Value!.ToString()), credential);

builder.Services.AddHttpClient();

if(builder.Environment.IsProduction() || builder.Environment.IsDevelopment())
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(client.GetSecret("DBConnection").Value.Value.ToString()));
}
//else if (builder.Environment.IsDevelopment())
//{
//    builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
//}

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();
var jwtKey = client.GetSecret("JWTKey").Value.Value;
builder.Services.AddSingleton(new JwtHelper(
    jwtKey,
    jwtSettings!.Issuer,
    jwtSettings.Audience,
    jwtSettings.ExpireDays));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,       
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    //app.UseSwagger();
    //app.UseSwaggerUI();
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseRouting();

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.Run();
