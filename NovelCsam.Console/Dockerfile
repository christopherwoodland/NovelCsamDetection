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
ENV AZURE_SQL_CONNECTION_STRING="Server=tcp:df-sql-server-eastus-001.database.windows.net,1433;Initial Catalog=df-sql-eastus-001;Persist Security Info=False;User ID=cwoodland;Password=django20221!!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
ENV CONTENT_SAFETY_CONNECTION_STRING="https://cwcontentsafetyeastus2001.cognitiveservices.azure.com/"
ENV CONTENT_SAFETY_CONNECTION_KEY="2b69c5c74f0642c9bc5cf713111e5bf9"
ENV COSMOS_DB_CONNECTION_STRING="AccountEndpoint=https://igital-forensics-eastus-001.documents.azure.com:443/;AccountKey=QGcPlwvi2dq5gFSJheXIF9rupVC1vh6bQqqucgkuU4cGh7O5bQCsV7FdZZS2wFJWyNetwhouv27xACDbTrvj2A==;"
ENV COSMOS_DB_DATABASE_NAME="Forensics"
ENV COSMOS_DB_CONTAINER_NAME="Images"
ENV STORAGE_ACCOUNT_NAME="digitalforensicsstg"
ENV STORAGE_ACCOUNT_KEY="IGnMXV0C6jZrPX3RapNgHDtfY0OI8M20Y9K+qOG6Zm0mvvRdzUJOHHIkae0VjTKoIWRugb82cFi7+AStRuP/MQ=="
ENV STORAGE_ACCOUNT_URL="https://digitalforensicsstg.dfs.core.windows.net"
ENV OPEN_AI_DEPLOYMENT_NAME="gpt-4o"
ENV OPEN_AI_KEY="89a35462495b4448b433e57d092397e3"
ENV OPEN_AI_ENDPOINT="https://openai-sesame-eastus-001.openai.azure.com/"
ENV OPEN_AI_MODEL="gpt-4o"

# Set the entry point for the application
ENTRYPOINT ["dotnet", "NovelCsam.Console.dll"]