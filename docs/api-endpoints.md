# API Endpoints

## Customers

| Method | Endpoint | Purpose |
|---|---|---|
| GET | `/api/customers` | Get all customers |
| GET | `/api/customers/{id}` | Get customer by ID |
| POST | `/api/customers` | Create customer |
| PUT | `/api/customers/{id}` | Update customer |
| GET | `/api/customers/{customerId}/rentals` | Get customer rentals |
| GET | `/api/customers/{customerId}/reservations` | Get customer reservations |
| GET | `/api/customers/{customerId}/payments` | Get customer payments |
| GET | `/api/customers/{customerId}/invoices` | Get customer invoices |
| GET | `/api/customers/{customerId}/support-requests` | Get customer support requests |

## Facilities

| Method | Endpoint | Purpose |
|---|---|---|
| GET | `/api/facilities` | Get all facilities. Optional `?search=` query |
| GET | `/api/facilities/{id}` | Get facility by ID |
| GET | `/api/facilities/{id}/units` | Get units at facility |
| GET | `/api/facilities/{id}/available-units` | Get available units at facility |

## Storage unit types

| Method | Endpoint | Purpose |
|---|---|---|
| GET | `/api/storage-unit-types` | Get all unit types |
| GET | `/api/storage-unit-types/{id}` | Get unit type by ID |

## Storage units

| Method | Endpoint | Purpose |
|---|---|---|
| GET | `/api/storage-units` | Get all storage units |
| GET | `/api/storage-units/{id}` | Get storage unit by ID |
| GET | `/api/storage-units/available` | Get available units. Optional `facilityId`, `unitTypeId`, `maxPrice` queries |

## Reservations

| Method | Endpoint | Purpose |
|---|---|---|
| GET | `/api/reservations/{id}` | Get reservation by ID |
| POST | `/api/reservations` | Create reservation |
| DELETE | `/api/reservations/{id}` | Cancel reservation |

## Rentals

| Method | Endpoint | Purpose |
|---|---|---|
| GET | `/api/rentals/{id}` | Get rental by ID |
| POST | `/api/rentals/from-reservation` | Convert reservation to active rental |
| POST | `/api/rentals/direct` | Create rental directly |
| PUT | `/api/rentals/{id}/end` | End rental |
| GET | `/api/rentals/{rentalId}/access-code` | Get active access code |
| GET | `/api/rentals/{rentalId}/payments` | Get rental payments |
| GET | `/api/rentals/{rentalId}/invoices` | Get rental invoices |

## Payments

| Method | Endpoint | Purpose |
|---|---|---|
| GET | `/api/payments/{id}` | Get payment by ID |
| POST | `/api/payments` | Create mock paid payment |

## Invoices

| Method | Endpoint | Purpose |
|---|---|---|
| GET | `/api/invoices/{id}` | Get invoice by ID |
| POST | `/api/invoices` | Generate invoice for rental |
| PUT | `/api/invoices/{id}/mark-paid` | Mark invoice as paid |

## Support requests

| Method | Endpoint | Purpose |
|---|---|---|
| GET | `/api/support-requests` | Get all support requests |
| GET | `/api/support-requests/{id}` | Get support request by ID |
| POST | `/api/support-requests` | Create support request |
| PUT | `/api/support-requests/{id}/status` | Update support request status |
