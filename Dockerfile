# ===== Estágio 1: build =====
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Arquivos de solução + gerenciamento central de pacotes (necessários para o restore)
COPY Directory.Build.props Directory.Packages.props nuget.config WhatsAppSendingService.sln ./
COPY src/WhatsAppSendingService.Domain/WhatsAppSendingService.Domain.csproj                 src/WhatsAppSendingService.Domain/
COPY src/WhatsAppSendingService.Application/WhatsAppSendingService.Application.csproj         src/WhatsAppSendingService.Application/
COPY src/WhatsAppSendingService.Infrastructure/WhatsAppSendingService.Infrastructure.csproj   src/WhatsAppSendingService.Infrastructure/
COPY src/WhatsAppSendingService.Api/WhatsAppSendingService.Api.csproj                         src/WhatsAppSendingService.Api/

RUN dotnet restore src/WhatsAppSendingService.Api/WhatsAppSendingService.Api.csproj

COPY src/ src/
RUN dotnet publish src/WhatsAppSendingService.Api/WhatsAppSendingService.Api.csproj \
    -c Release -o /app/publish /p:UseAppHost=false

# ===== Estágio 2: runtime =====
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=20s --retries=3 \
  CMD wget -qO- http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "WhatsAppSendingService.Api.dll"]
