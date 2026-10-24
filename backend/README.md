# AR/VR House Planner — Backend Service

A lightweight, local, zero-cost REST API built with FastAPI and SQLite.
This backend is **optional** — the Unity application functions 100% offline using local JSON persistence in `Application.persistentDataPath`.

---

## Prerequisites
- Python 3.9+ (Installed natively on macOS)
- `pip`

---

## Quickstart

### 1. Create a Python Virtual Environment
```bash
cd Backend
python3 -m venv venv
source venv/bin/activate
```

### 2. Install Dependencies
```bash
pip install -r requirements.txt
```

### 3. Run the Development Server
```bash
uvicorn main:app --reload --port 8000
```

The server will start at `http://localhost:8000`.

---

## Interactive API Documentation
- **Swagger UI**: Visit [http://localhost:8000/docs](http://localhost:8000/docs) in your browser.
- **ReDoc**: Visit [http://localhost:8000/redoc](http://localhost:8000/redoc).

---

## API Endpoints

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/health` | Service health status check |
| `GET` | `/api/projects` | List summaries of all stored projects |
| `GET` | `/api/projects/{id}` | Retrieve full ProjectData JSON |
| `POST` | `/api/projects` | Save a new project |
| `PUT` | `/api/projects/{id}` | Update an existing project |
| `DELETE` | `/api/projects/{id}` | Delete a project by ID |

---

## Database Storage
Projects are stored in an SQLite database file named `house_planner.db` located inside the `Backend/` directory. No external database server or cloud subscription is required.
