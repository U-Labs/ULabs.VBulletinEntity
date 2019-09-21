#!/bin/bash
BAGET_API_KEY=$1
version=$(cat ULabs.VBulletinEntity.csproj | grep "<Version>" | egrep -o "[0-9\.]+")
packageFile=$(ls bin/Debug | grep ${version})
echo "Publish ${packageFile}"
dotnet nuget push ./bin/Debug/${packageFile} -s http://baget/v3/index.json -k ${BAGET_API_KEY}