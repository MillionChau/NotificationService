using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Notification.Application.Common.Interfaces;
using Notification.Domain.Contracts;
using Notification.Domain.Enums;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using NotificationEntity = Notification.Domain.Entities.Notification;

namespace Notification.Infrastructure.Services;

public class RabbitMqNotificationConsumer : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RabbitMqNotificationConsumer> _logger;
    private readonly string _exchangeName;
    private readonly string _queueName = "notification.events";
    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitMqNotificationConsumer(
        IConfiguration configuration,
        IServiceScopeFactory scopeFactory,
        ILogger<RabbitMqNotificationConsumer> logger)
    {
        _configuration = configuration;
        _scopeFactory = scopeFactory;
        _logger = logger;
        _exchangeName = _configuration["RabbitMQ:Exchange"] ?? "devradar.events";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _configuration["RabbitMQ:HostName"] ?? "localhost",
                    Port = int.TryParse(_configuration["RabbitMQ:Port"], out var port) ? port : 5672,
                    UserName = _configuration["RabbitMQ:UserName"] ?? "devradar",
                    Password = _configuration["RabbitMQ:Password"] ?? "devradar2026"
                };

                _logger.LogInformation("NotificationService connecting to RabbitMQ at {Host}:{Port}...", 
                    factory.HostName, factory.Port);

                _connection = await factory.CreateConnectionAsync(stoppingToken);
                _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

                await _channel.ExchangeDeclareAsync(
                    exchange: _exchangeName,
                    type: ExchangeType.Topic,
                    durable: true,
                    autoDelete: false,
                    cancellationToken: stoppingToken);

                await _channel.QueueDeclareAsync(
                    queue: _queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    cancellationToken: stoppingToken);

                await _channel.QueueBindAsync(
                    queue: _queueName,
                    exchange: _exchangeName,
                    routingKey: "post.created",
                    cancellationToken: stoppingToken);

                await _channel.QueueBindAsync(
                    queue: _queueName,
                    exchange: _exchangeName,
                    routingKey: "comment.created",
                    cancellationToken: stoppingToken);

                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.ReceivedAsync += async (sender, ea) =>
                {
                    try
                    {
                        var body = ea.Body.ToArray();
                        var json = Encoding.UTF8.GetString(body);
                        var routingKey = ea.RoutingKey;

                        _logger.LogInformation("NotificationService received event with routing key '{RoutingKey}'", routingKey);

                        await ProcessMessageAsync(routingKey, json, stoppingToken);

                        if (_channel != null && _channel.IsOpen)
                        {
                            await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing event in NotificationService consumer.");
                        if (_channel != null && _channel.IsOpen)
                        {
                            await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
                        }
                    }
                };

                await _channel.BasicConsumeAsync(
                    queue: _queueName,
                    autoAck: false,
                    consumer: consumer,
                    cancellationToken: stoppingToken);

                _logger.LogInformation("NotificationService listening on queue '{Queue}' bound to '{Exchange}'", 
                    _queueName, _exchangeName);

                // Keep alive until cancelled
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "NotificationService RabbitMQ connection error. Retrying in 5 seconds...");
                await Task.Delay(5000, stoppingToken);
            }
        }
    }

    private async Task ProcessMessageAsync(string routingKey, string json, CancellationToken cancellationToken)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var eventType = root.TryGetProperty("eventType", out var et) ? et.GetString() : routingKey;

        if (!root.TryGetProperty("data", out var data))
        {
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
        var realTimeNotifier = scope.ServiceProvider.GetService<IRealTimeNotifier>();

        if (routingKey == "post.created")
        {
            if (data.TryGetProperty("postId", out var pid) && Guid.TryParse(pid.GetString(), out var postId) &&
                data.TryGetProperty("authorId", out var aid) && Guid.TryParse(aid.GetString(), out var authorId))
            {
                _logger.LogInformation("Post {PostId} created by {AuthorId}. Processing feed notification subscribers.", postId, authorId);
            }
        }
        else if (routingKey == "comment.created")
        {
            if (data.TryGetProperty("postId", out var pid) && Guid.TryParse(pid.GetString(), out var postId) &&
                data.TryGetProperty("commentId", out var cid) && Guid.TryParse(cid.GetString(), out var commentId) &&
                data.TryGetProperty("authorId", out var aid) && Guid.TryParse(aid.GetString(), out var authorId))
            {
                _logger.LogInformation("Comment {CommentId} created on post {PostId} by author {AuthorId}.", commentId, postId, authorId);
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (_channel != null)
            {
                await _channel.CloseAsync(cancellationToken);
                await _channel.DisposeAsync();
            }
            if (_connection != null)
            {
                await _connection.CloseAsync(cancellationToken);
                await _connection.DisposeAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error closing NotificationService RabbitMQ connection.");
        }
        await base.StopAsync(cancellationToken);
    }
}
