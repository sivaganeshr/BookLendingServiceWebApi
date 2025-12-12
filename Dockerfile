# Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ["BookLendingService.Api/BookLendingService.Api.csproj", "BookLendingService.Api/"]
RUN dotnet restore "BookLendingService.Api/BookLendingService.Api.csproj"

# Copy the entire solution
COPY . .

# Publish
WORKDIR "/src/BookLendingService.Api"
RUN dotnet publish -c Release -o /app/publish

# Final image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "BookLendingService.Api.dll"]