FROM mcr.microsoft.com/dotnet/core/sdk:2.1 AS sdk-image
ARG BAGET_API_KEY
ENV BAGET_API_KEY=${BAGET_API_KEY}
ARG BAGET_URL
ENV BAGET_URL=${BAGET_URL}

WORKDIR /app
COPY publish.sh .
RUN chmod +x publish.sh

WORKDIR /app/ULabs.VBulletinEntity.Shared
COPY ULabs.VBulletinEntity.Shared/ULabs.VBulletinEntity.Shared.csproj .
RUN dotnet restore
COPY ULabs.VBulletinEntity.Shared .
RUN dotnet build

RUN ["sh", "-c", "/app/docker-entrypoint.sh ${BAGET_API_KEY} ${BAGET_URL} /app/ULabs.VBulletinEntity.Shared ULabs.VBulletinEntity.Shared"]