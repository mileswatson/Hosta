FROM mcr.microsoft.com/dotnet/sdk:5.0 AS base

WORKDIR /app
COPY Hosta/ ./Hosta
COPY HostaTests/ ./HostaTests

WORKDIR /app/HostaTests
RUN dotnet restore
CMD dotnet test