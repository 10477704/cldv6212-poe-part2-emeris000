# CoffeeNChill Canteen Management System — Part 2

CLDV6212 Portfolio of Evidence — Group 25

## Project Overview

CoffeeNChill is a cloud-enabled canteen management system built with Azure Functions, Azure Storage (Tables, Queues, Files), Docker, and Postman. Part 2 focuses on asynchronous order processing via Azure Storage Queues and orchestration with Docker Compose.

## Repository Structure

CoffeeNChill.Functions/
├── Functions/              # HTTP and Queue-triggered Azure Functions
│   ├── OrderQueueProducer.cs       # POST /api/orders/queue
│   ├── ProcessOrderQueue.cs        # Queue-triggered consumer
│   └── OrderQueryFunctions.cs      # GET endpoints for querying orders
├── Models/                 # Entity and DTO classes
│   ├── Order.cs                    # Orders table entity
│   └── OrderMessage.cs             # Queue message DTO
├── Services/               # Data access layer
│   └── OrderRepository.cs          # Orders Azure Table repository
├── MenuFunctions/          # Part 1 menu functions (preserved)
├── Helpers/                # Shared helpers
├── Dockerfile              # Multi-stage Docker build
├── docker-compose.yml      # Orchestrates Azurite + Functions
└── Program.cs              # DI registration

## Docker Hub

The published Part 2 image is available at:

https://hub.docker.com/r/emeris000/coffeenchill-functions

Pull command:
docker pull emeris000/coffeenchill-functions:v2.0

## Running Locally

### Prerequisites
- .NET 10 SDK
- Docker Desktop
- Postman (for testing)

### With Docker Compose (recommended)

    docker compose up -d

This starts:
- Azurite on ports 10000 (blob), 10001 (queue), 10002 (table)
- Functions host on port 8080

Access the API at: http://localhost:8080/api/orders/queue

### Without Docker (using Visual Studio + Azurite)

1. Start Azurite:
   docker run -d --name coffeenchill-azurite -p 10000:10000 -p 10001:10001 -p 10002:10002 mcr.microsoft.com/azure-storage/azurite

2. Press F5 in Visual Studio
3. API listens on http://localhost:7054

## API Endpoints

| Method | Route | Purpose |
|--------|-------|---------|
| POST | /api/orders/queue | Queue a new order (producer) |
| GET | /api/orders/by-id/{orderDate}/{orderId} | Retrieve a single order |
| GET | /api/orders/by-date/{orderDate} | List all orders on a date |
| POST | /api/menu | Create menu item (Part 1) |
| GET | /api/menu | List all menu items (Part 1) |

### Sample Order Request

{
    "OrderId": "ORD-2026-8801",
    "CustomerName": "Jane Smith",
    "SelectedItemSKUs": ["COF-001", "PAS-104"],
    "TotalPrice": 65.00,
    "OrderTimestamp": "2026-10-12T10:15:00Z"
}

## Postman Collection

The Postman collection is available in the /docs folder:
- CLDV6212_CoffeeNChill_P2.postman_collection.json
- CoffeeNChill_Local.postman_environment.json

Import both files into Postman and select the CoffeeNChill Local environment.

## Changelog — Part 2

### Added
- Order processing queue: order-processing-queue on Azurite
- Queue producer (OrderQueueProducer.cs): HTTP endpoint that serializes orders to JSON, Base64-encodes them, and pushes to the queue
- Queue consumer (ProcessOrderQueue.cs): Queue-triggered function that deserializes messages, writes to the Orders table, and simulates the barista lifecycle (Received -> Preparing -> Ready -> Collected)
- Orders Azure Table: PartitionKey = OrderDate, RowKey = OrderId
- Order query endpoints: GET /api/orders/by-id/... and GET /api/orders/by-date/...
- Docker Compose orchestration: single docker compose up starts Azurite + Functions

### Changed
- Docker Hub image: emeris000/coffeenchill-functions:v2.0 (Base64 fix applied)
- Program.cs: DI now registers TableServiceClient, QueueServiceClient, and OrderRepository as singletons
- Route disambiguation: orders/date/{orderDate} renamed to orders/by-date/{orderDate} to avoid collision with orders/{orderDate}/{orderId}

### Fixed
- Queue trigger binding: switched from string to QueueMessage type with Base64 encoding for reliable deserialization in the isolated worker model
- DI lifetime bug: OrderRepository changed from AddScoped to AddSingleton — queue-triggered functions are singletons and cannot consume scoped dependencies

## Team Contributions

- Member 1: Part 1 menu functions, initial project scaffold, Docker Hub base image
- Member 2 (emeris000): Order models, repository, queue producer, queue consumer, query functions, Dockerfile, docker-compose, Postman collection, this README
- Member 3: Compose orchestration coordination, VPS deployment (Part 3)

## Video Demonstration

YouTube link: [To be added after recording]

## Git Commits (Part 2)

77e1dad chore(part2): add Dockerfile, docker-compose, and DI wiring
e1d837f feat(part2): add OrderQueryFunctions with by-id and by-date routes
39e4ae92 feat(part2): implement ProcessOrderQueue with status lifecycle simulation
d31f7f84 feat(part2): add OrderRepository, queue producer with Base64 fix, queue consumer, and query functions
6830c00 feat(part2): add Order and OrderMessage models for queue processing

## License

This project is part of The IIE Bachelor of Computer and Information Sciences coursework.
