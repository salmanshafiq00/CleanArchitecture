using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Application.Common.Abstractions;
using Domain.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Infrastructure.Persistence.Outbox;

public class ProcessOutboxMessagesJob(
    ISqlConnectionFactory sqlConnectionFactory,
    IPublisher publisher,
    ILogger<ProcessOutboxMessagesJob> logger) : IProcessOutboxMessagesJob
{
    private const int BatchSize = 10;
    private const int MaxRetryAttempts = 3;

    private static readonly JsonSerializerSettings SerializerSettings = new()
    {
        TypeNameHandling = TypeNameHandling.All,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
    };

    public async Task ProcessOutboxMessagesAsync()
    {
        try
        {
            logger.LogInformation("Starting outbox message processing");

            using var connection = sqlConnectionFactory.GetOpenConnection();
            using var transaction = connection.BeginTransaction();

            var outboxMessages = await GetUnprocessedMessagesAsync(connection, transaction);

            if (!outboxMessages.Any())
            {
                logger.LogInformation("No unprocessed outbox messages found");
                return;
            }

            foreach (var message in outboxMessages)
            {
                await ProcessSingleMessageAsync(message, connection, transaction);
            }

            transaction.Commit();
            logger.LogInformation("Completed processing outbox messages batch");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fatal error in outbox message processing");
            throw;
        }
    }

    private async Task ProcessSingleMessageAsync(
        OutboxMessage message,
        IDbConnection connection,
        IDbTransaction transaction)
    {
        try
        {
            // Attempt to acquire lock for processing
            if (!await AcquireProcessingLockAsync(message.Id, connection, transaction))
            {
                logger.LogInformation("Message {MessageId} is being processed by another instance", message.Id);
                return;
            }

            var success = await ExecuteWithRetryAsync(async () =>
            {
                var domainEvent = JsonConvert.DeserializeObject<BaseEvent>(message.Content, SerializerSettings);

                if (domainEvent == null)
                {
                    throw new InvalidOperationException($"Failed to deserialize message {message.Id}");
                }

                await publisher.Publish(domainEvent);
            }, message.Id);

            if (success)
            {
                await MarkMessageAsProcessedAsync(
                    message.Id,
                    connection,
                    transaction);

                logger.LogInformation(
                    "Successfully processed outbox message {MessageId} of type {EventType}",
                    message.Id,
                    message.Type);
            }
        }
        catch (Exception ex)
        {
            await UpdateMessageErrorAsync(
                message.Id,
                ex.ToString(),
                connection,
                transaction);

            logger.LogError(
                ex,
                "Failed to process outbox message {MessageId} after all retry attempts",
                message.Id);
        }
    }

    private async Task<bool> ExecuteWithRetryAsync(Func<Task> action, Guid messageId)
    {
        var currentRetry = 0;
        var retryDelay = TimeSpan.FromSeconds(5); // Initial delay

        while (currentRetry < MaxRetryAttempts)
        {
            try
            {
                await action();
                return true;
            }
            catch (Exception ex)
            {
                currentRetry++;

                if (currentRetry >= MaxRetryAttempts)
                {
                    logger.LogError(ex,
                        "Final retry attempt failed for message {MessageId}. Total attempts: {RetryCount}",
                        messageId,
                        currentRetry);
                    throw;
                }

                logger.LogWarning(ex,
                    "Retry attempt {RetryCount} for message {MessageId} failed. Waiting {DelaySeconds} seconds before next attempt",
                    currentRetry,
                    messageId,
                    retryDelay.TotalSeconds);

                await Task.Delay(retryDelay);
                retryDelay *= 2; // Double the delay for next retry (exponential backoff)
            }
        }

        return false;
    }

    private async Task<IReadOnlyList<OutboxMessage>> GetUnprocessedMessagesAsync(
        IDbConnection connection,
        IDbTransaction transaction)
    {
        const string sql = """
            SELECT TOP (@BatchSize) 
                Id, Type, Content, CreatedOn, ProcessedOn, Error, RetryCount
            FROM OutboxMessage WITH (READPAST)
            WHERE ProcessedOn IS NULL 
                AND (RetryCount < @MaxRetryAttempts OR RetryCount IS NULL)
                AND ProcessingLock IS NULL
                AND (LastProcessingAttempt IS NULL OR DATEADD(MINUTE, 5, LastProcessingAttempt) < GETUTCDATE())
            ORDER BY CreatedOn
            """;

        var messages = await connection.QueryAsync<OutboxMessage>(
            sql,
            new { BatchSize, MaxRetryAttempts },
            transaction);

        return messages.ToList();
    }

    private async Task<bool> AcquireProcessingLockAsync(
        Guid messageId,
        IDbConnection connection,
        IDbTransaction transaction)
    {
        const string sql = """
            UPDATE OutboxMessage
            SET 
                ProcessingLock = @LockId,
                LastProcessingAttempt = @Now,
                RetryCount = ISNULL(RetryCount, 0) + 1
            WHERE Id = @MessageId 
                AND ProcessingLock IS NULL
                AND (LastProcessingAttempt IS NULL OR DATEADD(MINUTE, 5, LastProcessingAttempt) < @Now)
            """;

        var lockId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var updatedRows = await connection.ExecuteAsync(
            sql,
            new { LockId = lockId, MessageId = messageId, Now = now },
            transaction);

        return updatedRows > 0;
    }

    private async Task MarkMessageAsProcessedAsync(
        Guid messageId,
        IDbConnection connection,
        IDbTransaction transaction)
    {
        const string sql = """
            UPDATE OutboxMessage
            SET 
                ProcessedOn = @Now,
                ProcessingLock = NULL,
                Error = NULL
            WHERE Id = @MessageId
            """;

        await connection.ExecuteAsync(
            sql,
            new { MessageId = messageId, Now = DateTime.UtcNow },
            transaction);
    }

    private async Task UpdateMessageErrorAsync(
        Guid messageId,
        string error,
        IDbConnection connection,
        IDbTransaction transaction)
    {
        const string sql = """
            UPDATE OutboxMessage
            SET 
                Error = @Error,
                ProcessingLock = NULL
            WHERE Id = @MessageId
            """;

        await connection.ExecuteAsync(
            sql,
            new { MessageId = messageId, Error = error },
            transaction);
    }
}

