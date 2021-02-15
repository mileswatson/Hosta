@echo off

powershell write-host -fore Yellow "This command should be run from the root Hosta directory. Ensure that you are in the correct directory before continuing."
pause
powershell write-host -fore Cyan "Building node..."
cd src
docker buildx build -f Node/Dockerfile --platform linux/amd64,linux/arm64,linux/arm -t mileswatson/hosta:node .
cd ..
powershell write-host -fore Cyan "Building client..."
dotnet publish src/ClientWPF/ClientWPF.csproj ^
    -r win-x64 ^
    -c Release ^
    -o src/ClientWPF/bin/HostaClient ^
    -p:DebugType=None ^
    -p:PublishSingleFile=true ^
    -p:IncludeAllContentForSelfExtract=true