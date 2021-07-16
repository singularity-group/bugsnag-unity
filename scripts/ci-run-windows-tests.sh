#!/bin/bash
pushd features/fixtures/maze_runner/build
  unzip Windows-$UNITY_VERSION.zip
popd

bundle install
bundle exec maze-runner --app=features/fixtures/maze_runner/build/Windows/Mazerunner.exe --os=windows
