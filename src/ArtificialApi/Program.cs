global using static ArtificialCast.ArtificialCast;

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

var app = builder.Build();

app.Map("{**catchall}", async (HttpContext context) =>
{
    try
    {
        var apiRequest = new WebRequest
        {
            Method = context.Request.Method,
            Path = context.Request.Path + context.Request.QueryString,
            Headers = context.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
            Body = await new StreamReader(context.Request.Body).ReadToEndAsync()
        };

        Console.WriteLine($"Request: {apiRequest.Method} {apiRequest.Path}");
        try
        {
            var apiResponse = await AC<WebRequest, FilledWebResponse>(apiRequest);

            context.Response.StatusCode = apiResponse.StatusCode;

            if (apiResponse.Headers != null)
            {
                foreach (var kvp in apiResponse.Headers)
                    context.Response.Headers[kvp.Key] = kvp.Value;
            }

            if (!string.IsNullOrWhiteSpace(apiResponse.GeneratedBody))
            {
                context.Response.Headers.Remove("Content-Length");
                await context.Response.WriteAsync(apiResponse.GeneratedBody);
            }
        }
        catch (Exception acEx)
        {
            Console.Error.WriteLine($"Error in AC function: {acEx.Message}");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsync("An error occurred while processing the request.");
        }
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Error: {ex.Message}");
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsync("An internal server error occurred.");
    }
});

app.Run();

public class WebRequest
{
    public string AdditionalModelInstructions = "In case css is used, it must be inline or appended as a <style> tag after the body. Output json when it makes sense. Ouput html where it makes sense. Ouput long html pages if not api. CSS must be fancy and modern with complex layouts. YOU MUST RESPECT CSSTHEME PARAMETERS. Ouput the full response always. Do not truncate.";
    public string Seed = Guid.NewGuid().ToString();
    public string Method { get; set; }
    public string Path { get; set; }
    public Dictionary<string, string> Headers { get; set; }
    public string? Body { get; set; }
}

public class FilledWebResponse
{
    public int StatusCode { get; set; }
    public Dictionary<string, string>? Headers { get; set; }
    public string GeneratedBody { get; set; }
}

[JsonSerializable(typeof(WebRequest))]
[JsonSerializable(typeof(FilledWebResponse))]
internal partial class AppJsonSerializerContext : JsonSerializerContext { }
