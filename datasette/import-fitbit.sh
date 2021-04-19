#!/usr/bin/env bash
set -euo pipefail

data_path="output/fitbit"
db_path="bromanko.db"

jq -s '[.[].weight | select(length > 0)] | flatten' "$data_path"/*.json |
	sqlite-utils insert "$db_path" fitbit-body-weight - --pk=logId --replace
