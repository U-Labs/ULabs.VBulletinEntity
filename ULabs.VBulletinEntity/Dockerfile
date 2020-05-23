FROM ul-vbentity-base AS sdk-image
ARG BAGET_API_KEY
ENV BAGET_API_KEY=${BAGET_API_KEY}

# Since we copy the entire project to a subfolder for matching the shared project, we need to seperately copy the entrypoint script
WORKDIR /app/
COPY docker-entrypoint.sh .
RUN chmod +x docker-entrypoint.sh

WORKDIR /app/ULabs.VBulletinEntity
COPY ULabs.VBulletinEntity.csproj .
RUN dotnet restore

COPY . .
RUN dotnet build

ENTRYPOINT ["sh", "-c", "/app/docker-entrypoint.sh ${BAGET_API_KEY}"]