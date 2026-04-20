FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["EventMarketplace.sln", "."]
COPY ["EventMarketplace.API/EventMarketplace.API.csproj", "EventMarketplace.API/"]
COPY ["EventMarketplace.Application/EventMarketplace.Application.csproj", "EventMarketplace.Application/"]
COPY ["EventMarketplace.Domain/EventMarketplace.Domain.csproj", "EventMarketplace.Domain/"]
COPY ["EventMarketplace.Infrastructure/EventMarketplace.Infrastructure.csproj", "EventMarketplace.Infrastructure/"]
RUN dotnet restore "EventMarketplace.API/EventMarketplace.API.csproj"
COPY . .
RUN dotnet publish "EventMarketplace.API/EventMarketplace.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "EventMarketplace.API.dll"]
