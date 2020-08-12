#!/usr/bin/env bash

set -euo pipefail

script_path="$( cd "$(dirname "$0")" ; pwd -P )"
cd "${script_path}"

./dev-entrypoint.sh \
  -wait "http://${TEST_SERVER_HOST}" \
  -- dotnet run --project test/Trenton.Webhooks.ITests/Trenton.Webhooks.ITests.fsproj
