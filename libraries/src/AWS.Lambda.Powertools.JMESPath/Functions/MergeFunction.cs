/*
 * Copyright JsonCons.Net authors. All Rights Reserved.
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

using System.Collections.Generic;
using System.Linq;
using AWS.Lambda.Powertools.JMESPath.Expressions;
using AWS.Lambda.Powertools.JMESPath.Values;

namespace AWS.Lambda.Powertools.JMESPath.Functions;

/// <summary>
/// Merges multiple objects into a single object.
/// </summary>
internal sealed class MergeFunction : BaseFunction
{
    internal MergeFunction()
        : base(null)
    {
    }

    /// <inheritdoc />
    public override bool TryEvaluate(DynamicResources resources, IList<IValue> args, out IValue element)
    {
        if (!args.Any())
        {
            element = JsonConstants.Null;
            return false;
        }

        var arg0 = args[0];
        if (arg0.Type != JmesPathType.Object)
        {
            element = JsonConstants.Null;
            return false;
        }

        if (args.Count == 1)
        {
            element = arg0;
            return true;
        }

        var dict = new Dictionary<string, IValue>();
        foreach (var argi in args)
        {
            if (argi.Type != JmesPathType.Object)
            {
                element = JsonConstants.Null;
                return false;
            }

            foreach (var item in argi.EnumerateObject())
            {
                if (dict.TryAdd(item.Name, item.Value)) continue;
                dict.Remove(item.Name);
                dict.Add(item.Name, item.Value);
            }
        }

        element = new ObjectValue(dict);
        return true;
    }

    public override string ToString()
    {
        return "merge";
    }
}