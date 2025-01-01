# Use the official .NET SDK image as a build stage
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

# Set the working directory inside the container
WORKDIR /app

# Copy the project file and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy the rest of the application code
COPY . ./

# Build the application
RUN dotnet publish -c Release -o out

# Use the official .NET runtime image as a runtime stage
FROM mcr.microsoft.com/dotnet/runtime:7.0

# Set the working directory inside the container
WORKDIR /app

# Copy the built application from the build stage
COPY --from=build /app/out .

# Set environment variables (optional, you can also set them in your code)
ENV AZURE_SQL_CONNECTION_STRING=""
ENV CONTENT_SAFETY_CONNECTION_STRING=""
ENV CONTENT_SAFETY_CONNECTION_KEY=""
ENV COSMOS_DB_CONNECTION_STRING=""
ENV COSMOS_DB_DATABASE_NAME=""
ENV COSMOS_DB_CONTAINER_NAME=""
ENV STORAGE_ACCOUNT_NAME=""
ENV STORAGE_ACCOUNT_KEY=""
ENV STORAGE_ACCOUNT_URL=""
ENV OPEN_AI_DEPLOYMENT_NAME=""
ENV OPEN_AI_KEY=""
ENV OPEN_AI_ENDPOINT=""
ENV OPEN_AI_MODEL=""

# Set the entry point for the application
ENTRYPOINT ["dotnet", "NovelCsam.Console.dll"]