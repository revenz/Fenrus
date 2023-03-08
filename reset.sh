#!/bin/bash

# Resets the initial configuration
echo This will delete all the system settings.  
echo Users and their settings will NOT be effected
echo # empty line 
read -p "Are you sure you want to continue? [y/n]" -n 1 -r
echo    # move to a new line
if [[ $REPLY =~ ^[Yy]$ ]]
then
  dotnet Fenrus.dll --init-config 
  kill 1 
fi