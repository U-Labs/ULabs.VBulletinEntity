node {
	checkout scm
}

pipeline {
    agent { 
		label 'ul-host'
	}
	// https://stackoverflow.com/a/48805385/3276634
	options { 
		disableConcurrentBuilds() 
	}

	// https://stackoverflow.com/a/48150841/3276634
    triggers {
		pollSCM('')
	}

    stages {
		stage('Build base image with shared project') {
			steps {
				sh 'docker build -t ul-vbentity-base --build-arg BAGET_API_KEY=$BAGET_API_KEY --build-arg BAGET_URL=$BAGET_URL .'
			}
		}
		stage('Build and push NuGet package') {
			steps {
				sh 'docker-compose up --build'
			}
		}
    }
}