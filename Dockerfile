FROM mcr.microsoft.com/dotnet/core/sdk:2.1 AS sdk-image

WORKDIR /app/ULabs.VBulletinEntity.Shared
COPY ULabs.VBulletinEntity.Shared/ULabs.VBulletinEntity.Shared.csproj .
RUN dotnet restore
COPY ULabs.VBulletinEntity.Shared .
RUN dotnet build