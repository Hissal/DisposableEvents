# TestGuidelines for DisposableEvents Library

## Overview
This document outlines the guidelines and best practices for writing tests for the DisposableEvents library. The goal is to ensure that all tests are consistent, maintainable, and effectively validate the functionality of the library.

## Test Structure
- **Arrange, Act, Assert (AAA) Pattern**: Each test should follow the AAA pattern to enhance readability and maintainability. (Include comments for each phase in larger tests where each phase is not immediately clear.)
  - **Arrange**: Set up the necessary objects and state.
  - **Act**: Execute the method or functionality being tested.
  - **Assert**: Verify that the outcome is as expected.
- **Single Responsibility**: Each test should focus on a single aspect of functionality. Avoid combining multiple assertions that test different behaviors in one test method.
- **Setup and Teardown**: Use setup and teardown methods to initialize and clean up resources needed for tests. This helps to avoid code duplication and ensures a clean state for each test.
- **Test Naming**: Use descriptive names for test methods that clearly indicate what is being tested and the expected outcome. For example, `Should_InvokeEventHandler_When_EventIsRaised`.
- **Failure Messages**: Provide clear and informative failure messages in assertions to aid in diagnosing test failures especially when the reason for failure is not apparent otherwise.

## Test Coverage
- **Comprehensive Coverage**: Ensure that all public methods and properties of the DisposableEvents library are covered by tests. This includes edge cases and error conditions.
- **Boundary Testing**: Include tests that cover boundary conditions, such as empty collections, maximum limits, and null inputs within the limit of nullable reference types.
- **Performance Testing**: Where applicable, include tests that measure the performance of critical methods to ensure they meet acceptable thresholds.
- **Exception Handling**: Write tests to verify that the library correctly handles exceptions and edge cases.

## Test Tools and Frameworks
- **Testing Framework**: Use xUnit as the library for writing tests.
- **Mocking**: Utilize NSubstitute for creating mock objects and dependencies.
- **Assertions**: Prefer FluentAssertions for more readable and expressive assertions in tests.
- **Alloc Testing**: Where allocations are a consideration use dotMemory for collecting memory usage data and verifying that acceptable conditions are met.