#!/bin/bash

get_version() {
  local date=$(date -u -d "+12 hours" +"%Y-%m-%dT%H:%M:%S%z") # NZST date and time
  local week_number=$(get_week_number "$date")
  local version=$(date -u -d "$date" +"%y.%m.")$week_number
  echo "$version"
}

get_week_number() {
  local date=$1
  local first_day_of_month=$(date -u -d "$(date -u -d "$date" +"%Y-%m-01T00:00:00%z")" +"%Y-%m-%dT%H:%M:%S%z") # First day of the month
  local days_until_first_monday=$(( ( $(date -u -d "$first_day_of_month" +"%u") + 7 - 1 ) % 7 )) # Days until the first Monday of the month
  local first_monday_of_month=$(date -u -d "$first_day_of_month +$days_until_first_monday days" +"%Y-%m-%dT%H:%M:%S%z") # First Monday of the month

  if [[ $date < $first_monday_of_month ]]; then
    echo 1
  else
    local week_number=$(( ( $(date -u -d "$date" +"%d") - $(date -u -d "$first_monday_of_month" +"%d") ) / 7 + 1 ))
    local next_monday=$(date -u -d "$first_monday_of_month +$((week_number * 7)) days" +"%Y-%m-%dT%H:%M:%S%z")
    
    if [[ $next_monday > $date ]]; then
      echo $week_number
    else
      echo $((week_number + 1))
    fi
  fi
}

versionNumber=$(get_version)
gitversion=`cat gitversion.txt`

sed -i "s/BuildNumber = \"[^\"]*\"/BuildNumber = \"${gitversion}\"/g" Globals.cs
sed -i "s/VersionNumber = \"[^\"]*\"/VersionNumber = \"${versionNumber}\"/g" Globals.cs