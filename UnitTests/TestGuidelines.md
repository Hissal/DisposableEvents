# Test Guidelines

This document outlines the guidelines for writing unit tests for the DisposableEvent system, including specific requirements for different event types and general testing practices.

### General Guidelines
- Each event type should have a dedicated test class named after the event (e.g., `BufferedEventTests` for `BufferedEvent`).
- Use NUnit for all test cases, following the `[Test]` attribute convention.
- Organize tests into folders by event type and system (e.g., `Events/NormalEvents/`, `OtherSystems/EventFilters/`).
- Use clear, descriptive test method names reflecting the scenario being tested.
- Prefer Arrange-Act-Assert structure in each test for clarity.
- Use mock or test observer classes (e.g., `TestObserver`) to verify event notifications.
- Ensure tests are independent and do not rely on shared state.

### Events
**All events should be tested for:**
- **Publish_InvokesSubscriber**: Ensure the event is called and received.
- **OnError_HandlesError_WhenThrown**: Ensure that if an exception is thrown during invocation, OnError is received.
- **Dispose_CallsOnCompleted**: Ensure that when the event is disposed, OnCompleted is called.
- **DisposingSubscription_Unsubscribes**: Ensure that disposing a subscription unsubscribes from the event.
- **MultipleSubscribers_AllReceivePublishedValue**: Ensure that multiple subscribers receive the published value.
- **SubscribeAfterDispose_ReceivesOnCompleted**: Ensure that subscribing after the event has been disposed receives an OnCompleted notification.
- **PublishWithNoSubscribers_DoesNotThrow**: Ensure that publishing an event with no subscribers does not throw an exception.
- **Subscribe_AppliesFilters**: Ensure that filters are applied to subscriptions.