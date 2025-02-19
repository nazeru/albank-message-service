FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ./MessageService.sln .
COPY . ./MessageService.Api/

WORKDIR /src/MessageService.Api
RUN dotnet restore

RUN dotnet publish -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "MessageService.Api.dll"]
