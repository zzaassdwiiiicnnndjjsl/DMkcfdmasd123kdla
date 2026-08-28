using System.Net.Http.Headers;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    ApplicationName = "StumbleServer"
});

// ОТКЛЮЧАЕМ ОТСЛЕЖИВАНИЕ ФАЙЛОВ (фикс для Render)
builder.Environment.ContentRootPath = AppContext.BaseDirectory;
builder.Host.UseContentRoot(AppContext.BaseDirectory);

builder.Services.AddCors();
builder.Services.AddHttpClient();

var app = builder.Build();
app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

string supabaseUrl = "https://wdqawcpinnrhnxsvpyly.supabase.co/rest/v1";
string supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6IndkcWF3Y3Bpbm5yaG54c3ZweWx5Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3ODc5NDE1MDYsImV4cCI6MjEwMzUxNzUwNn0.pMWL644gw2yPbPCyxnRtQdKv18r_FYCWGgsy3Pe_YWY";

app.MapGet("/api/test", () => Results.Json(new { status = "ok", message = "Server works!" }));

app.MapGet("/api/player/{hwid}", async (string hwid, HttpClient client) =>
{
    try
    {
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("apikey", supabaseKey);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", supabaseKey);
        
        var response = await client.GetAsync($"{supabaseUrl}/players?hwid=eq.{hwid}");
        var json = await response.Content.ReadAsStringAsync();
        
        return Results.Json(JsonSerializer.Deserialize<object>(json));
    }
    catch (Exception e)
    {
        return Results.Problem($"Ошибка: {e.Message}");
    }
});

app.MapPost("/api/player", async (HttpContext context, HttpClient client) =>
{
    try
    {
        using var reader = new StreamReader(context.Request.Body);
        var body = await reader.ReadToEndAsync();
        
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("apikey", supabaseKey);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", supabaseKey);
        client.DefaultRequestHeaders.Add("Prefer", "return=representation");
        
        var content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
        var response = await client.PostAsync($"{supabaseUrl}/players", content);
        var result = await response.Content.ReadAsStringAsync();
        
        return Results.Json(JsonSerializer.Deserialize<object>(result));
    }
    catch (Exception e)
    {
        return Results.Problem($"Ошибка: {e.Message}");
    }
});

app.MapDelete("/api/player/{hwid}", async (string hwid, HttpClient client) =>
{
    try
    {
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("apikey", supabaseKey);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", supabaseKey);
        
        var response = await client.DeleteAsync($"{supabaseUrl}/players?hwid=eq.{hwid}");
        return Results.Ok(new { success = true, message = "Игрок удален" });
    }
    catch (Exception e)
    {
        return Results.Problem($"Ошибка: {e.Message}");
    }
});

app.Run();
