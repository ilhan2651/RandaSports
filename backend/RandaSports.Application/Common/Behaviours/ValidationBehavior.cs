using System.Net;
using FluentValidation;
using MediatR;
using RandaSports.Application.Common.Wrappers;

namespace RandaSports.Application.Common.Behaviours;

public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result, new()
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);
        var results = await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var messages = results
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .Select(f => f.ErrorMessage)
            .ToList();

        if (messages.Count == 0)
            return await next();

        return new TResponse
        {
            Success = false,
            StatusCode = HttpStatusCode.BadRequest,
            Messages = messages
        };
    }
}
