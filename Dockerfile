FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

# No need to expose a fixed port; optional:
# EXPOSE 10000  

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# Tell ASP.NET Core to listen on the port Render assigns
ENV ASPNETCORE_URLS=http://+:$PORT

ENTRYPOINT ["dotnet", "ELearning.API.dll"]
