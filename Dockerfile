FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY NiquiBackend.csproj global.json ./
RUN dotnet restore NiquiBackend.csproj
COPY . .
RUN dotnet publish NiquiBackend.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "NiquiBackend.dll"]
