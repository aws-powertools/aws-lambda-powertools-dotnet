﻿/*
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
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.KinesisEvents;
using AWS.Lambda.Powertools.BatchProcessing;
using AWS.Lambda.Powertools.BatchProcessing.Kinesis;
using AWS.Lambda.Powertools.Logging;
using HelloWorld.Data;

namespace HelloWorld.Kinesis;
internal class CustomKinesisEventRecordHandler : IKinesisEventRecordHandler
{
    public async Task<RecordHandlerResult> HandleAsync(KinesisEvent.KinesisEventRecord record, CancellationToken cancellationToken)
    {
        Logger.LogInformation($"Handling Kinesis record with sequence number: '{record.Kinesis.SequenceNumber}'.");
        
        var product = JsonSerializer.Deserialize<Product>(record.Kinesis.Data);
        
        Logger.LogInformation($"Handling product with id: {product!.Id}");

        if (product.Id == 4)
        {
            throw new ArgumentException("Error on id 4");
        }
        
        return await Task.FromResult(RecordHandlerResult.None);
    }
}
