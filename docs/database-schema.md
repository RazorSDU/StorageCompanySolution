# Future Mock Database Tables

This document describes the database tables represented by the showcase API entities.

## Customers

Stores customer account information.

| Column | Type | Notes |
|---|---|---|
| CustomerId | guid | Primary key |
| FirstName | string | Customer first name |
| LastName | string | Customer last name |
| Email | string | Unique login/contact email |
| PhoneNumber | string | Customer phone |
| PasswordHash | string | Stored password hash |
| CreatedAtUtc | datetime | Created timestamp |
| IsActive | boolean | Account status |

## Facilities

Stores physical storage locations.

| Column | Type | Notes |
|---|---|---|
| FacilityId | guid | Primary key |
| Name | string | Facility name |
| Address | string | Street address |
| City | string | City |
| PostalCode | string | Postal code |
| Country | string | Country |
| PhoneNumber | string | Facility phone |
| Email | string | Facility email |
| AccessHours | string | Customer access hours |
| OfficeHours | string | Staffed office hours |
| HasParking | boolean | Parking available |
| HasElevator | boolean | Elevator available |
| HasCCTV | boolean | CCTV/security cameras available |

## StorageUnitTypes

Stores size and category information.

| Column | Type | Notes |
|---|---|---|
| UnitTypeId | guid | Primary key |
| Name | string | Small, Medium, Large, Business |
| SizeInSquareMeters | decimal | Unit size |
| Description | string | Human-readable explanation |
| RecommendedFor | string | Example customer use case |

## StorageUnits

Stores individual physical storage units.

| Column | Type | Notes |
|---|---|---|
| StorageUnitId | guid | Primary key |
| FacilityId | guid | Foreign key to Facilities |
| UnitTypeId | guid | Foreign key to StorageUnitTypes |
| UnitNumber | string | Physical unit number |
| Floor | int | Floor number |
| MonthlyPrice | decimal | Monthly rental price |
| Status | enum | Available, Reserved, Rented, Maintenance |
| IsClimateControlled | boolean | Climate-controlled unit |
| IsDriveUp | boolean | Drive-up access |

## Reservations

Stores temporary customer reservations.

| Column | Type | Notes |
|---|---|---|
| ReservationId | guid | Primary key |
| CustomerId | guid | Foreign key to Customers |
| StorageUnitId | guid | Foreign key to StorageUnits |
| ReservationDateUtc | datetime | Reservation date |
| MoveInDateUtc | datetime | Desired move-in date |
| ExpiresAtUtc | datetime nullable | Expiry date |
| Status | enum | Pending, Confirmed, Cancelled, Expired |

## Rentals

Stores active and historical rental agreements.

| Column | Type | Notes |
|---|---|---|
| RentalId | guid | Primary key |
| CustomerId | guid | Foreign key to Customers |
| StorageUnitId | guid | Foreign key to StorageUnits |
| StartDateUtc | datetime | Rental start date |
| EndDateUtc | datetime nullable | Rental end date |
| MonthlyPrice | decimal | Snapshot of price at rental start |
| Status | enum | Active, Cancelled, Ended, Overdue |

## Payments

Stores payment records.

| Column | Type | Notes |
|---|---|---|
| PaymentId | guid | Primary key |
| RentalId | guid | Foreign key to Rentals |
| CustomerId | guid | Foreign key to Customers |
| InvoiceId | guid nullable | Optional link to invoice |
| Amount | decimal | Payment amount |
| PaymentDateUtc | datetime | Payment date |
| PaymentMethod | enum | Card, BankTransfer, MobilePay, Cash, Mock |
| Status | enum | Pending, Paid, Failed, Refunded |
| TransactionReference | string | Mock transaction reference |

## Invoices

Stores billing records.

| Column | Type | Notes |
|---|---|---|
| InvoiceId | guid | Primary key |
| RentalId | guid | Foreign key to Rentals |
| CustomerId | guid | Foreign key to Customers |
| InvoiceNumber | string | Human-readable invoice number |
| Amount | decimal | Invoice amount |
| DueDateUtc | datetime | Due date |
| Status | enum | Unpaid, Paid, Overdue, Cancelled |

## AccessCodes

Stores access code information for active rentals.

| Column | Type | Notes |
|---|---|---|
| AccessCodeId | guid | Primary key |
| RentalId | guid | Foreign key to Rentals |
| Code | string | Gate/door PIN code |
| IsActive | boolean | Active/inactive |
| CreatedAtUtc | datetime | Created timestamp |
| ExpiresAtUtc | datetime nullable | Expiry date |

## SupportRequests

Stores customer support messages.

| Column | Type | Notes |
|---|---|---|
| SupportRequestId | guid | Primary key |
| CustomerId | guid | Foreign key to Customers |
| RentalId | guid nullable | Optional foreign key to Rentals |
| Subject | string | Request subject |
| Message | string | Customer message |
| Status | enum | Open, InProgress, Resolved, Closed |
| CreatedAtUtc | datetime | Created timestamp |

## Relationship overview

```text
Customers
   ├── Reservations
   ├── Rentals
   │      ├── Payments
   │      ├── Invoices
   │      └── AccessCodes
   └── SupportRequests

Facilities
   └── StorageUnits
          └── StorageUnitTypes

StorageUnits
   ├── Reservations
   └── Rentals
```
