FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY RealTimeSystem.slnx ./
COPY src/RealTimeSystem.Domain/RealTimeSystem.Domain.csproj src/RealTimeSystem.Domain/
COPY src/RealTimeSystem.Application/RealTimeSystem.Application.csproj src/RealTimeSystem.Application/
COPY src/RealTimeSystem.Infrastructure/RealTimeSystem.Infrastructure.csproj src/RealTimeSystem.Infrastructure/
COPY src/RealTimeSystem.WebAPI/RealTimeSystem.WebAPI.csproj src/RealTimeSystem.WebAPI/
RUN dotnet restore src/RealTimeSystem.WebAPI/RealTimeSystem.WebAPI.csproj
COPY src/ src/
RUN dotnet publish src/RealTimeSystem.WebAPI/RealTimeSystem.WebAPI.csproj -c Release -o /app /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
ENTRYPOINT ["dotnet", "RealTimeSystem.WebAPI.dll"]
