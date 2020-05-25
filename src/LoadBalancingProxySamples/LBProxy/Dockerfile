FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80
ENV ASPNETCORE_ENVIRONMENT=Docker \
    ASPNETCORE_URLS=http://+:80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["LBProxy/LBProxy.csproj", "LBProxy/"]
RUN dotnet restore "LBProxy/LBProxy.csproj"
COPY . .
WORKDIR "/src/LBProxy"
RUN dotnet build "LBProxy.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LBProxy.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LBProxy.dll"]
