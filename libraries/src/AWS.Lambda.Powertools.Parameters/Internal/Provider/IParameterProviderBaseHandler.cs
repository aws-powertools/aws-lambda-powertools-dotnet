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

using AWS.Lambda.Powertools.Parameters.Cache;
using AWS.Lambda.Powertools.Parameters.Configuration;
using AWS.Lambda.Powertools.Parameters.Transform;

namespace AWS.Lambda.Powertools.Parameters.Internal.Provider;

public interface IParameterProviderBaseHandler
{
    void SetDefaultMaxAge(TimeSpan maxAge);

    TimeSpan? GetDefaultMaxAge();

    TimeSpan GetMaxAge(ParameterProviderConfiguration? config);

    void SetCacheManager(ICacheManager cacheManager);

    ICacheManager GetCacheManager();

    void SetTransformerManager(ITransformerManager transformerManager);

    void AddCustomTransformer(string name, ITransformer transformer);

    Task<T?> GetAsync<T>(string key, ParameterProviderConfiguration? config, Transformation? transformation,
        string? transformerName) where T: class;

    Task<IDictionary<string, T?>> GetMultipleAsync<T>(string path,
        ParameterProviderConfiguration? config, Transformation? transformation, string? transformerName) where T: class;
}