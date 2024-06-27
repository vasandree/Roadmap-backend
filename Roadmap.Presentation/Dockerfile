FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Roadmap.Presentation/Roadmap.Presentation.csproj", "Roadmap.Presentation/"]
COPY ["Roadmap.Infrastructure/Roadmap.Infrastructure.csproj", "Roadmap.Infrastructure/"]
COPY ["Roadmap.Domain/Roadmap.Domain.csproj", "Roadmap.Domain/"]
COPY ["Roadmap.Persistence/Roadmap.Persistence.csproj", "Roadmap.Persistence/"]
COPY ["Roadmap.Application/Roadmap.Application.csproj", "Roadmap.Application/"]
COPY ["Common/Common.csproj", "Common/"]
RUN dotnet restore "Roadmap.Presentation/Roadmap.Presentation.csproj"
COPY . .
WORKDIR "/src/Roadmap.Presentation"
RUN dotnet build "Roadmap.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Roadmap.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Roadmap.Presentation.dll"]
