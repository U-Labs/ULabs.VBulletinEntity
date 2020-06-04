# Build ARGS are provided by the base image
FROM ul-vbentity-base AS sdk-image

WORKDIR /app/ULabs.VBulletinEntity
COPY ULabs.VBulletinEntity.csproj .
RUN dotnet restore

COPY . .
RUN dotnet build

# Publish script is copied from the base image
RUN ["sh", "-x", "-c", "/app/publish.sh ${BAGET_API_KEY} ${BAGET_URL} /app/ULabs.VBulletinEntity ULabs.VBulletinEntity"]