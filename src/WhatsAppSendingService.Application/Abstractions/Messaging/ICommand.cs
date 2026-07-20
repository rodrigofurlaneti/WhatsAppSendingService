using MediatR;
using WhatsAppSendingService.Domain.Common;

namespace WhatsAppSendingService.Application.Abstractions.Messaging;

public interface ICommand : IRequest<Result>;
public interface ICommand<TResponse> : IRequest<Result<TResponse>>;
