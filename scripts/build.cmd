@echo off

powershell write-host -fore Yellow "This command should be run from the root Hosta directory."
powershell write-host -fore Yellow "Ensure that you are in the correct directory before continuing."
powershell write-host -fore Yellow "In addition, the command relies on the docker buildx command."
powershell write-host -fore Yellow "Ensure that you have followed the instructions in the README for setting up buildx."
pause
powershell write-host -fore Cyan "Building node..."
cd src
docker build -f Node/Dockerfile -t mileswatson/hosta:node-dev .
cd ..
powershell write-host -fore Cyan "Building client..."
dotnet publish src/ClientWPF/ClientWPF.csproj ^
    -r win-x64 ^
    -c Release ^
    -o src/ClientWPF/bin/HostaClient ^
    -p:DebugType=None ^
    -p:PublishSingleFile=true ^
    -p:IncludeNativeLibrariesForSelfExtract=true