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

using AWS.Lambda.Powertools.Parameters.Transform;

namespace AWS.Lambda.Powertools.Parameters.Configuration;

/// <summary>
/// ParameterProviderConfiguration class.
/// </summary>
public class ParameterProviderConfiguration
{
    /// <summary>
    /// Fetches the latest value from the store regardless if already available in cache.
    /// </summary>
    public bool ForceFetch { get; set; }

    /// <summary>
    /// The cache maximum age.
    /// </summary>
    public TimeSpan? MaxAge { get; set; }

    /// <summary>
    /// The transformer instance.
    /// </summary>
    public ITransformer? Transformer { get; set; }
}