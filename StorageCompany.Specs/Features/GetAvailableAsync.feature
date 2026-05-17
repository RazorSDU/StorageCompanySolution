Feature: GetAvailableAsync
  As a customer who is seeking rental of a storage unit
  I want to find vacant/available storage units
  So that I can rent one for my chosen period

  Background:
    Given the following storage unit exists:
      | UnitNumber | Facility   | UnitType | MonthlyPrice |
      | U-01       | Facility A | Small    | 500          |
      | U-02       | Facility A | Medium   | 800          |
      | U-03       | Facility A | Small    | 450          |
      | U-04       | Facility B | Medium   | 1200         |

  # A1
  Scenario: No filters returns all available storage units
    When I search for available storage units with no filters
    Then I should receive a non-empty list of storage units

  # A2
  Scenario: Max price filter set above unit price returns storage unit
    When I search for available storage units with a max price of 600
    Then I should receive a non-empty list of storage units

  # A3
  Scenario: Max price filter equal to unit price returns storage unit
    When I search for available storage units with a max price of 500
    Then I should receive a non-empty list of storage units

  # A4
  Scenario: Max price filter set below unit price returns no storage units
    When I search for available storage units with a max price of 400
    Then I should receive an empty list of storage units

  # A5
  Scenario: Matching unit type filter returns storage unit
    When I search for available storage units with unit type "Small"
    Then I should receive a non-empty list of storage units

  # A6
  Scenario: Non-matching unit type filter returns no storage units
    When I search for available storage units with unit type "Large"
    Then I should receive an empty list of storage units

  # A7
  Scenario: Matching facility filter returns storage unit
    When I search for available storage units from facility "Facility A"
    Then I should receive a non-empty list of storage units

  # A8
  Scenario: Non-matching facility filter returns no units
    When I search for available storage units from facility "Facility C"
    Then I should receive an empty list of storage units

  # A9
  Scenario: All filters matching returns storage unit
    When I search for available storage units from facility "Facility A" with unit type "Small" and max price of 500
    Then I should receive a non-empty list of storage units

  # A10
  Scenario: All filters matching except max price too low returns no storage units
    When I search for available storage units from facility "Facility A" with unit type "Small" and max price of 400
    Then I should receive an empty list of storage units

  # A11
  Scenario: Matching facility and price but non-matching unit type returns no storage units
    When I search for available storage units from facility "Facility A" with unit type "Large" and max price of 500
    Then I should receive an empty list of storage units

  # A12
  Scenario: Matching unit type and price but non-matching facility returns no storage units
    When I search for available storage units from facility "Facility C" with unit type "Small" and max price of 500
    Then I should receive an empty list of storage units
