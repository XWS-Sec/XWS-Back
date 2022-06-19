ARG ASPNET_VERSION=5.0
ARG SDK_VERSION=5.0

FROM mcr.microsoft.com/dotnet/aspnet:${ASPNET_VERSION} AS base
WORKDIR /app
EXPOSE 7701

FROM mcr.microsoft.com/dotnet/sdk:${SDK_VERSION} AS build
WORKDIR /src
COPY Chats.Messages ./Chats.Messages
COPY Chats.Handlers ./Chats.Handlers
COPY Chats.Model ./Chats.Model
COPY Shared ./Shared
RUN dotnet restore "Chats.Handlers/Chats.Handlers.csproj" && \
    dotnet build "Chats.Handlers/Chats.Handlers.csproj" -c Release

FROM build AS publish
ENV PATH $PATH:/root/.dotnet/tools
RUN dotnet tool install -g dotnet-ef --version 5.0.11 && \
    dotnet publish "Chats.Handlers/Chats.Handlers.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
WORKDIR /app/publish
CMD ASPNETCORE_URLS=http://*:7701 dotnet Chats.Handlers.dll
ENTRYPOINT ["dotnet", "Chats.Handlers.dll"]