FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
FROM python:3.6.4
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY TocGenerator/TocGenerator.csproj TocGenerator/
RUN dotnet restore "TocGenerator/TocGenerator.csproj"
COPY . .
WORKDIR "/src/TocGenerator"
RUN dotnet build "TocGenerator.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TocGenerator.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TocGenerator.dll"]
