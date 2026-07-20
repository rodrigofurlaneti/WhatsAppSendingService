Feature: Send WhatsApp message
  As a client of the WhatsApp Sending Service
  I want to submit a message and have it delivered autonomously
  So that recipients receive WhatsApp texts without me managing the provider directly

  Scenario: Successfully queue and deliver a text message
    Given the WhatsApp provider is available
    When I submit a message to "+55 11 99999-8888" with body "Hello from BDD"
    Then the request is accepted with status "Pending"
    When the outbox dispatcher runs
    Then the message status becomes "Sent"
    And the provider received 1 message

  Scenario: Delivery fails when the provider rejects the message
    Given the WhatsApp provider is failing with reason "invalid access token"
    When I submit a message to "5511888887777" with body "Hi there"
    Then the request is accepted with status "Pending"
    When the outbox dispatcher runs
    Then the message status becomes "Failed"
    And the failure reason is "invalid access token"

  Scenario: Reject a message with an invalid phone number
    Given the WhatsApp provider is available
    When I submit a message to "123" with body "Hi"
    Then the request is rejected with a validation error
