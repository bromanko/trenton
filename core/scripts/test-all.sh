#!/usr/bin/env bash

set -eo pipefail
SCRIPT_PATH="$( cd "$(dirname "$0")" ; pwd -P )"
cd "${SCRIPT_PATH}/.."

for proj in ./test/**/*.fsproj; do
    [[ ! $(grep "<OutputType>Exe</OutputType>" "${proj}") ]] && continue
    [[ "${proj}" =~ .*ITest.* ]] && continue

    echo
    echo "Running tests in ${proj}"
    dotnet run -p "${proj}"
    echo
done
