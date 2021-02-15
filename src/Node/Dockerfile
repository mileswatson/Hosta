FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim-amd64 AS build

WORKDIR /app
COPY Hosta/ ./Hosta
COPY Node/ ./Node

WORKDIR /app/Node
RUN dotnet restore
RUN dotnet publish -c Release -o /out --no-restore

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base

ENV PORT 12000

WORKDIR /app
COPY --from=build out/ ./out
RUN mkdir data

ENTRYPOINT [ "sh", "-c", "dotnet out/Node.dll /app/data $PORT" ]