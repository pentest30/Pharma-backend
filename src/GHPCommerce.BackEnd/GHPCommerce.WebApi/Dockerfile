#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["GHPCommerce.BackEnd/GHPCommerce.WebApi/GHPCommerce.WebApi.csproj", "GHPCommerce.BackEnd/GHPCommerce.WebApi/"]
COPY ["GHPCommerce.Infrastructure/GHPCommerce.Infra/GHPCommerce.Infra.csproj", "GHPCommerce.Infrastructure/GHPCommerce.Infra/"]
COPY ["GHPCommerce.CrossCuttingConcerns/GHPCommerce.CrossCuttingConcerns/GHPCommerce.CrossCuttingConcerns.csproj", "GHPCommerce.CrossCuttingConcerns/GHPCommerce.CrossCuttingConcerns/"]
COPY ["GHPCommerce.Infrastructure/GHPCommerce.Persistence/GHPCommerce.Persistence.csproj", "GHPCommerce.Infrastructure/GHPCommerce.Persistence/"]
COPY ["GHPCommerce.Core/GHPCommerce.Domain/GHPCommerce.Domain.csproj", "GHPCommerce.Core/GHPCommerce.Domain/"]
COPY ["GHPCommerce.Core/GHPCommerce.Application/GHPCommerce.Application.csproj", "GHPCommerce.Core/GHPCommerce.Application/"]
RUN dotnet restore "GHPCommerce.BackEnd/GHPCommerce.WebApi/GHPCommerce.WebApi.csproj"
COPY . .
WORKDIR "/src/GHPCommerce.BackEnd/GHPCommerce.WebApi"
RUN dotnet build "GHPCommerce.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GHPCommerce.WebApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GHPCommerce.WebApi.dll"]