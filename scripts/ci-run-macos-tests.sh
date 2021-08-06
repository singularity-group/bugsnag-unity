#!/bin/bash
pushd features/fixtures/maze_runner/build
  unzip MacOS-$UNITY_VERSION.zip
  # Ensure app is not marked as damaged
  xattr -cr MacOS/Mazerunner.app
popd
sleep 1
bundle install
bundle exec maze-runner --app=features/fixtures/maze_runner/build/MacOS/Mazerunner.app --os=macos features/desktop
