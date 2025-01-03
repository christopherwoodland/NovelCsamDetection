# Use the official .NET 8 SDK image to build the project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory
WORKDIR /src

# Copy the solution file
COPY NovelCsamDetection.sln ./

# Copy all project files
#COPY NovelCsamDetection.Tests/NovelCsamDetection.Tests.csproj NovelCsamDetection.Tests/
COPY NovelCsam.Helpers/NovelCsam.Helpers.csproj NovelCsam.Helpers/
COPY NovelCsam.Models/NovelCsam.Models.csproj NovelCsam.Models/
#COPY NovelCsam.Console/NovelCsam.Console.csproj NovelCsam.Console/
#COPY NovelCsam.UI.Console/NovelCsam.UI.Console.csproj NovelCsam.UI.Console/

# Restore dependencies
RUN dotnet restore

# Copy the rest of the project files
COPY . .

# Build the project
WORKDIR /src/NovelCsam.Console
RUN dotnet publish -c Release -o /app/out
#
## Use the official .NET 8 runtime image to run the application
#FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
#
## Set the working directory
#WORKDIR /app
#
## Copy the build output from the build stage
#COPY --from=build /app/out .
#
## Set the entry point for the application
#ENTRYPOINT ["dotnet", "NovelCsam.Console.dll"]