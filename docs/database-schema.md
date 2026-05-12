# Future Mock Database Tables

This document describes the database tables represented by the showcase API entities.

## Users

Stores user account information.

| Column | Type | Notes |
|---|---|---|
| UserId | guid | Primary key |
| FirstName | string | User first name |
| LastName | string | User last name |
| Email | string | Unique login/contact email |
| PhoneNumber | string | User phone |
| PasswordHash | string | Stored password hash |
| PasswordSalt | string | Password salt for hashing |
| Role | string | User role (e.g., Customer, Admin) |
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
| RecommendedFor | string | Example user use case |

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

Stores temporary user reservations.

| Column | Type | Notes |
|---|---|---|
| ReservationId | guid | Primary key |
| UserId | guid | Foreign key to Users |
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
| UserId | guid | Foreign key to Users |
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
| UserId | guid | Foreign key to Users |
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
| UserId | guid | Foreign key to Users |
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

Stores user support messages.

| Column | Type | Notes |
|---|---|---|
| SupportRequestId | guid | Primary key |
| UserId | guid | Foreign key to Users |
| RentalId | guid nullable | Optional foreign key to Rentals |
| Subject | string | Request subject |
| Message | string | User message |
| Status | enum | Open, InProgress, Resolved, Closed |
| CreatedAtUtc | datetime | Created timestamp |

## Relationship overview

```text
Users
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