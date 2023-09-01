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

using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Lambda.SQSEvents;
using AWS.Lambda.Powertools.BatchProcessing.Tests.Handlers.SQS;
using Xunit;

namespace AWS.Lambda.Powertools.BatchProcessing.Tests.Handlers;

public class HandlerTests
{
    [Fact]
    public async Task SqsHandlerUsingAttribute()
    {
        var request = new SQSEvent
        {
            Records = new List<SQSEvent.SQSMessage>
            {
                new()
                {
                    MessageId = "1",
                    Body = "{\"Id\":1,\"Name\":\"product-4\",\"Price\":14}",
                    EventSourceArn = "arn:aws:sqs:us-east-2:123456789012:my-queue"
                },
                new()
                {
                    MessageId = "2",
                    Body = "fail",
                    EventSourceArn = "arn:aws:sqs:us-east-2:123456789012:my-queue"
                },
                new()
                {
                    MessageId = "3",
                    Body = "{\"Id\":3,\"Name\":\"product-4\",\"Price\":14}",
                    EventSourceArn = "arn:aws:sqs:us-east-2:123456789012:my-queue"
                },
                new()
                {
                    MessageId = "4",
                    Body = "{\"Id\":4,\"Name\":\"product-4\",\"Price\":14}",
                    EventSourceArn = "arn:aws:sqs:us-east-2:123456789012:my-queue"
                },
                new()
                {
                    MessageId = "5",
                    Body = "{\"Id\":5,\"Name\":\"product-4\",\"Price\":14}",
                    EventSourceArn = "arn:aws:sqs:us-east-2:123456789012:my-queue"
                },
            }
        };


        var function = new SQSHandlerFunction();

        var response = function.SqsHandlerUsingAttribute(request);
        
        Assert.Equal(2, response.BatchItemFailures.Count);
        Assert.Equal("2", response.BatchItemFailures[0].ItemIdentifier);
        Assert.Equal("4", response.BatchItemFailures[1].ItemIdentifier);
    }
}