using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using NJsonSchema;

namespace ArtificialCast;

public static class ArtificialCast
{
    #region Configuration
    public static string Host = "http://localhost:11434";
    public static string Model = "";
    public static string SystemPrompt = """ 
    You are a conversion assistant. Given two class definitions and input data, your job is to generate plausible output data in the target format
    You always return valid JSON matching the target structure, even if you have to invent or infer values. Do not output anything but JSON.
    Do not output markdown. Do not wrap the output in an object like {{ 'result': [...] }}. Only output the array directly if an array is requested.
    You must not state that you are an AI model.
    You must pretend data to be real and plausible.
    You must infer and invent data to make it plausible.
    Content must always fill the request with proper sounding data.
    Do not generate things like 'this is an example' or 'this provides information about the topic' placeholders.
    You must not output null values on non-nullable properties.
    You must fill requested values with plausible data.
    When asked to merge objects, merge what you can, fill the rest and only ever output the requested type.
    Do not generate an array, unless specified by the schema.
    If asked to query something, return the requested value only. Do not generate an array if not specified.
    If asked to query something, respect the query EXACTLY. Do not add anything else.
    Seriously, don't generate an array unless explicitly asked for it. Please.
    Make sure to respect the output types defined in the schema.
    """;
    #endregion
    #region Utility
    static string ExtractJsonResponse(string response)
    {
        var jsonStart = response.IndexOf('{');
        var jsonEnd = response.LastIndexOf('}');
        if (jsonStart == -1 || jsonEnd == -1 || jsonEnd < jsonStart)
            throw new Exception("Invalid JSON response");

        return response.Substring(jsonStart, jsonEnd - jsonStart + 1);
    }

    static async Task<string> RequestFromOllama(string prompt){
        var request = new
        {
            model = Model,
            prompt = prompt,
            stream = false,
            system = SystemPrompt,
            options = new
            {
                temperature = 1,
            },
        };

        var jsonRequest = JsonSerializer.Serialize(request);
        var httpClient = new HttpClient();
        var response = await httpClient.PostAsync($"{Host}/api/generate", new StringContent(jsonRequest, Encoding.UTF8, "application/json"));
        var result = await response.Content.ReadAsStringAsync();
        return result;
    }
    #endregion
    #region AC
    public static async Task<TOutput> AC<TInput, TOutput>(TInput input)
    {
        var json = JsonSerializer.Serialize(input, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        });

        var outputTypeJson = JsonSchema.FromType<TOutput>().ToJson();

        var prompt = $"""
            Convert the following object of type {typeof(TInput)} to an object of type {typeof(TOutput)}.

            Input object: {json}

            Output type definition: {outputTypeJson}
            """;

        var response = await RequestFromOllama(prompt);
        // The response is json containing a "result" field. We want that field.
        var modelResponse = JsonDocument.Parse(response).RootElement;
    
        var jsonResponse = modelResponse.GetProperty("response").GetString();
        if (string.IsNullOrWhiteSpace(jsonResponse))
            throw new InvalidOperationException("LLM returned null or empty response.");
        Console.WriteLine($"LLM response: {jsonResponse}");
        jsonResponse = ExtractJsonResponse(jsonResponse);

        TOutput? output;
        
        try{
            output = JsonSerializer.Deserialize<TOutput>(jsonResponse);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Failed to deserialize LLM response.", ex);
        }
        
        if (output == null)
            throw new InvalidOperationException("Deserialization resulted in a null value.");
        
        return output;
    }
    #endregion
    #region AF
    class ObjectCreationRequest
    {
        public string Prompt { get; set; }
    }
    class ObjectCreationResponse<T>
    {
        public T Instance { get; set; }
    }
    public static async Task<TOutput> AF<TOutput>(string prompt = "Randomly filled object")
    {
        var request = new ObjectCreationRequest
        {
            Prompt = prompt
        };

        ObjectCreationResponse<TOutput> response = await AC<ObjectCreationRequest, ObjectCreationResponse<TOutput>>(request);

        return response.Instance;
    }
    #endregion
    #region AM
    class ObjectMergeRequest<TInput, TInput2>
    {
        public TInput Input { get; set; }
        public TInput2 Input2 { get; set; }
    }
    class ObjectMergeResponse<TOutput>
    {
        public TOutput MergedInstance { get; set; }
    }
    public static async Task<TOutput> AM<TInput, TInput2, TOutput>(TInput input, TInput2 input2)
    {
        var request = new ObjectMergeRequest<TInput, TInput2>
        {
            Input = input,
            Input2 = input2
        };

        ObjectMergeResponse<TOutput> response = await AC<ObjectMergeRequest<TInput, TInput2>, ObjectMergeResponse<TOutput>>(request);

        return response.MergedInstance;
    }
    #endregion
    #region AS
    class ObjectSplitRequest<TInput>
    {
        public TInput Input { get; set; }
    }
    class ObjectSplitResponse<TOutput, TOutput2>
    {
        public TOutput Instance { get; set; }
        public TOutput2 Instance2 { get; set; }
    }
    public static async Task<Tuple<TOutput, TOutput2>> AS<TInput, TOutput, TOutput2>(TInput input)
    {
        var request = new ObjectSplitRequest<TInput>
        {
            Input = input
        };

        ObjectSplitResponse<TOutput, TOutput2> response = await AC<ObjectSplitRequest<TInput>, ObjectSplitResponse<TOutput, TOutput2>>(request);

        return new Tuple<TOutput, TOutput2>(response.Instance, response.Instance2);
    }
    #endregion
    #region AQ
    class ObjectQueryRequest<TInput>
    {
        public TInput Input { get; set; }
        public string Query { get; set; }
    }
    class ObjectQueryResponse<TOutput>
    {
        public TOutput Instance { get; set; }
    }
    public static async Task<TOutput> AQ<TInput, TOutput>(TInput input, string query)
    {
        var request = new ObjectQueryRequest<TInput>
        {
            Input = input,
            Query = query
        };

        ObjectQueryResponse<TOutput> response = await AC<ObjectQueryRequest<TInput>, ObjectQueryResponse<TOutput>>(request);

        return response.Instance;
    }
    #endregion
}