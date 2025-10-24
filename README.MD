# 🚌 Bus Ticket Reservation System

> **Internship Assignment**: Full-stack Bus Ticket Reservation System  
> **Deadline**: October 28, 2025  
> **Tech Stack**: .NET 9 (Backend) + Angular 18 (Frontend)

A complete Full-Stack Bus Ticket Reservation System built with **Clean Architecture** and **Domain-Driven Design** principles.

## 🎯 Features

- 🔍 **Bus Search**: Find available buses by route and date
- 🪑 **Seat Selection**: Interactive seat layout with real-time availability
- 🎫 **Ticket Booking**: Reserve seats with passenger details
- 📋 **Booking Management**: View and cancel reservations
- ✅ **Validation**: Comprehensive input and business validation
- 📚 **API Documentation**: Interactive Swagger UI
- 🎨 **Modern UI**: Responsive Angular frontend with Bootstrap

## 📁 Project Structure

```
Bus Ticket Reservation System/
├── src/                           # 🔧 Backend (.NET 9)
│   ├── BusTicketReservation.Domain/
│   ├── BusTicketReservation.Application/
│   ├── BusTicketReservation.Infrastructure/
│   └── BusTicketReservation.WebApi/
├── frontend/                      # 🎨 Frontend (Angular 18)
│   └── src/app/
├── tests/                         # 🧪 Unit Tests
└── BusTicketReservation.sln      # Solution file
```

## 🚀 Quick Start

### Prerequisites
- .NET 9 SDK
- Node.js 18+ and npm
- Angular CLI 18
- Visual Studio Code or Visual Studio 2022

### Running the Application

#### Option 1: Run Backend and Frontend Separately

**Terminal 1 - Backend:**
```bash
cd src/BusTicketReservation.WebApi
dotnet run --urls "http://localhost:5003"
```

**Terminal 2 - Frontend:**
```bash
cd frontend
npm install
ng serve
```

**Access:**
- **Frontend**: http://localhost:4200
- **Backend API**: http://localhost:5003
- **Swagger UI**: http://localhost:5003/swagger

#### Option 2: Run Both Together
```bash
cd frontend
npm install
npm run dev
```

This starts both backend and frontend concurrently.

## 🔌 API Endpoints

| Method | Endpoint | Description | Status |
|--------|----------|-------------|--------|
| `POST` | `/api/busreservation/search` | Search available buses | ✅ Working |
| `GET` | `/api/busreservation/seats/{scheduleId}` | Get seat availability | ✅ Working |
| `POST` | `/api/busreservation/book` | Book tickets | ✅ Working |
| `GET` | `/api/busreservation/ticket/{ticketId}` | Get ticket details | ✅ Working |
| `DELETE` | `/api/busreservation/cancel/{ticketId}` | Cancel ticket | ✅ Working |

## 🧪 API Testing

### Method 1: Swagger UI (Recommended ⭐)
1. Start the API: `dotnet run --urls "http://localhost:5003"`
2. Open: **http://localhost:5003/swagger**
3. Click on any endpoint → "Try it out" → Fill parameters → "Execute"

### Method 2: REST Client Extension
1. Install "REST Client" extension in VS Code
2. Open `api-tests.http`
3. Click "Send Request" above any test

### Method 3: Example Requests

#### Search Buses
```json
POST http://localhost:5003/api/busreservation/search
Content-Type: application/json

{
  "fromCity": "New York",
  "toCity": "Boston",
  "journeyDate": "2025-10-25",
  "passengerCount": 2
}
```

**Expected Response:**
```json
{
  "success": true,
  "message": "Buses found successfully",
  "data": {
    "availableBuses": [
      {
        "scheduleId": 1,
        "busName": "Express Bus 101",
        "companyName": "City Transport",
        "departureTime": "08:00:00",
        "arrivalTime": "12:00:00", 
        "price": 45.00,
        "currency": "USD",
        "availableSeats": 28,
        "totalSeats": 40
      }
    ]
  }
}
```