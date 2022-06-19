ARG ASPNET_VERSION=5.0
ARG SDK_VERSION=5.0

FROM mcr.microsoft.com/dotnet/aspnet:${ASPNET_VERSION} AS base
WORKDIR /app
EXPOSE 7703

FROM mcr.microsoft.com/dotnet/sdk:${SDK_VERSION} AS build
WORKDIR /src
COPY Posts.Messages ./Posts.Messages
COPY Posts.Handlers ./Posts.Handlers
COPY Posts.Model ./Posts.Model
COPY Shared ./Shared
RUN dotnet restore "Posts.Handlers/Posts.Handlers.csproj" && \
    dotnet build "Posts.Handlers/Posts.Handlers.csproj" -c Release

FROM build AS publish
ENV PATH $PATH:/root/.dotnet/tools
RUN dotnet tool install -g dotnet-ef --version 5.0.11 && \
    dotnet publish "Posts.Handlers/Posts.Handlers.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
WORKDIR /app/publish
CMD ASPNETCORE_URLS=http://*:7703 dotnet Posts.Handlers.dll
ENTRYPOINT ["dotnet", "Posts.Handlers.dll"]