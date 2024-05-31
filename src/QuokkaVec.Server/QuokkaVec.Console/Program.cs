// See https://aka.ms/new-console-template for more information
using Amazon;
using Amazon.BedrockAgentRuntime.Model;
using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using Amazon.Util;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.TextGeneration;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

Console.WriteLine("Hello, Quokkas!");
#pragma warning disable SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

// configuration init
var builder = new ConfigurationBuilder()
			//.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
			.AddUserSecrets<Program>() // TODO do this only in development
			.AddEnvironmentVariables();

IConfigurationRoot configuration = builder.Build();

// Amazon Bedrock experiment
// in dev-time thise uses user secrets
// in prod-time this uses environment variables
//string awsAccessKeyId = configuration["AWSAccessKeyId"] ?? string.Empty;

// use amazon sdk to retreive data from knowledge base
var bedrockAgentClient = new Amazon.BedrockAgentRuntime.AmazonBedrockAgentRuntimeClient(RegionEndpoint.USEast1);
var r = await bedrockAgentClient.RetrieveAndGenerateAsync(new RetrieveAndGenerateRequest { Input = "Hello, Quokkas!" });
Console.WriteLine(r.Output);


// TODO: use Semantic Kernel with custom model for LocalAI Embeddings
// https://github.com/microsoft/semantic-kernel/blob/main/dotnet/README.md


#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning restore SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.