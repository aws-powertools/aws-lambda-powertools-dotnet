﻿using System.Collections.Generic;
using System.Linq;

namespace AWS.Lambda.PowerTools.Metrics
{
    /// <summary>
    /// List of key-value pairs with Metric Dimensions
    /// </summary>
    public class DimensionSet
    {
        internal Dictionary<string, string> Dimensions { get; } = new Dictionary<string, string>(); 

        public DimensionSet(string key, string value)
        {
            Dimensions[key] = value;
        }

        public List<string> DimensionKeys => Dimensions.Keys.ToList();
    }
}