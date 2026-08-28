using System.Text.Json;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// Добавляем поддержку контроллеров
builder.Services.AddControllers();
// Разрешаем CORS (чтобы мод мог обращаться к серверу)
builder.Services.AddCors();
// Добавляем HTTP клиент для запросов к Supabase
builder.Services.AddHttpClient();

var app = builder.Build();

// Включаем CORS - разрешаем всем
app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

// Включаем маршрутизацию
app.MapControllers();

// ============================================
// ТВОИ ДАННЫЕ ИЗ SUPABASE (вставь свои!)
// ============================================
string supabaseUrl = "https://wdqawwpcinnnrhxsvpyly.supabase.co/rest/v1";
string supabaseKey = "sb_publishable_mYg8jJpjkTgYzSGGGPz5PQ_8_PLnDWe"; // ЗАМЕНИ НА СВОЙ КЛЮЧ!

// ============================================
// ЭНДПОИНТ: Получить игрока по HWID
// ============================================
app.MapGet("/api/player/{hwid}", async (string hwid, HttpClient client) =>
{
    client.DefaultRequestHeaders.Clear();
    client.DefaultRequestHeaders.Add("apikey", supabaseKey);
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", supabaseKey);

    var response = await client.GetAsync($"{supabaseUrl}/players?hwid=eq.{hwid}");
    var json = await response.Content.ReadAsStringAsync();

    if (response.IsSuccessStatusCode)
    {
        return Results.Json(JsonSerializer.Deserialize<object>(json));
    }
    else
    {
        return Results.Problem($"Ошибка Supabase: {response.StatusCode}");
    }
});

// ============================================
// ЭНДПОИНТ: Создать нового игрока
// ============================================
app.MapPost("/api/player", async (HttpContext context, HttpClient client) =>
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

    if (response.IsSuccessStatusCode)
    {
        return Results.Json(JsonSerializer.Deserialize<object>(result));
    }
    else
    {
        return Results.Problem($"Ошибка Supabase: {response.StatusCode}");
    }
});

// ============================================
// ЭНДПОИНТ: Обновить игрока
// ============================================
app.MapPatch("/api/player/{hwid}", async (string hwid, HttpContext context, HttpClient client) =>
{
    using var reader = new StreamReader(context.Request.Body);
    var body = await reader.ReadToEndAsync();

    client.DefaultRequestHeaders.Clear();
    client.DefaultRequestHeaders.Add("apikey", supabaseKey);
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", supabaseKey);
    client.DefaultRequestHeaders.Add("Prefer", "return=representation");

    var content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
    var response = await client.PatchAsync($"{supabaseUrl}/players?hwid=eq.{hwid}", content);
    var result = await response.Content.ReadAsStringAsync();

    if (response.IsSuccessStatusCode)
    {
        return Results.Json(JsonSerializer.Deserialize<object>(result));
    }
    else
    {
        return Results.Problem($"Ошибка Supabase: {response.StatusCode}");
    }
});

// ============================================
// ЭНДПОИНТ: Разблокировать способность
// ============================================
app.MapPost("/api/player/{hwid}/unlock", async (string hwid, HttpContext context, HttpClient client) =>
{
    using var reader = new StreamReader(context.Request.Body);
    var body = await reader.ReadToEndAsync();

    // Сначала получаем текущие данные игрока
    client.DefaultRequestHeaders.Clear();
    client.DefaultRequestHeaders.Add("apikey", supabaseKey);
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", supabaseKey);

    var getResponse = await client.GetAsync($"{supabaseUrl}/players?hwid=eq.{hwid}");
    var getJson = await getResponse.Content.ReadAsStringAsync();

    if (!getResponse.IsSuccessStatusCode)
        return Results.Problem("Игрок не найден");

    // Обновляем данные
    var content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
    var patchResponse = await client.PatchAsync($"{supabaseUrl}/players?hwid=eq.{hwid}", content);
    var result = await patchResponse.Content.ReadAsStringAsync();

    if (patchResponse.IsSuccessStatusCode)
    {
        return Results.Json(JsonSerializer.Deserialize<object>(result));
    }
    else
    {
        return Results.Problem($"Ошибка Supabase: {patchResponse.StatusCode}");
    }
});

// ============================================
// Запуск сервера
// ============================================
app.Run();
