#!/bin/bash
pushd features/fixtures/maze_runner/build
  rm -rf MacOS
  sha1sum MacOS-$UNITY_VERSION.zip
  unzip MacOS-$UNITY_VERSION.zip
  ls -lR
popd
bundle install
sleep 3
bundle exec maze-runner --app=features/fixtures/maze_runner/build/MacOS/Mazerunner.app --os=macos features/desktop
