// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using Qdrant.Client;
using Qdrant.Client.Grpc;

Console.WriteLine("Hello, Quokkas!");
#pragma warning disable SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

// configuration init
var builder = new ConfigurationBuilder()
			//.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
			.AddUserSecrets<Program>() // TODO do this only in development
			.AddEnvironmentVariables();

IConfigurationRoot configuration = builder.Build();
// in dev-time thise uses user secrets
// in prod-time this uses environment variables
string openAIAPIKey = configuration["OpenAIAPIKey"] ?? string.Empty;

if (string.IsNullOrWhiteSpace(openAIAPIKey))
{
	Console.WriteLine("OpenAIAPIKey is not set, use 'dotnet user-secrets init' and 'dotnet user-secrets set OpenAIAPIKey <your key>' to set the key, exiting...");
	return;
}

// Semantic Kernel init
var kerbelBuilder = Kernel.CreateBuilder();
kerbelBuilder.AddOpenAITextEmbeddingGeneration("text-embedding-3-small", openAIAPIKey);

var kernel = kerbelBuilder.Build();

var embeddingGenerationService = kernel.GetRequiredService<ITextEmbeddingGenerationService>();

// Vector DB (Qdrant) init
// TODO: look at Semntic Kernel for Qdrant integration
var collectionName = "test";
var qdrantClient = new QdrantClient("localhost");
if (!await qdrantClient.CollectionExistsAsync(collectionName))
{
	await qdrantClient.CreateCollectionAsync(collectionName, new VectorParams { Size = 1536, Distance = Distance.Cosine });
}

// TODO: use Semantic Kernel with custom model for LocalAI Embeddings
// https://github.com/microsoft/semantic-kernel/blob/main/dotnet/README.md

// read input from user, get path to folder to index
Console.WriteLine("Enter path to folder to index:");
var path = Console.ReadLine();

if (string.IsNullOrWhiteSpace(path))
{
	Console.WriteLine("Path is empty, exiting...");
	return;
}
// index all files in the folder
var files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
int i = 0;
foreach (var file in files)
{
	var text = await File.ReadAllTextAsync(file);
	// take only first 1k characters
	text = text.Substring(0, Math.Min(1000, text.Length));
	ReadOnlyMemory<float> embedding = await embeddingGenerationService.GenerateEmbeddingAsync(text);

	var points = new List<PointStruct>()
	{
		new PointStruct
		{
			Id = new PointId { Num = (ulong)i },
			Vectors = embedding.ToArray(),
			Payload = { { "fileName", file } }
		}
	};
	var updateResult = await qdrantClient.UpsertAsync(collectionName, points);
	Console.WriteLine($"File {file} indexed, result: {updateResult}");

	i++;
}

// search test
Console.WriteLine("Enter search query:");
var query = Console.ReadLine();

var queryVector = await embeddingGenerationService.GenerateEmbeddingAsync(query);
var searchResult = await qdrantClient.SearchAsync(collectionName, queryVector.ToArray(), limit: 2);

foreach (var result in searchResult)
{
	Console.WriteLine($"File: {result.Payload["fileName"]}, Score: {result.Score}");
}

#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning restore SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.