#!/usr/bin/env bash

set -euo pipefail

script_path="$( cd "$(dirname "$0")" ; pwd -P )"
cd "${script_path}/../"

function installDependencies {
    updated_file="/root/.nuget/packages/.updated"
    if [[ paket.dependencies -nt "${updated_file}" ]]; then
        dotnet tool restore
        dotnet paket install

        date > "${updated_file}"
    fi
}

installDependencies

dockerize \
  "$@"
