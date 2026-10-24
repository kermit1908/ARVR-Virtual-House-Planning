import json
import sqlite3
from typing import List, Dict, Any
from fastapi import APIRouter, Depends, HTTPException
from backend.app.database.database import get_db
from backend.app.schemas.common import CostEstimateResponse
from backend.app.services.cost_service import CostService
from backend.app.services.bim_service import BIMService
from backend.app.api.deps import get_current_user

router = APIRouter(tags=["5D Cost Estimation"])

@router.get("/projects/{project_id}/cost-estimate", response_model=CostEstimateResponse)
def get_cost_estimate(
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
    quality = bim_data.get("raw_config", {}).get("quality", "Standard")
    location = project["location"]

    return CostService.calculate_cost_estimate(conn, bim_data, location, quality)

@router.get("/rates")
def list_material_rates(
    location: str = "Bengaluru",
    conn: sqlite3.Connection = Depends(get_db)
):
    cursor = conn.cursor()
    cursor.execute(
        "SELECT id, location, category, item_name, unit, unit_rate, currency FROM material_rates WHERE location = ? ORDER BY category, item_name",
        (location,)
    )
    rows = cursor.fetchall()
    return [dict(r) for r in rows]
