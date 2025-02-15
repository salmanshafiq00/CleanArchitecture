using Application.Common.Abstractions.Identity;
using Domain.Shared;
using Serilog.Context;

namespace Application.Common.Behaviours;

internal sealed class RequestLoggingBehaviour<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IBaseRequest
    where TResponse : Result
{
    private readonly ILogger _logger;
    private readonly IUser _user;
    private readonly IIdentityService _identityService;

    public RequestLoggingBehaviour(ILogger<TRequest> logger, IUser user, IIdentityService identityService)
    {
        _logger = logger;
        _user = user;
        _identityService = identityService;

    }
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = _user.Id ?? string.Empty;
        string? userName = string.Empty;

        if (!string.IsNullOrEmpty(userId))
        {
            userName = await _identityService.GetUserNameAsync(userId);
        }

        _logger.LogInformation("Processing Request ({Name}): {@UserId} {@UserName} {@Request}",
            requestName, userId, userName, request);

        TResponse result = await next().ConfigureAwait(false);

        if (result.IsSuccess)
        {
            _logger.LogInformation("Completed Request ({@RequestName}): {@DateTime}, {@Result}", requestName, DateTime.Now, result);
        }
        else
        {
            if (result is IValidationResult validationResult)
            {
                foreach (var error in validationResult.Errors)
                {
                    _logger.LogError("Validation Error: {@Error}", error);
                }
            }

            using (LogContext.PushProperty("Error", result.Error, true))
            {
                _logger.LogError("Completed Request with Failure ({RequestName}): {ErrorCode}", requestName, result.Error.Code);
            }

        }

        return result;
    }
}
