FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80 443
ENV ASPNETCORE_ENVIRONMENT=Docker \
    ASPNETCORE_URLS=http://+:80 

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["DemoAddressApi/DemoAddressApi.csproj", "DemoAddressApi/"]
RUN dotnet restore "DemoAddressApi/DemoAddressApi.csproj"
COPY . .
WORKDIR "/src/DemoAddressApi"
RUN dotnet build "DemoAddressApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DemoAddressApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DemoAddressApi.dll"]
