FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY src/MessageService.sln ./
COPY src/MessageService.Api/MessageService.Api.csproj MessageService.Api/
COPY src/MessageService.Dal/MessageService.Dal.csproj MessageService.Dal/
RUN dotnet restore MessageService.Api/MessageService.Api.csproj

COPY src/ ./
WORKDIR /app/MessageService.Api
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "MessageService.Api.dll"]
