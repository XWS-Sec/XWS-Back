ARG ASPNET_VERSION=5.0
ARG SDK_VERSION=5.0

FROM mcr.microsoft.com/dotnet/aspnet:${ASPNET_VERSION} AS base
WORKDIR /app
EXPOSE 7704

FROM mcr.microsoft.com/dotnet/sdk:${SDK_VERSION} AS build
WORKDIR /src
COPY Users.Graph.Messages ./Users.Graph.Messages
COPY Users.Graph.Handlers ./Users.Graph.Handlers
COPY Users.Graph.Model ./Users.Graph.Model
COPY Shared ./Shared
RUN dotnet restore "Users.Graph.Handlers/Users.Graph.Handlers.csproj" && \
    dotnet build "Users.Graph.Handlers/Users.Graph.Handlers.csproj" -c Release

FROM build AS publish
ENV PATH $PATH:/root/.dotnet/tools
RUN dotnet tool install -g dotnet-ef --version 5.0.11 && \
    dotnet publish "Users.Graph.Handlers/Users.Graph.Handlers.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
WORKDIR /app/publish
CMD ASPNETCORE_URLS=http://*:7704 dotnet Users.Graph.Handlers.dll
ENTRYPOINT ["dotnet", "Users.Graph.Handlers.dll"]