Feature: CreateAsync
  As a customer
  I want to make a reservation for a storage unit
  So that I can secure it for my chosen move-in date

  # A1
  Scenario: Customer does not exist
    Given a customer that does not exist
    When the customer creates a reservation with a future move-in date
    Then the reservation should not be created

  # A2
  Scenario: Customer account is not active
    Given an inactive customer
    When the customer creates a reservation with a future move-in date
    Then the reservation should not be created

  # A3
  Scenario: Storage unit does not exist
    Given an active customer
    And a storage unit that does not exist
    When the customer creates a reservation with a future move-in date
    Then the reservation should not be created

  # A4
  Scenario: Storage unit is not available
    Given an active customer
    And a storage unit that is not available
    When the customer creates a reservation with a future move-in date
    Then the reservation should not be created

  # A5
  Scenario: Move-in date is in the past
    Given an active customer
    And an available storage unit
    When the customer creates a reservation with a past move-in date
    Then the reservation should not be created

  # A6
  Scenario: All conditions are met
    Given an active customer
    And an available storage unit
    When the customer creates a reservation with a future move-in date
    Then the reservation should be created successfully
    And the storage unit should be marked as reserved
