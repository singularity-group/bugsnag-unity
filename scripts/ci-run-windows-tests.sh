#!/bin/bash

# Using the artifacts plugin v1.3 on Windows, even under WSL, seems to break the whole step
buildkite-agent artifact download "features/fixtures/maze_runner/build/Windows-$UNITY_VERSION.zip" .

pushd features/fixtures/maze_runner/build
  unzip Windows-$UNITY_VERSION.zip
popd

bundle install
bundle exec maze-runner --app=features/fixtures/maze_runner/build/Windows/Mazerunner.exe --os=windows
