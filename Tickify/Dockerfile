FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore TickifyBackendOnly.sln
RUN dotnet build TickifyBackendOnly.sln -c Release -o /app/build
RUN dotnet publish TickifyBackendOnly.sln -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

RUN mkdir -p /app/wwwroot/uploads

ENTRYPOINT ["dotnet", "Tickify.dll"]
