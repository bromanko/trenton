#!/usr/bin/env bash

set -euo pipefail

script_path="$( cd "$(dirname "$0")" ; pwd -P )"
cd "${script_path}"

project=${1?Must provide the itest project name}
project="$project/$project.fsproj"
[[ -f "../test/$project" ]] || (echo "Could not find project $1" && exit 1)

./dev-entrypoint.sh \
  -wait "http://${TEST_SERVER_HOST}" \
  -- dotnet run --project "$project"
