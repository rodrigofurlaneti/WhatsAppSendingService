using MediatR;
using Microsoft.AspNetCore.Mvc;
using WhatsAppSendingService.Api.Contracts;
using WhatsAppSendingService.Api.Extensions;
using WhatsAppSendingService.Application.Messages.Commands.SendMessage;
using WhatsAppSendingService.Application.Messages.Queries.GetMessageById;

namespace WhatsAppSendingService.Api.Controllers;

[ApiController]
[Route("api/v1/messages")]
[Produces("application/json")]
public sealed class MessagesController(ISender sender) : ControllerBase
{
    /// <summary>Queues a WhatsApp message for autonomous delivery.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(SendMessageResponseDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Send(
        [FromBody] SendMessageRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new SendWhatsAppMessageCommand(request.To, request.Body), cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error.ToProblemDetails());

        var response = new SendMessageResponseDto(result.Value.MessageId, result.Value.Status);
        return AcceptedAtAction(nameof(GetById), new { id = response.MessageId }, response);
    }

    /// <summary>Returns the delivery status of a message.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(MessageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetMessageByIdQuery(id), cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : NotFound(result.Error.ToProblemDetails());
    }
}
