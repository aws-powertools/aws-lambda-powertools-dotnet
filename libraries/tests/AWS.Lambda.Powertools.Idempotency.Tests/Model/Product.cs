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

namespace AWS.Lambda.Powertools.Idempotency.Tests.Model;

public class Product : IEquatable<Product>
{
    // ReSharper disable once MemberCanBePrivate.Global
    public long Id { get; }
    // ReSharper disable once MemberCanBePrivate.Global
    public string Name { get; }
    // ReSharper disable once MemberCanBePrivate.Global
    public double Price { get; }
    
    public Product(long id, string name, double price)
    {
        Id = id;
        Name = name;
        Price = price;
    }

    public bool Equals(Product other)
    {
        if (other == null)
        {
            return false;
        }
        return Id == other.Id && Name == other.Name && Price.Equals(other.Price);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Product) obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Name, Price);
    }
}