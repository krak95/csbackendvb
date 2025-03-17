using Microsoft.EntityFrameworkCore;
using webAPIreact.Models;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
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
            .WithOrigins("http://localhost:3000")
            .AllowAnyMethod()
            .AllowAnyHeader()
            );
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

        app.Run();
    }
}