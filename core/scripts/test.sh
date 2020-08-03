#!/usr/bin/env bash

set -eo pipefail
SCRIPT_PATH="$( cd "$(dirname "$0")" ; pwd -P )"
cd "${SCRIPT_PATH}/.."

if [[ ${1} == "--watch" ]]; then
    watch_flag=$1
    shift
fi

project=${1:?You must specify a project name}

project="test/${project}/${project}.fsproj"
[[ -f "${project}" ]] || (echo "The specified project ${project} does not exist."; exit 1)

if [[ -z ${watch_flag+x} ]]; then
    dotnet run -p "${project}"
else
    dotnet watch -p "${project}" run
fi
