#!/bin/bash

# Using the artifacts plugin v1.3 on Windows, even under WSL, seems to break the whole step
buildkite-agent artifact download "Bugsnag.unitypackage" .

# Build the Windows fixture (assumes agent is running under WSL)
./features/scripts/build_maze_runner.sh windows
CODE=$?
if [[ $CODE != 0 ]]; then
  echo "Error, exit code: $CODE"
  exit $CODE
fi

pushd features/fixtures/maze_runner/build
  zip -r Windows-$UNITY_VERSION.zip Windows
popd
