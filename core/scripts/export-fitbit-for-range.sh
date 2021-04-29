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

out_dir="./output/fitbit"

trenton="dotnet run --project src/Trenton.Cli/Trenton.Cli.fsproj --"
config="$HOME/.config/trenton/config.json"
fitbit_creds="$HOME/.config/trenton/fitbit.json"

client_id="$(jq -r .Fitbit.ClientId <"$config")"
client_secret="$(jq -r .Fitbit.ClientSecret <"$config")"
access_token="$(jq -r .AccessToken <"$fitbit_creds")"
refresh_token="$(jq -r .RefreshToken <"$fitbit_creds")"

echo "Refreshing auth token..."
$trenton auth fitbit refresh-token \
	--client_id "$client_id" \
	--client_secret "$client_secret" \
	--access_token "$access_token" \
	--refresh_token "$refresh_token" \
	>"$fitbit_creds"

access_token="$(jq -r .AccessToken <"$fitbit_creds")"
refresh_token="$(jq -r .RefreshToken <"$fitbit_creds")"

echo "Exporting data for period from $start_date to $end_date ..."
mkdir -p "$out_dir"

$trenton export fitbit \
	--debug \
	--client_id "$client_id" \
	--client_secret "$client_secret" \
	--access_token "$access_token" \
	--refresh_token "$refresh_token" \
	--start "$start_date" \
	--end "$end_date" \
	--out "$out_dir"

echo "Done!"
