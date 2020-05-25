FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 443 80
ENV ASPNETCORE_ENVIRONMENT=Docker \
    ASPNETCORE_URLS=https://+:443 \ 
    ASPNETCORE_Kestrel__Certificates__Default__Password=1111 \
    ASPNETCORE_Kestrel__Certificates__Default__Path=/https/demoproxy.pfx

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["DemoItemsApi/DemoItemsApi.csproj", "DemoItemsApi/"]
RUN dotnet restore "DemoItemsApi/DemoItemsApi.csproj"
COPY . .
WORKDIR "/src/DemoItemsApi"
RUN dotnet build "DemoItemsApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DemoItemsApi.csproj" -c Release -o /app/publish

FROM base AS final
COPY demoproxy.pfx /https/
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DemoItemsApi.dll"]
