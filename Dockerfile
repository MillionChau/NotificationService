FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY ["src/Notification.Domain/Notification.Domain.csproj", "src/Notification.Domain/"]
COPY ["src/Notification.Application/Notification.Application.csproj", "src/Notification.Application/"]
COPY ["src/Notification.Infrastructure/Notification.Infrastructure.csproj", "src/Notification.Infrastructure/"]
COPY ["src/Notification.API/Notification.API.csproj", "src/Notification.API/"]

RUN dotnet restore "src/Notification.API/Notification.API.csproj"

COPY . .
WORKDIR "/app/src/Notification.API"
RUN dotnet publish "Notification.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Notification.API.dll"]
