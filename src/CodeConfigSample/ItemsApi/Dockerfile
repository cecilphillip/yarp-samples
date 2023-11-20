FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5010
ENV ASPNETCORE_ENVIRONMENT=Docker \
    ASPNETCORE_URLS=http://+:5010

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet build "ItemsApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ItemsApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ItemsApi.dll"]

USER app
