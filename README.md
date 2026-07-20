# WhatsApp Sending Service

API em **.NET 9** que recebe um DTO e dispara mensagens de WhatsApp de forma
assíncrona e desacoplada, seguindo **DDD + Clean Architecture**. A camada de
Infrastructure entrega as mensagens através de um adaptador plugável
(`IWhatsAppSender`) — implementado por padrão sobre a **WhatsApp Cloud API (Meta)**.

> ⚠️ **Nota importante sobre "sem provider"**
> Diferente de e-mail (SMTP é um protocolo aberto), o WhatsApp usa um protocolo
> **proprietário, fechado e criptografado fim-a-fim** controlado pela Meta. Não
> existe forma nativa/legal de disparar mensagens "100% em C# sem provider": as
> únicas alternativas sem a API oficial são automações não-oficiais do WhatsApp
> Web (Baileys, WPPConnect, Playwright), que **violam os Termos de Uso** e correm
> risco de banimento. Por isso o adaptador padrão é a **Cloud API oficial**. A
> arquitetura, porém, isola o "motor de envio" atrás da porta `IWhatsAppSender`,
> então trocar o provedor não exige tocar no núcleo da aplicação.

---

## Arquitetura (Clean Architecture / DDD)

```
                +------------------------------------------+
                |                  Api                     |  ASP.NET Core, Controllers, DTOs
                +---------------------+--------------------+
                                      | depende de
                +---------------------v--------------------+
                |             Infrastructure               |  EF Core (SQL Server), Cloud API
                |  ApplicationDbContext, Repositories,     |  adapter, Outbox dispatcher
                |  UnitOfWork, WhatsAppCloudApiSender      |
                +---------------------+--------------------+
                                      | depende de
                +---------------------v--------------------+
                |               Application                |  CQRS (MediatR), Validators,
                |  Commands/Queries, Handlers, Ports,      |  Ports (IWhatsAppSender, IUnitOfWork)
                |  ValidationBehavior, Result pattern      |
                +---------------------+--------------------+
                                      | depende de
                +---------------------v--------------------+
                |                 Domain                   |  Aggregate WhatsAppMessage, Value
                |  WhatsAppMessage, PhoneNumber,           |  Objects, Domain Events, invariantes
                |  MessageContent, DomainEvents            |  (ZERO dependências externas)
                +------------------------------------------+
```

A **regra de dependência** (dependências apontam sempre para dentro) é validada
automaticamente pelos **ArchTests**. O `Domain` não referencia nenhum framework
(nem EF Core, nem MediatR).

### Fluxo de uma mensagem (Outbox Pattern)

1. `POST /api/v1/messages` recebe o DTO `SendMessageRequest`.
2. O `SendWhatsAppMessageCommandHandler` cria o agregado `WhatsAppMessage`
   (estado `Pending`) e persiste — a API responde **202 Accepted** rapidamente.
3. O `OutboxDispatcherBackgroundService` (hosted service) dispara periodicamente
   o `DispatchPendingMessagesCommand`, que lê as mensagens pendentes e chama o
   `IWhatsAppSender`.
4. Conforme o resultado, o agregado transiciona para `Sent` (com `ProviderMessageId`)
   ou `Failed` (com motivo), emitindo os respectivos **Domain Events**.

Esse desacoplamento dá **resiliência** (reprocessável), **baixa latência** na API
e **troca de provedor** sem impacto no núcleo.

---

## Estrutura de pastas

```
WhatsAppSendingService.sln
Directory.Build.props            # TFM, nullable, implicit usings (todos os projetos)
Directory.Packages.props         # Central Package Management (versões centralizadas)
src/
  WhatsAppSendingService.Domain
  WhatsAppSendingService.Application
  WhatsAppSendingService.Infrastructure
  WhatsAppSendingService.Api
tests/
  WhatsAppSendingService.UnitTests   # xUnit + FluentAssertions + NSubstitute (20 testes)
  WhatsAppSendingService.BddTests     # Reqnroll (Gherkin) (3 cenários)
  WhatsAppSendingService.ArchTests    # NetArchTest (9 regras de arquitetura)
```

---

## Como rodar

### Pré-requisitos
- .NET SDK 9.0
- SQL Server (local, Docker ou Azure) — **ou** rode em modo InMemory para demo.

### 1) Restaurar e compilar
```bash
dotnet build
```

### 2) Rodar todos os testes (32 no total)
```bash
dotnet test
```

### 3) Rodar a API

**Modo demo (sem banco, InMemory):**
```bash
Persistence__Provider=InMemory dotnet run --project src/WhatsAppSendingService.Api
```

**Modo produção (SQL Server):** configure a connection string em
`appsettings.json` (`ConnectionStrings:Database`). As migrations são aplicadas
automaticamente em `Development`, ou manualmente:
```bash
dotnet ef database update \
  --project src/WhatsAppSendingService.Infrastructure \
  --startup-project src/WhatsAppSendingService.Api
```

Swagger disponível em `/swagger` (ambiente Development).

### 4) Testar os endpoints
```bash
# Enfileirar uma mensagem
curl -X POST http://localhost:5080/api/v1/messages \
  -H "Content-Type: application/json" \
  -d '{"to":"+55 11 99999-8888","body":"Ola!"}'
# -> 202 { "messageId": "...", "status": "Pending" }

# Consultar status
curl http://localhost:5080/api/v1/messages/{id}
```

---

## Configuração da WhatsApp Cloud API

Em `appsettings.json`:

```json
"WhatsApp": {
  "CloudApi": {
    "BaseUrl": "https://graph.facebook.com",
    "ApiVersion": "v21.0",
    "PhoneNumberId": "SEU_PHONE_NUMBER_ID",
    "AccessToken": "SEU_ACCESS_TOKEN"
  },
  "OutboxDispatcher": { "Enabled": true, "PollingIntervalSeconds": 10, "BatchSize": 20 }
}
```

`PhoneNumberId` e `AccessToken` vêm do painel Meta for Developers (WhatsApp > API Setup).
Use um **System User token permanente** em produção.

### Trocar de provedor
Basta implementar `IWhatsAppSender` (ex.: `BaileysGatewaySender`, `PlaywrightWebSender`)
e registrá-lo no lugar de `WhatsAppCloudApiSender` em
`Infrastructure/DependencyInjection.cs`. Nenhuma outra camada muda.

---

## Padrões e práticas aplicados

- **DDD**: Aggregate Root (`WhatsAppMessage`) com invariantes protegidas, Value
  Objects (`PhoneNumber`, `MessageContent`), Domain Events, factory methods.
- **Clean Architecture**: dependências apontando para dentro, portas/adaptadores
  (Dependency Inversion), Domain sem dependências externas.
- **CQRS** com MediatR (Commands/Queries separados) + **Pipeline Behavior** de
  validação (FluentValidation).
- **Result pattern** (sem exceções para fluxo de erro esperado).
- **Transactional Outbox** + background dispatcher para entrega resiliente.
- **Central Package Management** (`Directory.Packages.props`).
- **ProblemDetails** (RFC 7807) para erros HTTP.
- **Testes**: Unit (domínio + handlers), BDD (Gherkin/Reqnroll ponta-a-ponta),
  Architecture (NetArchTest garantindo as regras de camadas).
