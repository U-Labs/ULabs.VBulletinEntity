FROM ul-vbentity-base AS sdk-image
WORKDIR /app
ARG BAGET_API_KEY
ENV BAGET_API_KEY=${BAGET_API_KEY}

COPY ULabs.VBulletinEntity.csproj .
RUN dotnet restore

COPY . .
RUN dotnet build

ENTRYPOINT ["sh", "-c", "/app/docker-entrypoint.sh ${BAGET_API_KEY}"]