# API

## ArtificialCast AC

Transforms any object into another type using structured inference and LLMs.

```csharp
public static Task<TOut> AC<TIn, TOut>(TIn input);
```

## ArtificialFactory AF
Generates an object of the specified type using type metadata.

```csharp
public static Task<T> AF<T>();
```

## ArtificialMerge AM
Merges two unrelated objects into a new type using structured inference.

```csharp
public static Task<TOut> AM<T1, T2, TOut>(T1 input1, T2 input2);
```

### ArtificialSplit AS
Splits a single object into two separate types using structured inference.

```csharp
public static Task<(Tuple<TOut1, TOut2>)> AS<TIn, TOut1, TOut2>(TIn input);
```

## ArtificialQuery AQ
Performs a natural language query on an object or array, returning a plausible result.

```csharp
public static Task<TOut> AQ<TIn, TOut>(TIn input, string query);
```

## ArtificialCast Configuration
```csharp
// This is required to be set to a model (like gemma3:4b)
public static string Model { get; set; } = "";

// These are optional
public static string Host { get; set; } = "http://localhost:11434";
public static string SystemPrompt { get; set; } = "...";
```