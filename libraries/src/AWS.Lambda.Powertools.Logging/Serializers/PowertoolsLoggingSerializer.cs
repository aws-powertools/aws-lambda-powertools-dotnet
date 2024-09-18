/*
 * Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License").
 * You may not use this file except in compliance with the License.
 * A copy of the License is located at
 *
 *  http://aws.amazon.com/apache2.0
 *
 * or in the "license" file accompanying this file. This file is distributed
 * on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either
 * express or implied. See the License for the specific language governing
 * permissions and limitations under the License.
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using AWS.Lambda.Powertools.Common;
using AWS.Lambda.Powertools.Logging.Internal.Converters;
using Microsoft.Extensions.Logging;

namespace AWS.Lambda.Powertools.Logging.Serializers;

/// <summary>
/// Provides serialization functionality for Powertools logging.
/// </summary>
internal static class PowertoolsLoggingSerializer
{
    private static LoggerOutputCase _currentOutputCase = LoggerOutputCase.SnakeCase;
    private static readonly object _lock = new object();
    private static readonly ConcurrentBag<JsonSerializerContext> AdditionalContexts =
        new ConcurrentBag<JsonSerializerContext>();

    /// <summary>
    /// Gets the JsonSerializerOptions instance.
    /// </summary>
    internal static JsonSerializerOptions GetSerializerOptions()
    {
        lock (_lock)
        {
            var options = BuildJsonSerializerOptions();

#if NET8_0_OR_GREATER
            foreach (var context in AdditionalContexts)
            {
                options.TypeInfoResolverChain.Add(context);
            }
#endif

            return options;
        }
    }

    /// <summary>
    /// Configures the naming policy for the serializer.
    /// </summary>
    /// <param name="loggerOutputCase">The case to use for serialization.</param>
    public static void ConfigureNamingPolicy(LoggerOutputCase loggerOutputCase)
    {
        lock (_lock)
        {
            _currentOutputCase = loggerOutputCase;
        }
    }

#if NET6_0
    /// <summary>
    /// Serializes an object to a JSON string.
    /// </summary>
    /// <param name="value">The object to serialize.</param>
    /// <returns>A JSON string representation of the object.</returns>
    public static string Serialize(object value)
    {
        var options = GetSerializerOptions();
        return JsonSerializer.Serialize(value, options);
    }
#endif

#if NET8_0_OR_GREATER

    /// <summary>
    /// Serializes an object to a JSON string.
    /// </summary>
    /// <param name="value">The object to serialize.</param>
    /// <param name="inputType">The type of the object to serialize.</param>
    /// <returns>A JSON string representation of the object.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the input type is not known to the serializer.</exception>
    public static string Serialize(object value, Type inputType)
    {
        var typeInfo = GetTypeInfo(inputType);

        if (typeInfo == null)
        {
            throw new InvalidOperationException(
                $"Type {inputType} is not known to the serializer. Ensure it's included in the JsonSerializerContext.");
        }

        return JsonSerializer.Serialize(value, typeInfo);
    }

    /// <summary>
    /// Adds a JsonSerializerContext to the serializer options.
    /// </summary>
    /// <param name="context">The JsonSerializerContext to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when the context is null.</exception>
    internal static void AddSerializerContext(JsonSerializerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!AdditionalContexts.Contains(context))
        {
            AdditionalContexts.Add(context);
        }
    }

    /// <summary>
    /// Gets the JsonTypeInfo for a given type.
    /// </summary>
    /// <param name="type">The type to get information for.</param>
    /// <returns>The JsonTypeInfo for the specified type, or null if not found.</returns>
    internal static JsonTypeInfo GetTypeInfo(Type type)
    {
        var options = GetSerializerOptions();
        return options.TypeInfoResolver?.GetTypeInfo(type, options);
    }
#endif

    /// <summary>
    /// Builds and configures the JsonSerializerOptions.
    /// </summary>
    /// <returns>A configured JsonSerializerOptions instance.</returns>
    private static JsonSerializerOptions BuildJsonSerializerOptions()
    {
        var jsonOptions = new JsonSerializerOptions();

        switch (_currentOutputCase)
        {
            case LoggerOutputCase.CamelCase:
                jsonOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                jsonOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                break;
            case LoggerOutputCase.PascalCase:
                jsonOptions.PropertyNamingPolicy = PascalCaseNamingPolicy.Instance;
                jsonOptions.DictionaryKeyPolicy = PascalCaseNamingPolicy.Instance;
                break;
            default: // Snake case
                jsonOptions.PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance;
                jsonOptions.DictionaryKeyPolicy = SnakeCaseNamingPolicy.Instance;
                break;
        }

        jsonOptions.Converters.Add(new ByteArrayConverter());
        jsonOptions.Converters.Add(new ExceptionConverter());
        jsonOptions.Converters.Add(new MemoryStreamConverter());
        jsonOptions.Converters.Add(new ConstantClassConverter());
        jsonOptions.Converters.Add(new DateOnlyConverter());
        jsonOptions.Converters.Add(new TimeOnlyConverter());

#if NET8_0_OR_GREATER
        jsonOptions.Converters.Add(new JsonStringEnumConverter<LogLevel>());
#elif NET6_0
        jsonOptions.Converters.Add(new JsonStringEnumConverter());
#endif

        jsonOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        jsonOptions.PropertyNameCaseInsensitive = true;

#if NET8_0_OR_GREATER
        jsonOptions.TypeInfoResolverChain.Add(PowertoolsLoggingSerializationContext.Default);
        foreach (var context in AdditionalContexts)
        {
            jsonOptions.TypeInfoResolverChain.Add(context);
        }
#endif
        return jsonOptions;
    }

#if NET8_0_OR_GREATER
    internal static bool HasContext(JsonSerializerContext customContext)
    {
        return AdditionalContexts.Contains(customContext);
    }

    internal static void ClearContext()
    {
        AdditionalContexts.Clear();
    }
#endif
}