using FluentValidation;
using MediatR;
using WhatsAppSendingService.Domain.Common;

namespace WhatsAppSendingService.Application.Behaviors;

/// <summary>Cross-cutting pipeline: runs FluentValidation before the handler.
/// On failure it short-circuits into a failed Result instead of throwing,
/// keeping the happy path clean.</summary>
public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);
        var failures = validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count == 0)
            return await next();

        var message = string.Join(" ", failures.Select(f => f.ErrorMessage).Distinct());
        var error = Error.Validation(message);

        return CreateValidationResult<TResponse>(error);
    }

    private static TResult CreateValidationResult<TResult>(Error error)
        where TResult : Result
    {
        if (typeof(TResult) == typeof(Result))
            return (Result.Failure(error) as TResult)!;

        // TResult is Result<TValue> -> build a failed Result<TValue> via reflection-free helper.
        var valueType = typeof(TResult).GetGenericArguments()[0];
        var method = typeof(Result)
            .GetMethods()
            .First(m => m.Name == nameof(Result.Failure) && m.IsGenericMethod)
            .MakeGenericMethod(valueType);

        return (TResult)method.Invoke(null, new object[] { error })!;
    }
}
