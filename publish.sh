#!/bin/bash
BAGET_API_KEY=$1
URL=$2

function publish() {
        PROJECT_PATH=$1
        PROJECT=$2
        version=$(cat $PROJECT_PATH/$PROJECT.csproj | grep "<Version>" | egrep -o "[0-9\.]+")
        packageFile=$(ls ${PROJECT_PATH}/bin/Debug | grep ${version})
        echo "Try to publish ${packageFile}"

        if ! curl -s --fail "https://baget.dev.u-labs.de/v3/registration/${PROJECT}/${version}.json" > /dev/null
        then
                echo "Not published yet: Start publishing!"
                dotnet nuget push ${PROJECT_PATH}/bin/Debug/${packageFile} -s $URL -k ${BAGET_API_KEY}
                publishRc=$?
                echo "Publish rc = ${publishRc}"

                if [ $publishRc -ne 0 ]; then
                        echo "Nuget publish error: exit with $publishRc"
                        exit $publishRc
                fi
        else
                echo "WARNING: Version ${version} was already published"
        fi
}

publish "/app/ULabs.VBulletinEntity.Shared" "ULabs.VBulletinEntity.Shared"
publish "/app/ULabs.LightVBulletinEntity" "ULabs.LightVBulletinEntity"
publish "/app/ULabs.VBulletinEntity" "ULabs.VBulletinEntity"