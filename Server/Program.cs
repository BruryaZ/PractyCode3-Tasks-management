using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Threading.Tasks;
using TodoApi.Models;
using TodoApi.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// Configure the database context
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("tododb"), 
    new MySqlServerVersion(new Version(8, 0, 21))));

// Add CORS services
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

// JWT Authentication
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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"]
    };
});
// Add Authorization
builder.Services.AddAuthorization();

// Add Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Description = "Bearer Authentication with JWT Token",
        Type = SecuritySchemeType.Http
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

app.UseMiddleware<ErrorLoggingMiddleware>();

// Global error handling
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        var exceptionHandlerPathFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();

        if (exceptionHandlerPathFeature?.Error != null)
        {
            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            logger.LogError(exceptionHandlerPathFeature.Error, "Unhandled exception");

            await context.Response.WriteAsJsonAsync(new
            {
                error = exceptionHandlerPathFeature.Error.Message,
                stackTrace = exceptionHandlerPathFeature.Error.StackTrace
            });
        }
    });
});

// Use CORS policy
app.UseCors("AllowAllOrigins");

// Enable Swagger middleware
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
});

// Ensure HTTPS redirection is set up
app.UseHttpsRedirection();

// Ensure routing middleware is set up
app.UseRouting();

// Use authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Route to get all tasks
app.MapGet("/tasks", async (ToDoDbContext context) =>
{
    return await context.Tasks.ToListAsync();
});


// Route to get task by id
app.MapGet("/tasks/{id}", async (HttpContext httpContext, ToDoDbContext context) =>
{
    if (!int.TryParse(httpContext.Request.RouteValues["id"]?.ToString(), out var id))
    {
        return Results.BadRequest();
    }

    var task = await context.Tasks.FindAsync(id);
    if (task is null) return Results.NotFound();
    return Results.Ok(task);
});

// Route to add a new task
app.MapPost("/tasks", async (ToDoDbContext context, MyTask task) =>
{
    context.Tasks.Add(task);
    await context.SaveChangesAsync();
    return Results.Created($"/tasks/{task.Id}", task);
});

// Route to update a task
app.MapPut("/tasks/{id}", async (int id, MyTask updatedTask, ToDoDbContext context) =>
{
    var task = await context.Tasks.FindAsync(id);
    if (task is null) return Results.NotFound();

    task.IsComplete = updatedTask.IsComplete; // Assuming there is a field named IsComplete
    context.Tasks.Update(task);
    await context.SaveChangesAsync();
    return Results.NoContent();
});

// Route to delete a task
app.MapDelete("/tasks/{id}", async (int id, ToDoDbContext context) =>
{
    var task = await context.Tasks.FindAsync(id);
    if (task is null) return Results.NotFound();

    context.Tasks.Remove(task);
    await context.SaveChangesAsync();
    return Results.NoContent();
});

// Home page
app.MapGet("/", () => "A Task management tool API is running!!");

app.Run();
