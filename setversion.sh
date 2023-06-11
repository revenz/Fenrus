#!/bin/bash

get_version() {
  local date=$(date -u -d "+12 hours" +"%Y-%m-%dT%H:%M:%S%z") # NZST date and time
  local week_number=$(get_week_number "$date")
  local version=$(date -u -d "$date" +"%y.%m.")$week_number
  echo "$version"
}
get_week_number() {
    local date="$1"
    local first_day_of_month=$(date -d "$date" +%Y-%m-01)
    local first_day_of_week="Monday"

    # Find the first day of the week that falls on or before the first day of the month
    while [[ $(date -d "$first_day_of_month" +%A) != "$first_day_of_week" ]]; do
        first_day_of_month=$(date -d "$first_day_of_month - 1 day" +%Y-%m-%d)
    done

    local week_number=$((($(date -d "$date" +%s) - $(date -d "$first_day_of_month" +%s)) / (60 * 60 * 24 * 7) + 1))
    echo "$week_number"
}

versionNumber=$(get_version)
gitversion=`cat gitversion.txt`

sed -i "s/BuildNumber = \"[^\"]*\"/BuildNumber = \"${gitversion}\"/g" Globals.cs
sed -i "s/VersionNumber = \"[^\"]*\"/VersionNumber = \"${versionNumber}\"/g" Globals.cs