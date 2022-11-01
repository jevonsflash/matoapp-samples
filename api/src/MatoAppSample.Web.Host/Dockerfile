#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 44311

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/MatoAppSample.Web.Host/MatoAppSample.Web.Host.csproj", "src/MatoAppSample.Web.Host/"]
COPY ["src/MatoAppSample.Web.Core/MatoAppSample.Web.Core.csproj", "src/MatoAppSample.Web.Core/"]
COPY ["src/MatoAppSample.Application/MatoAppSample.Application.csproj", "src/MatoAppSample.Application/"]
COPY ["src/MatoAppSample.Core/MatoAppSample.Core.csproj", "src/MatoAppSample.Core/"]
COPY ["src/MatoAppSample.EntityFrameworkCore/MatoAppSample.EntityFrameworkCore.csproj", "src/MatoAppSample.EntityFrameworkCore/"]
RUN dotnet restore "src/MatoAppSample.Web.Host/MatoAppSample.Web.Host.csproj"
COPY . .
WORKDIR "/src/src/MatoAppSample.Web.Host"
RUN dotnet build "MatoAppSample.Web.Host.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MatoAppSample.Web.Host.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MatoAppSample.Web.Host.dll"]

