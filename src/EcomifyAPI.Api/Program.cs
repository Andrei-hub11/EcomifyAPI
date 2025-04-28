using EcomifyAPI.Api.DependencyInjection;
using EcomifyAPI.Api.Middleware;
using EcomifyAPI.Application;
using EcomifyAPI.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

{
    builder
        .Services.AddPresentation(builder.Configuration, builder.Environment)
        .AddApplication()
        .AddInfrastructure(builder.Configuration);
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowSpecificOrigin");

app.UseHttpsRedirection();
app.UseExceptionHandler();
app.UseMiddleware<TokenMiddleware>();
app.UseMiddleware<UnauthorizedResponseMiddleware>();

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers();
app.UseStaticFiles();

app.Run();

public partial class Program { }