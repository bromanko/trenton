#!/usr/bin/env bash

set -eo pipefail

script_path="$(
  cd "$(dirname "$0")"
  pwd -P
)"
cd "$script_path"/../

start_date=$1
: "${start_date:=$(date +%m-%d-%Y)}"
end_date=$2
: "${end_date:=$start_date}"

out_dir="./output/whoop"

trenton="dotnet run --project src/Trenton.Cli/Trenton.Cli.fsproj --"
whoop_creds="$HOME/.config/trenton/whoop.json"

access_token="$(jq -r .AccessToken <"$whoop_creds")"
refresh_token="$(jq -r .RefreshToken <"$whoop_creds")"

echo "Refreshing auth token..."
$trenton auth whoop refresh-token \
  --access_token "$access_token" \
  --refresh_token "$refresh_token" \
  >"$whoop_creds"

access_token="$(jq -r .AccessToken <"$whoop_creds")"
refresh_token="$(jq -r .RefreshToken <"$whoop_creds")"
user_id="$(jq -r .User.Id <"$whoop_creds")"

echo "Exporting data for period from $start_date to $end_date ..."
mkdir -p "$out_dir"

$trenton export whoop \
	--debug \
	--access_token "$access_token" \
	--refresh_token "$refresh_token" \
	--user_id "$user_id" \
	--start "$start_date" \
	--end "$end_date" \
	--out "$out_dir"

echo "Done!"
