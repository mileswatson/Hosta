@echo off

powershell write-host -fore Yellow "This command should be run from the root Hosta directory. Ensure that you are in the correct directory before continuing."
pause
powershell write-host -fore Cyan "Testing..."
docker-compose -f src/HostaTests/docker-compose.yml up