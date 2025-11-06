# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the solution file
COPY CsvInserterService.sln ./

# Copy all project files
COPY CsvInserterService.Api/*.csproj ./CsvInserterService.Api/
COPY CsvInserterService.Application/*.csproj ./CsvInserterService.Application/
COPY CsvInserterService.Domain/*.csproj ./CsvInserterService.Domain/
COPY CsvInserterService.Infrastructure/*.csproj ./CsvInserterService.Infrastructure/

# Restore NuGet packages
RUN dotnet restore

# Copy everything else
COPY . ./

# Build and publish
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Expose the port
EXPOSE 8080

# Run the app
ENTRYPOINT ["dotnet", "CsvInserterService.Api.dll"]
