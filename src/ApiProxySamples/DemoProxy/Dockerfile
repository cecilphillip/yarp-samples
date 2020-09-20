FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80 443
ENV ASPNETCORE_ENVIRONMENT=Docker \
    ASPNETCORE_URLS=https://+:443 \     
    ASPNETCORE_Kestrel__Certificates__Default__Password=1111 \
    ASPNETCORE_Kestrel__Certificates__Default__Path=/https/demoproxy.pfx

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["DemoProxy/DemoProxy.csproj", "DemoProxy/"]
RUN dotnet restore "DemoProxy/DemoProxy.csproj"
COPY . .
WORKDIR "/src/DemoProxy"
RUN dotnet build "DemoProxy.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DemoProxy.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY demoproxy.pfx /https/
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DemoProxy.dll"]
