using Amazon.EventBridge;
using Amazon.EventBridge.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.CloudWatchEvents;
using Amazon.Lambda.Core;
using System.Text.Json;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace MyLambda;

public class Function
{
    private readonly AmazonEventBridgeClient _eventBridgeClient;

    public Function()
    {
        _eventBridgeClient = new AmazonEventBridgeClient();
    }

    public record Payload(string Key);

    public async Task<APIGatewayProxyResponse> Produce(APIGatewayProxyRequest input, ILambdaContext context)
    {
        var putEventsRequest = new PutEventsRequest
        {
            Entries = new List<PutEventsRequestEntry> 
            {
                new PutEventsRequestEntry
                {
                    Source = "myapplication",
                    DetailType = "mycustomevent",
                    Detail = JsonSerializer.Serialize(new Payload(Guid.NewGuid().ToString())),
                    EventBusName = "MyEventBus"
                }
            }
        };

        var response = await _eventBridgeClient.PutEventsAsync(putEventsRequest);
        if (response.FailedEntryCount > 0)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }
        else
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }
    }

    public async Task Consume(CloudWatchEvent<Payload> input, ILambdaContext context)
    {
        context.Logger.LogLine("Event Source: " + input.Source);
        context.Logger.LogLine("Event Detail Type: " + input.DetailType);
        context.Logger.LogLine("Event Detail: " + input.Detail.Key);
        await Task.CompletedTask;
    }
}
