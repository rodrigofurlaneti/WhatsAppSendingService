using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WhatsAppSendingService.Domain.Common;

namespace WhatsAppSendingService.Api.Extensions;

public static class ResultExtensions
{
    public static ProblemDetails ToProblemDetails(this Error error)
    {
        var status = error.Code switch
        {
            "Validation" => StatusCodes.Status400BadRequest,
            "NotFound" => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        };

        return new ProblemDetails
        {
            Status = status,
            Title = error.Code,
            Detail = error.Message,
            Type = $"https://httpstatuses.io/{status}"
        };
    }
}
