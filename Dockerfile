FROM mcr.microsoft.com/dotnet/core/sdk:2.1 AS sdk-image
ARG BAGET_API_KEY
ENV BAGET_API_KEY=${BAGET_API_KEY}
ARG BAGET_URL
ENV BAGET_URL=${BAGET_URL}

WORKDIR /app/ULabs.VBulletinEntity.Shared
COPY ULabs.VBulletinEntity.Shared/ULabs.VBulletinEntity.Shared.csproj .
RUN dotnet restore
COPY ULabs.VBulletinEntity.Shared .

WORKDIR /app/ULabs.VBulletinEntity
COPY ULabs.VBulletinEntity/ULabs.VBulletinEntity.csproj .
RUN dotnet restore
COPY ULabs.VBulletinEntity .
RUN dotnet build ULabs.VBulletinEntity.csproj

WORKDIR /app/ULabs.LightVBulletinEntity
COPY ULabs.LightVBulletinEntity/ULabs.LightVBulletinEntity.csproj .
RUN dotnet restore
COPY ULabs.LightVBulletinEntity .
RUN dotnet build

# Publishing happens in the entrypoint since we need network connection to BaGet
COPY publish.sh /app
ENTRYPOINT /app/publish.sh ${BAGET_API_KEY} 