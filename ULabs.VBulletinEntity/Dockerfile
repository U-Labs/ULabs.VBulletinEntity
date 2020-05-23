FROM mcr.microsoft.com/dotnet/core/sdk:2.1 AS sdk-image
ARG BAGET_API_KEY
ENV BAGET_API_KEY=${BAGET_API_KEY}
ARG BAGET_URL
ENV BAGET_URL=${BAGET_URL}

WORKDIR /app/ULabs.VBulletinEntity
COPY ULabs.VBulletinEntity.csproj .
RUN dotnet restore

COPY . .
RUN chmod +x publish.sh \
	&& dotnet build

ENTRYPOINT ["sh", "-c", "/app/ULabs.VBulletinEntity/publish.sh ${BAGET_API_KEY} ${BAGET_URL} /app/ULabs.VBulletinEntity.Shared ULabs.VBulletinEntity.Shared"]