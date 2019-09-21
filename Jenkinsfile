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
		stage('Build and push NuGet package') {
			steps {
				sh 'docker-compose up --build'
			}
		}
    }
}