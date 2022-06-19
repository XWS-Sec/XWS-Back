ARG ASPNET_VERSION=5.0
ARG SDK_VERSION=5.0

FROM mcr.microsoft.com/dotnet/aspnet:${ASPNET_VERSION} AS base
WORKDIR /app
EXPOSE 7700

FROM mcr.microsoft.com/dotnet/sdk:${SDK_VERSION} AS build
WORKDIR /src
COPY BaseApi ./BaseApi
COPY BaseApi.Messages ./BaseApi.Messages
COPY BaseApi.Model ./BaseApi.Model
COPY BaseApi.Services ./BaseApi.Services
COPY Chats.Messages ./Chats.Messages
COPY JobOffers.Messages ./JobOffers.Messages
COPY Posts.Messages ./Posts.Messages
COPY Users.Graph.Messages ./Users.Graph.Messages
COPY Shared ./Shared
RUN dotnet restore "BaseApi/BaseApi.csproj" && \
    dotnet build "BaseApi/BaseApi.csproj" -c Release

FROM build AS publish
ENV PATH $PATH:/root/.dotnet/tools
RUN dotnet tool install -g dotnet-ef --version 5.0.11 && \
    dotnet publish "BaseApi/BaseApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
WORKDIR /app/publish
CMD ASPNETCORE_URLS=http://*:7700 dotnet BaseApi.dll
ENTRYPOINT ["dotnet", "BaseApi.dll"]