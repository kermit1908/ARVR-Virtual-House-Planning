import json
import sqlite3
from fastapi import APIRouter, Depends, HTTPException
from backend.app.database.database import get_db
from backend.app.schemas.common import QTOResponse, BOQResponse
from backend.app.services.quantity_service import QuantityService
from backend.app.services.cost_service import CostService
from backend.app.services.bim_service import BIMService
from backend.app.api.deps import get_current_user

router = APIRouter(prefix="/projects", tags=["Quantity Take-Off (QTO) & BOQ"])

def _get_project_and_bim(project_id: str, user_id: str, conn: sqlite3.Connection):
    cursor = conn.cursor()
    cursor.execute("SELECT * FROM projects WHERE id = ? AND owner_id = ?", (project_id, user_id))
    project = cursor.fetchone()
    if not project:
        raise HTTPException(status_code=404, detail="Project not found or unauthorized.")

    bim_data = BIMService.get_or_create_bim_model(conn, project_id)
    return dict(project), bim_data

@router.get("/{project_id}/quantities", response_model=QTOResponse)
def get_quantities(
    project_id: str,
    current_user: dict = Depends(get_current_user),
    conn: sqlite3.Connection = Depends(get_db)
):
    _, bim_data = _get_project_and_bim(project_id, current_user["id"], conn)
    return QuantityService.calculate_qto(bim_data)

@router.get("/{project_id}/boq", response_model=BOQResponse)
def get_boq(
    project_id: str,
    current_user: dict = Depends(get_current_user),
    conn: sqlite3.Connection = Depends(get_db)
):
    project, bim_data = _get_project_and_bim(project_id, current_user["id"], conn)
    location = project.get("location", "Bengaluru")
    rate_map = CostService.get_location_rates(conn, location)
    qto = QuantityService.calculate_qto(bim_data)
    return QuantityService.generate_boq(qto, rate_map, location)
