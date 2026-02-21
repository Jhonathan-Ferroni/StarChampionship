# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY *.sln .
COPY StarChampionship/*.csproj ./StarChampionship/
RUN dotnet restore

COPY . .
WORKDIR /app/StarChampionship
RUN dotnet publish -c Release -o out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/StarChampionship/out .

ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "StarChampionship.dll"]