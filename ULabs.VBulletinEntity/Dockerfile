FROM ul-vbentity-base AS sdk-image
ARG BAGET_API_KEY
ENV BAGET_API_KEY=${BAGET_API_KEY}
ARG BAGET_URL
ENV BAGET_URL=${BAGET_URL}

WORKDIR /app/ULabs.VBulletinEntity
COPY ULabs.VBulletinEntity.csproj .
RUN dotnet restore

COPY . .
RUN dotnet build

# Publish script is copied from the base image
RUN ["sh", "-c", "/app/publish.sh ${BAGET_API_KEY} ${BAGET_URL} /app/ULabs.VBulletinEntity ULabs.VBulletinEntity"]