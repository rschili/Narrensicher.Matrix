﻿using Narrensicher.Matrix;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Narrensicher.Matrix.Models;
using Narrensicher.Matrix.Http;

// load environment variables or a .env file
DotNetEnv.Env.TraversePath().Load();
var userid = Environment.GetEnvironmentVariable("MATRIX_USER_ID");
var password = Environment.GetEnvironmentVariable("MATRIX_PASSWORD");
var device = Environment.GetEnvironmentVariable("MATRIX_DEVICE_ID");
if (string.IsNullOrWhiteSpace(userid) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(device))
{
    throw new ArgumentException("Please provide the required environment variables: MATRIX_USER_ID, MATRIX_PASSWORD, MATRIX_DEVICE_ID");
}

//set up dependency injection
var services = new ServiceCollection()
    .AddHttpClient()
    .AddLogging(logging =>
    {
        logging.AddSimpleConsole(options =>
        {
            options.IncludeScopes = true;
            options.SingleLine = true;
            options.TimestampFormat = "hh:mm:ss ";
        });
        logging.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning); // Filter logs from HttpClient
    })
    .BuildServiceProvider();

//Using CancellationToken as a shutdown mechanism
var cancellationTokenSource = new CancellationTokenSource();
Console.CancelKeyPress += (sender, e) =>
{ // allows shutting down the app using Ctrl+C
    e.Cancel = true;
    cancellationTokenSource.Cancel();
};

try
{
    var client = await MatrixTextClient.ConnectAsync(userid, password, device,
        services.GetRequiredService<IHttpClientFactory>(), cancellationTokenSource.Token,
        services.GetRequiredService<ILogger<MatrixTextClient>>());

    client.DebugMode = true;
    await client.SyncAsync(MessageReceivedAsync);
    Console.WriteLine("Sync has ended.");
}
catch (OperationCanceledException)
{
    Console.WriteLine("Shutdown requested...");
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
finally
{
    await services.DisposeAsync();
}


Task MessageReceivedAsync(MatrixTextMessage message)
{
    Console.WriteLine(message.Body);
    return Task.CompletedTask;
}