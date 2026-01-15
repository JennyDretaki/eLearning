# Use ASP.NET runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# IMPORTANT: Render provides PORT dynamically
ENV ASPNETCORE_URLS=http://+:$PORT

ENTRYPOINT ["dotnet", "ELearning.API.dll"]
