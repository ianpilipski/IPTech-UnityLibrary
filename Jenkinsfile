pipeline {
    agent any
    options {
        skipDefaultCheckout true
    }
    stages {
        stage('Checkout') {
            steps {
                checkout scm
                script {
                    def PROJECT_VERSION = sh(returnStdout: true, script: "sed -nE 's/bundleVersion: (.+)/\\1/p' unity/iptech-unitylibrary/ProjectSettings/ProjectSettings.asset").trim()
                    currentBuild.description = "Version: ${PROJECT_VERSION}"
                    def UNITY_EDITOR_VERSION = sh(returnStdout: true, script: "sed -nE 's/m_EditorVersion: (.+)/\\1/p' unity/iptech-unitylibrary/ProjectSettings/ProjectVersion.txt").trim()
                    env.UNITY_EDITOR_VERSION = UNITY_EDITOR_VERSION
                }
            }
        }
        stage('Clean') {
            steps {
                sh "./gradlew clean --refresh-dependencies"
            }
        }
        stage('Build') {
            steps {
                sh "./gradlew buildDebugAndroid -PBUILD_NUMBER=${BUILD_NUMBER} -PUNITY_EDITOR_VERSION=${env.UNITY_EDITOR_VERSION}"
                sh "./gradlew buildDebugiOS -PBUILD_NUMBER=${BUILD_NUMBER} -PUNITY_EDITOR_VERSION=${env.UNITY_EDITOR_VERSION}"
                sh "./gradlew -b build-unityservices.gradle buildDebugAndroid -PBUILD_NUMBER=${BUILD_NUMBER} -PUNITY_EDITOR_VERSION=${env.UNITY_EDITOR_VERSION}"
                sh "./gradlew -b build-unityservices.gradle buildDebugiOS -PBUILD_NUMBER=${BUILD_NUMBER} -PUNITY_EDITOR_VERSION=${env.UNITY_EDITOR_VERSION}"
            }
        }
    }
    post {
        always {
            junit allowEmptyResults: true, skipPublishingChecks: true, testResults: 'build/unity/DebugAndroid/step_002_DebugAndroid_runEditModeTests/editmode-testresults.xml'
            junit allowEmptyResults: true, skipPublishingChecks: true, testResults: 'build/unity/DebugiOS/step_002_DebugiOS_runEditModeTests/editmode-testresults.xml'
            junit allowEmptyResults: true, skipPublishingChecks: true, testResults: 'build/unity-services/DebugAndroid/step_002_DebugAndroid_runEditModeTests/editmode-testresults.xml'
            junit allowEmptyResults: true, skipPublishingChecks: true, testResults: 'build/unity-services/DebugiOS/step_002_DebugiOS_runEditModeTests/editmode-testresults.xml'
        }
    }
}
