using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Microsoft.Extensions.DependencyInjection;
using Repository;
using Shared;
using System.Text.Json;

namespace AwsNativeAotReferenceOtherProjects;
public class Function
{
    private static readonly ICustomerRepository _customerRepository;

    public static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IAmazonDynamoDB>(new AmazonDynamoDBClient());
        services.AddTransient<ICustomerRepository, CustomerRepository>();
        return services.BuildServiceProvider();
    }

    static Function()
    {
        var provider = ConfigureServices();
        _customerRepository = provider.GetRequiredService<ICustomerRepository>();
    }

    /// <summary>
    /// The main entry point for the Lambda function. The main function is called once during the Lambda init phase. It
    /// initializes the .NET Lambda runtime client passing in the function handler to invoke for each Lambda event and
    /// the JSON serializer to use for converting Lambda JSON format to the .NET types. 
    /// </summary>
    private static async Task Main()
    {
        Func<string, ILambdaContext, Task<APIGatewayHttpApiV2ProxyResponse>> handler = FunctionHandlerAsync;
        await LambdaBootstrapBuilder.Create(handler, new SourceGeneratorLambdaJsonSerializer<LambdaFunctionJsonSerializerContext>(o =>
        {
            o.PropertyNameCaseInsensitive = true;
        }))
            .Build()
            .RunAsync();
    }

    /// <summary>
    /// To use this handler to respond to an AWS event, reference the appropriate package from 
    /// https://github.com/aws/aws-lambda-dotnet#events
    /// and change the string input parameter to the desired event type. When the event type
    /// is changed, the handler type registered in the main method needs to be updated and the LambdaFunctionJsonSerializerContext 
    /// defined below will need the JsonSerializable updated. If the return type and event type are different then the 
    /// LambdaFunctionJsonSerializerContext must have two JsonSerializable attributes, one for each type.
    ///
    // When using Native AOT extra testing with the deployed Lambda functions is required to ensure
    // the libraries used in the Lambda function work correctly with Native AOT. If a runtime 
    // error occurs about missing types or methods the most likely solution will be to remove references to trim-unsafe 
    // code or configure trimming options. This sample defaults to partial TrimMode because currently the AWS 
    // SDK for .NET does not support trimming. This will result in a larger executable size, and still does not 
    // guarantee runtime trimming errors won't be hit. 
    /// </summary>
    public static async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandlerAsync(string input, ILambdaContext context)
    {
        context.Logger.LogInformation("Starting to process request");
        var customer = await _customerRepository.GetAsync(Guid.Parse("f702dc75-1be9-4c15-98ba-73c5956d09af"));

        var text = JsonSerializer.Serialize(customer, LambdaFunctionJsonSerializerContext.Default.Customer!);

        return new APIGatewayHttpApiV2ProxyResponse()
        {
            StatusCode = 200,
            Body = text,
        };
    }
}
