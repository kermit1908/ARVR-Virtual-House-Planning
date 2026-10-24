"""
FastAPI Backend for AR/VR House Planning Project.
Provides local REST API persistence and synchronization for house planning projects.
Run locally with:
    cd Backend
    pip install -r requirements.txt
    uvicorn main:app --reload --port 8000
"""

import json
import sqlite3
from datetime import datetime
from typing import List, Optional, Any, Dict
from fastapi import FastAPI, HTTPException, status
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel, Field

# ---------------------------------------------------------------------------
# Database Setup
# ---------------------------------------------------------------------------

DB_FILE = "house_planner.db"

def get_db():
    conn = sqlite3.connect(DB_FILE)
    conn.row_factory = sqlite3.Row
    return conn

def init_db():
    conn = get_db()
    cursor = conn.cursor()
    cursor.execute("""
        CREATE TABLE IF NOT EXISTS projects (
            project_id TEXT PRIMARY KEY,
            project_name TEXT NOT NULL,
            schema_version INTEGER DEFAULT 1,
            created_at TEXT NOT NULL,
            modified_at TEXT NOT NULL,
            room_count INTEGER DEFAULT 0,
            furniture_count INTEGER DEFAULT 0,
            raw_json TEXT NOT NULL
        )
    """)
    conn.commit()
    conn.close()

# ---------------------------------------------------------------------------
# Pydantic Schemas
# ---------------------------------------------------------------------------

class ProjectSummaryResponse(BaseModel):
    projectId: str
    projectName: str
    createdAt: str
    modifiedAt: str
    roomCount: int
    furnitureCount: int

class HealthResponse(BaseModel):
    status: str
    timestamp: str
    version: str

# ---------------------------------------------------------------------------
# FastAPI Application
# ---------------------------------------------------------------------------

app = FastAPI(
    title="AR/VR House Planner API",
    description="Lightweight local persistence and sync backend for the Unity AR/VR House Planning system.",
    version="1.0.0"
)

# Enable CORS for Unity Editor & Web clients
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

@app.on_event("startup")
def on_startup():
    init_db()

# ---------------------------------------------------------------------------
# Endpoints
# ---------------------------------------------------------------------------

@app.get("/api/health", response_model=HealthResponse)
def health_check():
    """Health check endpoint to verify backend availability from Unity."""
    return HealthResponse(
        status="healthy",
        timestamp=datetime.utcnow().isoformat(),
        version="1.0.0"
    )

@app.get("/api/projects", response_model=List[ProjectSummaryResponse])
def list_projects():
    """Returns a summary list of all saved house projects."""
    conn = get_db()
    cursor = conn.cursor()
    cursor.execute("SELECT project_id, project_name, created_at, modified_at, room_count, furniture_count FROM projects ORDER BY modified_at DESC")
    rows = cursor.fetchall()
    conn.close()

    return [
        ProjectSummaryResponse(
            projectId=row["project_id"],
            projectName=row["project_name"],
            createdAt=row["created_at"],
            modifiedAt=row["modified_at"],
            roomCount=row["room_count"],
            furnitureCount=row["furniture_count"]
        )
        for row in rows
    ]

@app.get("/api/projects/{project_id}")
def get_project(project_id: str):
    """Retrieves the full ProjectData JSON for a given project ID."""
    conn = get_db()
    cursor = conn.cursor()
    cursor.execute("SELECT raw_json FROM projects WHERE project_id = ?", (project_id,))
    row = cursor.fetchone()
    conn.close()

    if not row:
        raise HTTPException(status_code=404, detail=f"Project with ID '{project_id}' not found.")

    return json.loads(row["raw_json"])

@app.post("/api/projects", status_code=status.HTTP_201_CREATED)
def create_project(project_data: Dict[str, Any]):
    """Creates a new house project record."""
    project_id = project_data.get("projectId")
    project_name = project_data.get("projectName", "Untitled Project")
    schema_ver = project_data.get("schemaVersion", 1)
    rooms = project_data.get("rooms", [])
    furniture = project_data.get("furnitureItems", [])

    if not project_id:
        raise HTTPException(status_code=400, detail="Missing required field: projectId.")

    now = datetime.utcnow().isoformat()
    created_at = project_data.get("createdAtIso", now)
    modified_at = project_data.get("modifiedAtIso", now)

    raw_json = json.dumps(project_data)

    conn = get_db()
    cursor = conn.cursor()
    try:
        cursor.execute("""
            INSERT INTO projects (project_id, project_name, schema_version, created_at, modified_at, room_count, furniture_count, raw_json)
            VALUES (?, ?, ?, ?, ?, ?, ?, ?)
        """, (project_id, project_name, schema_ver, created_at, modified_at, len(rooms), len(furniture), raw_json))
        conn.commit()
    except sqlite3.IntegrityError:
        conn.close()
        raise HTTPException(status_code=409, detail=f"Project '{project_id}' already exists. Use PUT to update.")
    finally:
        conn.close()

    return {"status": "created", "projectId": project_id}

@app.put("/api/projects/{project_id}")
def update_project(project_id: str, project_data: Dict[str, Any]):
    """Updates an existing house project record."""
    project_name = project_data.get("projectName", "Untitled Project")
    schema_ver = project_data.get("schemaVersion", 1)
    rooms = project_data.get("rooms", [])
    furniture = project_data.get("furnitureItems", [])
    modified_at = datetime.utcnow().isoformat()
    project_data["modifiedAtIso"] = modified_at
    raw_json = json.dumps(project_data)

    conn = get_db()
    cursor = conn.cursor()
    cursor.execute("""
        UPDATE projects
        SET project_name = ?, schema_version = ?, modified_at = ?, room_count = ?, furniture_count = ?, raw_json = ?
        WHERE project_id = ?
    """, (project_name, schema_ver, modified_at, len(rooms), len(furniture), raw_json, project_id))
    rows_affected = cursor.rowcount
    conn.commit()
    conn.close()

    if rows_affected == 0:
        raise HTTPException(status_code=404, detail=f"Project '{project_id}' not found.")

    return {"status": "updated", "projectId": project_id, "modifiedAt": modified_at}

@app.delete("/api/projects/{project_id}")
def delete_project(project_id: str):
    """Permanently deletes a house project record."""
    conn = get_db()
    cursor = conn.cursor()
    cursor.execute("DELETE FROM projects WHERE project_id = ?", (project_id,))
    rows_affected = cursor.rowcount
    conn.commit()
    conn.close()

    if rows_affected == 0:
        raise HTTPException(status_code=404, detail=f"Project '{project_id}' not found.")

    return {"status": "deleted", "projectId": project_id}
