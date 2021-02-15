@echo off

powershell write-host -fore Yellow "This command should be run from the root Hosta directory. Ensure that you are in the correct directory before continuing."
pause

powershell write-host -fore Yellow "Are you sure you want to set the version to" v%1"?"
pause

powershell write-host -fore Cyan "Building and pushing..."
cd src
docker buildx build -f Node/Dockerfile --platform linux/amd64,linux/arm64,linux/arm -t mileswatson/hosta:node mileswatson/hosta:node-v%1 . --push