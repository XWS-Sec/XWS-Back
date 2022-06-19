ARG ASPNET_VERSION=5.0
ARG SDK_VERSION=5.0

FROM mcr.microsoft.com/dotnet/aspnet:${ASPNET_VERSION} AS base
WORKDIR /app
EXPOSE 7702

FROM mcr.microsoft.com/dotnet/sdk:${SDK_VERSION} AS build
WORKDIR /src
COPY JobOffers.Messages ./JobOffers.Messages
COPY JobOffers.Handlers ./JobOffers.Handlers
COPY JobOffers.Model ./JobOffers.Model
COPY Shared ./Shared
RUN dotnet restore "JobOffers.Handlers/JobOffers.Handlers.csproj" && \
    dotnet build "JobOffers.Handlers/JobOffers.Handlers.csproj" -c Release

FROM build AS publish
ENV PATH $PATH:/root/.dotnet/tools
RUN dotnet tool install -g dotnet-ef --version 5.0.11 && \
    dotnet publish "JobOffers.Handlers/JobOffers.Handlers.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
WORKDIR /app/publish
CMD ASPNETCORE_URLS=http://*:7702 dotnet JobOffers.Handlers.dll
ENTRYPOINT ["dotnet", "JobOffers.Handlers.dll"]