#!/bin/bash

gitversion=`cat gitversion.txt`
sed -i "s/BuildNumber = \"[^\"]*\"/BuildNumber = \"${gitversion}\"/g" Globals.cs