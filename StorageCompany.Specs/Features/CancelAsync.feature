Feature: CancelAsync
  As a customer
  I want to cancel my reservation
  So that the storage unit can become available again

  # A1
  Scenario: Reservation is already cancelled
    Given a reservation with status Cancelled
    When the customer cancels the reservation
    Then the reservation status should remain Cancelled

  # A2
  Scenario: Reservation is already expired
    Given a reservation with status Expired
    When the customer cancels the reservation
    Then the reservation status should remain Expired

  # A3
  Scenario: Active reservation with a reserved storage unit
    Given an active reservation
    And the storage unit is reserved
    When the customer cancels the reservation
    Then the reservation should be cancelled
    And the storage unit should become available

  # A4
  Scenario: Active reservation but storage unit is not reserved
    Given an active reservation
    And the storage unit exists but is not reserved
    When the customer cancels the reservation
    Then the reservation should be cancelled
    And the storage unit should not become available

  # A5
  Scenario: Active reservation but storage unit does not exist
    Given an active reservation
    And the storage unit does not exist
    When the customer cancels the reservation
    Then the reservation should be cancelled
