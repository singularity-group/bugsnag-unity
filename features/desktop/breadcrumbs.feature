Feature: Leaving breadcrumbs to attach to reports

    Scenario: Attaching a message breadcrumb directly
        When I run the game in the "MessageBreadcrumbNotify" state
        And I wait to receive an error
        Then the error is valid for the error reporting API sent by the Unity notifier
        And the exception "errorClass" equals "Exception"
        And the exception "message" equals "Collective failure"
        And the event has a "manual" breadcrumb named "Initialize bumpers"
