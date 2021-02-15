@echo off

powershell write-host -fore Yellow "This command should be run from the root Hosta directory. Ensure that you are in the correct directory before continuing."
pause
powershell write-host -fore Cyan "Running build test..."
cd src
docker buildx build -f Node/Dockerfile --platform linux/amd64,linux/arm64,linux/arm -t mileswatson/hosta:node .
cd ..
powershell write-host -fore Cyan "Running unit tests..."
docker-compose -f src/HostaTests/docker-compose.yml up
