# Use ASP.NET runtime image as the base
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

# Optional: Render handles ports automatically, no need to EXPOSE
# ENV ASPNETCORE_URLS will be set below

# Use .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy all files and restore/build
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

# Final image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
<<<<<<< HEAD
ENV ASPNETCORE_URLS=http://+:10000
ENTRYPOINT ["dotnet", "eLearning.API.dll"]
=======

# Tell ASP.NET Core to listen on Render's dynamic port
ENV ASPNETCORE_URLS=http://+:$PORT

# Run the application
ENTRYPOINT ["dotnet", "ELearning.API.dll"]
>>>>>>> 821451a5134a027d3c4e8ecfae6ef059f72ad772
