import json
import sqlite3
from fastapi import APIRouter, Depends, HTTPException
from backend.app.database.database import get_db
from backend.app.schemas.common import ProjectReportResponse
from backend.app.services.report_service import ReportService
from backend.app.services.bim_service import BIMService
from backend.app.api.deps import get_current_user

router = APIRouter(prefix="/projects", tags=["Project Reporting"])

@router.get("/{project_id}/report", response_model=ProjectReportResponse)
def get_project_report(
    project_id: str,
    current_user: dict = Depends(get_current_user),
    conn: sqlite3.Connection = Depends(get_db)
):
    cursor = conn.cursor()
    cursor.execute("SELECT * FROM projects WHERE id = ? AND owner_id = ?", (project_id, current_user["id"]))
    project = cursor.fetchone()
    if not project:
        raise HTTPException(status_code=404, detail="Project not found or unauthorized.")

    bim_data = BIMService.get_or_create_bim_model(conn, project_id)
    return ReportService.generate_full_report(conn, dict(project), bim_data)
