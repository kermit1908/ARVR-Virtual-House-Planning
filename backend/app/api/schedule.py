import json
import sqlite3
from fastapi import APIRouter, Depends, HTTPException
from backend.app.database.database import get_db
from backend.app.schemas.common import ScheduleResponse
from backend.app.services.schedule_service import ScheduleService
from backend.app.services.bim_service import BIMService
from backend.app.api.deps import get_current_user

router = APIRouter(prefix="/projects", tags=["4D Construction Scheduling"])

@router.get("/{project_id}/schedule", response_model=ScheduleResponse)
def get_project_schedule(
    project_id: str,
    start_date: str = None,
    current_user: dict = Depends(get_current_user),
    conn: sqlite3.Connection = Depends(get_db)
):
    cursor = conn.cursor()
    cursor.execute("SELECT * FROM projects WHERE id = ? AND owner_id = ?", (project_id, current_user["id"]))
    project = cursor.fetchone()
    if not project:
        raise HTTPException(status_code=404, detail="Project not found or unauthorized.")

    bim_data = BIMService.get_or_create_bim_model(conn, project_id)
    return ScheduleService.generate_schedule(bim_data, start_date)
