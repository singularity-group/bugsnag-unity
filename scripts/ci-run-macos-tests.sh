#!/bin/bash
pushd features/fixtures/maze_runner/build
  rm -rf MacOS
  unzip MacOS-$UNITY_VERSION.zip
  # Ensure app is not marked as damaged
  xattr -cr MacOS/Mazerunner.app
popd
bundle install
sleep 3
bundle exec maze-runner --app=features/fixtures/maze_runner/build/MacOS/Mazerunner.app --os=macos features/desktop
