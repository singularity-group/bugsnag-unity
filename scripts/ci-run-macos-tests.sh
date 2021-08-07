#!/bin/bash
pushd features/fixtures/maze_runner/build
  rm -rf MacOS
  sha1sum MacOS-$UNITY_VERSION.zip
  unzip MacOS-$UNITY_VERSION.zip
  find ./MacOS -type f -print0 | xargs -0 sha1sum
popd
open -W features/fixtures/maze_runner/build/MacOS/Mazerunner.app --args -batchmode -nographics
