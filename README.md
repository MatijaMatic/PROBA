# AI Logistics Platform

A full-stack intelligent logistics management system with three AI/ML modules built on **ASP.NET Core**, **Blazor WebAssembly**, **Python FastAPI**, and **SQL Server**.

## Architecture

```
Blazor WebAssembly (UI)
        ↓
ASP.NET Core Web API (.NET 8)
        ↓
┌─────────────────────────────────┐
│           AI Layer              │
│  ┌──────────────────────────┐   │
│  │  LLM Service (OpenAI)    │   │  ← Module 1: NL → SQL
│  │  GPT-4o mini             │   │
│  └──────────────────────────┘   │
│  ┌──────────────────────────┐   │
│  │  ML Service (Python)     │   │  ← Module 2: Delay Prediction
│  │  FastAPI + scikit-learn  │   │
│  └──────────────────────────┘   │
│  ┌──────────────────────────┐   │
│  │  Route Service (C#)      │   │  ← Module 3: Route Optimization
│  │  Dijkstra Algorithm      │   │
│  └──────────────────────────┘   │
└─────────────────────────────────┘
        ↓
    SQL Server
```

## Modules

### Module 1 – Natural Language → SQL (Main Module)

Users ask questions in natural language. GPT-4o mini generates a SQL query, the backend validates and executes it, and the results are returned.

**Example questions:**
- "Which carrier has the most delays in the last 30 days?"
- "What is the average transport cost per ton for the Beograd–Niš route?"
- "Show top 10 clients by cargo volume this year"

**Endpoint:** `POST /api/ai/query`

### Module 2 – Delay Prediction (Python ML)

A Random Forest model predicts the probability of transport delay based on route, carrier, departure hour, season, weather, and cargo type.

**Endpoint:** `POST /api/delay/predict`

### Module 3 – Route Optimization (Dijkstra)

Finds the **cheapest** and **fastest** routes between Serbian cities using Dijkstra's algorithm on a weighted transport graph.

**Endpoint:** `POST /api/route/optimize`

## Tech Stack

| Layer       | Technology                           |
|-------------|--------------------------------------|
| Frontend    | Blazor WebAssembly (.NET 8)          |
| Backend     | ASP.NET Core Web API (.NET 8)        |
| Database    | SQL Server                           |
| AI (NL→SQL) | OpenAI GPT-4o mini                   |
| AI (ML)     | Python 3.12 + FastAPI + scikit-learn |
| IDE         | Visual Studio 2022 / VS Code         |

## Setup & Installation

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/sql-server) + SSMS
- [Python 3.12+](https://www.python.org/downloads/)
- OpenAI API key

### 1. Database Setup

Open **SSMS** and run in order:

```
database/schema.sql   ← creates LogisticsDB and Transports table
database/seed.sql     ← inserts sample transport records
```

### 2. Backend (ASP.NET Core API)

1. Edit `LogisticsAI.Api/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=LogisticsDB;Trusted_Connection=True;TrustServerCertificate=True;"
     },
     "OpenAI": {
       "ApiKey": "YOUR_OPENAI_API_KEY_HERE"
     },
     "PythonApi": {
       "BaseUrl": "http://localhost:8000"
     }
   }
   ```

2. Run:
   ```bash
   cd LogisticsAI.Api
   dotnet run
   ```
   Swagger UI: `http://localhost:5000`

### 3. Python ML Service

```bash
cd python
pip install -r requirements.txt
python train_model.py        # train and save delay_model.pkl
uvicorn main:app --reload --port 8000
```

API docs: `http://localhost:8000/docs`

### 4. Frontend (Blazor WebAssembly)

1. Edit `LogisticsAI.Web/wwwroot/appsettings.json`:
   ```json
   { "ApiBaseUrl": "http://localhost:5000" }
   ```

2. Run:
   ```bash
   cd LogisticsAI.Web
   dotnet run
   ```
   App: `http://localhost:5001`

## Security

- NL→SQL module only allows **SELECT** queries — `INSERT`, `UPDATE`, `DELETE`, `DROP`, etc. are blocked.
- Store the OpenAI API key in environment variables or Azure Key Vault in production.

## Project Structure

```
LogisticsAI/
├── LogisticsAI.sln
├── LogisticsAI.Api/              ← ASP.NET Core Web API
│   ├── Controllers/
│   │   ├── AiController.cs       ← Module 1: NL→SQL
│   │   ├── DelayController.cs    ← Module 2: Delay prediction proxy
│   │   └── RouteController.cs    ← Module 3: Route optimization
│   ├── Services/
│   │   ├── AiService.cs          ← OpenAI GPT-4o mini integration
│   │   ├── SqlService.cs         ← SQL execution + security validation
│   │   └── RouteService.cs       ← Dijkstra algorithm
│   ├── Models/
│   └── Program.cs
├── LogisticsAI.Web/              ← Blazor WebAssembly Frontend
│   └── Pages/
│       ├── Home.razor            ← Dashboard
│       ├── NlQuery.razor         ← Module 1 UI
│       ├── DelayPrediction.razor ← Module 2 UI
│       └── RouteOptimizer.razor  ← Module 3 UI
├── python/                       ← Python ML microservice
│   ├── main.py                   ← FastAPI server
│   ├── train_model.py            ← Model training script
│   └── requirements.txt
└── database/
    ├── schema.sql
    └── seed.sql
```
