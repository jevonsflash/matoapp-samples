#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/MatoAppSample.Migrator/MatoAppSample.Migrator.csproj", "src/MatoAppSample.Migrator/"]
COPY ["src/MatoAppSample.Core/MatoAppSample.Core.csproj", "src/MatoAppSample.Core/"]
COPY ["src/MatoAppSample.EntityFrameworkCore/MatoAppSample.EntityFrameworkCore.csproj", "src/MatoAppSample.EntityFrameworkCore/"]
RUN dotnet restore "src/MatoAppSample.Migrator/MatoAppSample.Migrator.csproj"
COPY . .
WORKDIR "/src/src/MatoAppSample.Migrator"
RUN dotnet build "MatoAppSample.Migrator.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MatoAppSample.Migrator.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MatoAppSample.Migrator.dll","-q"]