import uuid
from datetime import datetime, timezone
import sqlite3
from typing import List
from fastapi import APIRouter, Depends, HTTPException, status
from backend.app.database.database import get_db
from backend.app.schemas.common import ProjectCreate, ProjectUpdate, ProjectResponse
from backend.app.services.bim_service import BIMService
from backend.app.api.deps import get_current_user

router = APIRouter(prefix="/projects", tags=["Projects"])

@router.post("", response_model=ProjectResponse, status_code=status.HTTP_201_CREATED)
def create_project(
    proj_in: ProjectCreate,
    current_user: dict = Depends(get_current_user),
    conn: sqlite3.Connection = Depends(get_db)
):
    project_id = str(uuid.uuid4())
    plot_area = round(proj_in.plot_width * proj_in.plot_length, 2)
    now_str = datetime.now(timezone.utc).strftime("%Y-%m-%d %H:%M:%S")

    cursor = conn.cursor()
    cursor.execute(
        """INSERT INTO projects 
           (id, owner_id, name, description, location, plot_width, plot_length, plot_area, status, created_at, updated_at)
           VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)""",
        (project_id, current_user["id"], proj_in.name, proj_in.description, proj_in.location,
         proj_in.plot_width, proj_in.plot_length, plot_area, "model_generated", now_str, now_str)
    )
    conn.commit()

    # Automatically generate initial default BIM building model
    bim_dict = BIMService.get_or_create_bim_model(conn, project_id)

    return ProjectResponse(
        id=project_id,
        owner_id=current_user["id"],
        name=proj_in.name,
        description=proj_in.description,
        location=proj_in.location,
        plot_width=proj_in.plot_width,
        plot_length=proj_in.plot_length,
        plot_area=plot_area,
        status="model_generated",
        created_at=now_str,
        updated_at=now_str,
        has_bim=True,
        estimated_cost=None
    )

@router.get("", response_model=List[ProjectResponse])
def list_projects(
    current_user: dict = Depends(get_current_user),
    conn: sqlite3.Connection = Depends(get_db)
):
    cursor = conn.cursor()
    cursor.execute(
        """SELECT p.*, 
                  (CASE WHEN b.id IS NOT NULL THEN 1 ELSE 0 END) as has_bim,
                  e.total_rule_based_cost as est_cost
           FROM projects p
           LEFT JOIN bim_models b ON p.id = b.project_id
           LEFT JOIN project_estimates e ON p.id = e.project_id
           WHERE p.owner_id = ?
           ORDER BY p.updated_at DESC""",
        (current_user["id"],)
    )
    rows = cursor.fetchall()

    results = []
    for r in rows:
        results.append(ProjectResponse(
            id=r["id"],
            owner_id=r["owner_id"],
            name=r["name"],
            description=r["description"],
            location=r["location"],
            plot_width=float(r["plot_width"]),
            plot_length=float(r["plot_length"]),
            plot_area=float(r["plot_area"]),
            status=r["status"],
            created_at=str(r["created_at"]),
            updated_at=str(r["updated_at"]),
            has_bim=bool(r["has_bim"]),
            estimated_cost=float(r["est_cost"]) if r["est_cost"] is not None else None
        ))
    return results

@router.get("/{project_id}", response_model=ProjectResponse)
def get_project(
    project_id: str,
    current_user: dict = Depends(get_current_user),
    conn: sqlite3.Connection = Depends(get_db)
):
    cursor = conn.cursor()
    cursor.execute(
        """SELECT p.*, 
                  (CASE WHEN b.id IS NOT NULL THEN 1 ELSE 0 END) as has_bim,
                  e.total_rule_based_cost as est_cost
           FROM projects p
           LEFT JOIN bim_models b ON p.id = b.project_id
           LEFT JOIN project_estimates e ON p.id = e.project_id
           WHERE p.id = ? AND p.owner_id = ?""",
        (project_id, current_user["id"])
    )
    r = cursor.fetchone()
    if not r:
        raise HTTPException(status_code=404, detail="Project not found or unauthorized.")

    return ProjectResponse(
        id=r["id"],
        owner_id=r["owner_id"],
        name=r["name"],
        description=r["description"],
        location=r["location"],
        plot_width=float(r["plot_width"]),
        plot_length=float(r["plot_length"]),
        plot_area=float(r["plot_area"]),
        status=r["status"],
        created_at=str(r["created_at"]),
        updated_at=str(r["updated_at"]),
        has_bim=bool(r["has_bim"]),
        estimated_cost=float(r["est_cost"]) if r["est_cost"] is not None else None
    )

@router.put("/{project_id}", response_model=ProjectResponse)
def update_project(
    project_id: str,
    proj_in: ProjectUpdate,
    current_user: dict = Depends(get_current_user),
    conn: sqlite3.Connection = Depends(get_db)
):
    cursor = conn.cursor()
    cursor.execute("SELECT * FROM projects WHERE id = ? AND owner_id = ?", (project_id, current_user["id"]))
    existing = cursor.fetchone()
    if not existing:
        raise HTTPException(status_code=404, detail="Project not found or unauthorized.")

    name = proj_in.name if proj_in.name is not None else existing["name"]
    description = proj_in.description if proj_in.description is not None else existing["description"]
    location = proj_in.location if proj_in.location is not None else existing["location"]
    plot_width = proj_in.plot_width if proj_in.plot_width is not None else float(existing["plot_width"])
    plot_length = proj_in.plot_length if proj_in.plot_length is not None else float(existing["plot_length"])
    plot_area = round(plot_width * plot_length, 2)
    now_str = datetime.now(timezone.utc).strftime("%Y-%m-%d %H:%M:%S")

    cursor.execute(
        """UPDATE projects 
           SET name = ?, description = ?, location = ?, plot_width = ?, plot_length = ?, plot_area = ?, updated_at = ?
           WHERE id = ?""",
        (name, description, location, plot_width, plot_length, plot_area, now_str, project_id)
    )
    conn.commit()

    return get_project(project_id, current_user, conn)

@router.delete("/{project_id}", status_code=status.HTTP_204_NO_CONTENT)
def delete_project(
    project_id: str,
    current_user: dict = Depends(get_current_user),
    conn: sqlite3.Connection = Depends(get_db)
):
    cursor = conn.cursor()
    cursor.execute("SELECT id FROM projects WHERE id = ? AND owner_id = ?", (project_id, current_user["id"]))
    if not cursor.fetchone():
        raise HTTPException(status_code=404, detail="Project not found or unauthorized.")

    cursor.execute("DELETE FROM projects WHERE id = ?", (project_id,))
    conn.commit()
    return None
