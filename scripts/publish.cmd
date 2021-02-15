@echo off

powershell write-host -fore Yellow "This command should be run from the root Hosta directory. Ensure that you are in the correct directory before continuing."
pause
powershell write-host -fore Cyan "Zipping node..."
powershell Compress-Archive -Path run/HostaNode -DestinationPath publish/node.zip -Force
powershell write-host -fore Cyan "Zipping client..."
powershell Compress-Archive -Path src/ClientWPF/bin/HostaClient -DestinationPath publish/client.zip -Force