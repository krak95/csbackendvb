using Microsoft.EntityFrameworkCore;
using webAPIreact.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Identity;
public class Program
{

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddScoped<IPasswordHasher<Users>, PasswordHasher<Users>>();
        builder.Services.AddDbContext<MyDBcontext>(options =>
        options.UseMySql(builder.Configuration.GetConnectionString("connDB"), new MySqlServerVersion(new Version(8,0,41)))
        );
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowReactApp", policy =>
            policy.AllowAnyOrigin()
            .WithOrigins("http://localhost:3000","http://10.76.76.44:3000")
            .AllowAnyMethod()
            .AllowAnyHeader()
            );
        });
        var key = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiYWRtaW4iOnRydWUsImlhdCI6MTUxNjIzOTAyMn0.KMUFsIDTnFmyG3nMiGM6H9FNFUROf3wh7SmqJp-QV30";

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
        ClockSkew = TimeSpan.Zero
    };
});

        var app = builder.Build();

        if(app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();

        }
        app.UseCors("AllowReactApp");
        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();
        app.Use(async (context, next) =>
        {
            try
            {
                await next();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        });

        //app.UseWebSockets();
        //var webSocketHandler = new WebSocketHandler();
        //app.UseWebSockets();
        //app.Map("/ws", async context =>
        //{
        //    if (context.WebSockets.IsWebSocketRequest)
        //    {
        //        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        //        await webSocketHandler.HandleWebSocketAsync(webSocket);
        //    }
        //    else
        //    {
        //        context.Response.StatusCode = 400;
        //    }
        //});

 

        builder.Services.AddAuthorization();

        app.UseAuthentication();

        app.MapControllers();



        app.Run();
    }
}