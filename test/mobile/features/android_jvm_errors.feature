Feature: Android manual smoke tests

    Background:
        Given I wait for the game to start

    Scenario: Uncaught JVM exception
        When I tap the "Native exception" button
        Then I wait to receive 1 error

        # Exception details
        And the error payload field "events" is an array with 1 elements
        And the exception "errorClass" equals "java.lang.ArrayIndexOutOfBoundsException"
        And the exception "message" equals "length=2; index=2"
        And the event "unhandled" is true
        And the event "severity" equals "error"
        And the event "severityReason.type" equals "unhandledException"

        # Stacktrace validation
        And the error payload field "events.0.exceptions.0.stacktrace" is a non-empty array
        And the event "exceptions.0.stacktrace.0.method" equals "com.example.bugsnagcrashplugin.CrashHelper.triggerJvmException()"
        And the exception "stacktrace.0.file" equals "CrashHelper.java"
        And the event "exceptions.0.stacktrace.0.lineNumber" equals 12
        And the error payload field "events.0.threads" is null

        # App data
        And the event "app.id" equals "com.bugsnag.mazerunner"
        And the event "app.releaseStage" equals "production"
        And the event "app.type" equals "android"
        And the event "app.version" equals "1.2.3"
        And the event "app.versionCode" equals "1"
        And the error payload field "events.0.app.duration" is not null
        And the error payload field "events.0.app.durationInForeground" is not null
        And the event "app.inForeground" equals "true"
        And the event "app.isLaunching" equals "false"
        And the error payload field "events.0.app.memoryUsage" is not null
        And the event "app.name" equals "maze_runner"
        And the event "app.lowMemory" equals "false"

        # Device data
        And the error payload field "events.0.device.cpuAbi" is a non-empty array
        And the event "device.jailbroken" equals "false"
        And the event "device.id" is not null
        And the error payload field "events.0.device.id" is stored as the value "device_id"
        And the event "device.locale" is not null
        And the event "device.manufacturer" is not null
        And the event "device.model" is not null
        And the event "device.osName" equals "android"
        And the event "device.osVersion" is not null
        And the event "device.runtimeVersions" is not null
        And the error payload field "events.0.device.totalMemory" is not null
        And the error payload field "events.0.device.freeDisk" is not null
        And the error payload field "events.0.device.freeMemory" is not null
        And the event "device.orientation" equals "portrait"
        And the event "device.time" is not null
        And the event "device.locationStatus" is not null
        And the event "device.emulator" equals "false"
        And the event "device.networkAccess" is not null
        And the event "device.charging" is not null
        And the event "device.screenDensity" is not null
        And the event "device.dpi" is not null
        And the event "device.screenResolution" is not null
        And the event "device.brand" is not null
        And the event "device.batteryLevel" is not null

        # Metadata
        And the event "metaData.Unity.unityException" equals "true"
        And the event "metaData.Unity.unityLogType" equals "Exception"

        # User
        And the event "user.id" is not null

        # Breadcrumbs
        And the event has a "state" breadcrumb named "Bugsnag loaded"
        And the event has a "manual" breadcrumb named "String breadcrumb clicked"
        And the event has a "navigation" breadcrumb named "Tuple breadcrumb clicked"

        # Context
        And the event "context" equals "My context"

        # MetaData
        And the event "metaData.Unity.unityException" equals "true"
        And the event "metaData.Unity.osLanguage" is not null
        And the event "metaData.Unity.platform" equals "Android"
        And the event "metaData.Unity.version" equals "1.0"
        And the event "metaData.Unity.companyName" equals "DefaultCompany"
        And the event "metaData.Unity.productName" equals "maze_runner"
        And the event "metaData.Unity.unityLogType" equals "Exception"

        # Runtime versions
        And the error payload field "events.0.device.runtimeVersions.androidApiLevel" is not null
        And the error payload field "events.0.device.runtimeVersions.osBuild" is not null
        And the error payload field "events.0.device.runtimeVersions.unity" is not null