@echo off

cd ../Remote_Build_Server/MotherBuilder/bin/Debug
start MotherBuilder.exe 2

cd ../../../Repo/bin/Debug
start Repo.exe

cd ../../../TestHarness/bin/Debug
start TestHarness.exe

cd ../../../NavigatorClient/bin/Debug
start NavigatorClient.exe

cd ../../../

Pause