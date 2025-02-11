using Microsoft.EntityFrameworkCore;
using TodoApi;
using Task = TodoApi.Task;

var builder = WebApplication.CreateBuilder(args);

// הוסף את שירותי CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});

// הוסף את שירותי Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// הוסף את ה-DbContext לשירותים
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("ToDoDB"), 
    new MySqlServerVersion(new Version(6, 0, 21)))); 

var app = builder.Build();

// השתמש ב-Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        c.RoutePrefix = string.Empty; 
    });
}

// הוסף את ה-CORS
app.UseCors("AllowAll");

app.MapGet("/", () => "Hello World!");

// GET: לקבל את כל ה-Tasks
app.MapGet("/tasks", async (ToDoDbContext db) =>
    await db.Tasks.ToListAsync());

// GET: לקבל Task לפי ID
app.MapGet("/tasks/{id}", async (int id, ToDoDbContext db) =>
    await db.Tasks.FindAsync(id) is Task Task
        ? Results.Ok(Task)
        : Results.NotFound());

// POST: להוסיף Task חדש
app.MapPost("/tasks", async (Task task, ToDoDbContext db) =>
{
    db.Tasks.Add(task);
    await db.SaveChangesAsync();
    return Results.Created($"/tasks/{task.Id}", task);
});

// PUT: לעדכן Task קיים
app.MapPut("/tasks/{id}", async (int id, Task inputTask, ToDoDbContext db) =>
{
    var Task = await db.Tasks.FindAsync(id);

    if (Task is null) return Results.NotFound();

    Task.Name = inputTask.Name;
    Task.IsComplete = inputTask.IsComplete;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

// DELETE: למחוק Task לפי ID
app.MapDelete("/tasks/{id}", async (int id, ToDoDbContext db) =>
{
    if (await db.Tasks.FindAsync(id) is Task Task)
    {
        db.Tasks.Remove(Task);
        await db.SaveChangesAsync();
        return Results.Ok(Task);
    }

    return Results.NotFound();
});

app.Run();
