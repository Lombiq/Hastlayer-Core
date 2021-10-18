FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY . .
RUN rm Hastlayer.SDK.Client.sln Hastlayer.SDK.sln
RUN dotnet restore
RUN dotnet build Hast.Core/Hast.Remote.Worker.Daemon/Hast.Remote.Worker.Daemon.csproj -c Release -o /app/build

FROM build AS publish
RUN dotnet publish Hast.Core/Hast.Remote.Worker.Daemon/Hast.Remote.Worker.Daemon.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Hast.Remote.Worker.Daemon.dll"]
