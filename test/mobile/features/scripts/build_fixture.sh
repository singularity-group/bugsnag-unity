#!/usr/bin/env bash

pushd "${0%/*}"
  script_path=`pwd`
popd

echo "Script in: $script_path"

pushd "$script_path/../fixtures"

# Run unity and immediately exit afterwards, log all output, disable the
# package manager (we just don't need it and it slows things down)
DEFAULT_CLI_ARGS="-quit -batchmode -logFile unity.log -noUpm"

# Creating a new project in the MyProject directory
Unity $DEFAULT_CLI_ARGS -createProject `pwd`/maze_runner

# Installing the Bugsnag package
Unity $DEFAULT_CLI_ARGS -importPackage $script_path/../../../../Bugsnag.unitypackage
Unity $DEFAULT_CLI_ARGS -importPackage $script_path/../../../../Bugsnag-with-android-64bit.unitypackage

cp -r Assets/Scenes maze_runner/Assets/
cp -r Assets/Scripts maze_runner/Assets/
cp Assets/Editor/Builder.cs maze_runner/Assets/Scripts/
cp Assets/Plugins/Android/AndroidManifest.xml maze_runner/Assets/Plugins/Android

# Running a custom script - must reference a static method
Unity $DEFAULT_CLI_ARGS -executeMethod Builder.AndroidBuild -projectPath `pwd`/maze_runner

## Open unity.log to see the Hello World
open unity.log
