using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace WebApp.Infrastructure.SqliteRequestTracer
{
    public class RequestTraceWriterBackgroundServer : IHostedService
    {
        private readonly RequestTraceWriter _writer;

        public RequestTraceWriterBackgroundServer(RequestTraceWriter writer)
        {
            _writer = writer;
        }

        public async Task StartAsync(CancellationToken cancellationToken) => await _writer.StartAsync(cancellationToken);
        public async Task StopAsync(CancellationToken cancellationToken) => await _writer.StopAsync(cancellationToken);
    }

    public class RequestTraceWriter
    {
        private Channel<Request> _channel = Channel.CreateUnbounded<Request>();
        private Task _task;
        private object _fileName;
        private readonly ILogger<RequestTraceWriter> _logger;

        public RequestTraceWriter(ILogger<RequestTraceWriter> logger, IOptions<RequestTraceOptions> options)
        {
            _fileName = options.Value.FileName;
            _logger = logger;
            
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (_fileName == null)
                return Task.CompletedTask;

            _task = Task.Factory.StartNew(() => Process(), TaskCreationOptions.LongRunning);
            return Task.CompletedTask;
        }

        private async Task Process()
        {
            while (!_channel.Reader.Completion.IsCompleted)
            {
                try
                {
                    await EnsureDbIsInitialized();
                    while (await _channel.Reader.WaitToReadAsync())
                    {
                        using var db = await CreateConnection();
                        using var t = await db.BeginTransactionAsync();
                        while (_channel.Reader.TryRead(out var entry))
                        {
                            using var cmd = db.CreateCommand();
                            cmd.CommandText = "INSERT INTO Requests (Timestamp, Method, Path, QueryString, Protocol, Host, StatusCode, Duration, PerformanceBucket) VALUES (@Timestamp, @Method, @Path, @QueryString, @Protocol, @Host, @StatusCode, @Duration, @PerformanceBucket);";
                            cmd.Parameters.AddWithValue("@Timestamp", entry.Timestamp);
                            cmd.Parameters.AddWithValue("@Method", entry.Method);
                            cmd.Parameters.AddWithValue("@Path", entry.Path);
                            cmd.Parameters.AddWithValue("@QueryString", entry.QueryString);
                            cmd.Parameters.AddWithValue("@Protocol", entry.Protocol);
                            cmd.Parameters.AddWithValue("@Host", entry.Host);
                            cmd.Parameters.AddWithValue("@StatusCode", entry.StatusCode);
                            cmd.Parameters.AddWithValue("@Duration", entry.Duration);
                            cmd.Parameters.AddWithValue("@PerformanceBucket", entry.PerformanceBucket);
                            await cmd.ExecuteNonQueryAsync();
                        }
                        await t.CommitAsync();
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Request trace background service failed.");
                    await Task.Delay(5000);
                }
            }
        }

        private async Task EnsureDbIsInitialized()
        {
            using var con = await CreateConnection();
            using var cmd = con.CreateCommand();

            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Requests (
	                Id                   INTEGER NOT NULL PRIMARY KEY,
                    Timestamp            TEXT    NOT NULL,
                    Method               TEXT    NOT NULL,
                    Path                 TEXT    NOT NULL,
                    QueryString          TEXT    NOT NULL,
                    Protocol             TEXT    NOT NULL,
                    Host                 TEXT    NOT NULL,
                    StatusCode           INTEGER NOT NULL,
                    Duration             DECIMAL NOT NULL,
                    PerformanceBucket    TEXT    NOT NULL
                )
            ";

            await cmd.ExecuteNonQueryAsync();
        }

        private async Task<SqliteConnection> CreateConnection()
        {
            var con = new SqliteConnection($"Data Source={_fileName}");
            await con.OpenAsync();
            return con;
        }


        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _channel.Writer.Complete();

            if (_task != null)
                await _task;
        }

        public async Task Write(Request request)
        {
            await _channel.Writer.WriteAsync(request);
        }
    }

    public record Request(
        DateTime Timestamp,
        string Method,
        string Path,
        string QueryString,
        string Protocol,
        string Host,
        int StatusCode,
        double Duration,
        string PerformanceBucket
    );
}
