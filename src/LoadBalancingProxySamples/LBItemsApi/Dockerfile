FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
ENV ASPNETCORE_ENVIRONMENT=Docker \
    ASPNETCORE_URLS=http://+:80     

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["LBItemsApi/LBItemsApi.csproj", "LBItemsApi/"]
RUN dotnet restore "LBItemsApi/LBItemsApi.csproj"
COPY . .
WORKDIR "/src/LBItemsApi"
RUN dotnet build "LBItemsApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LBItemsApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LBItemsApi.dll"]