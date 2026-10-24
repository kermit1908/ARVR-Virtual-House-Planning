import json
import sqlite3
from fastapi import APIRouter, Depends, HTTPException
from backend.app.database.database import get_db
from backend.app.schemas.common import OptimizationResponse
from backend.app.services.optimize_service import OptimizeService
from backend.app.services.cost_service import CostService
from backend.app.services.bim_service import BIMService
from backend.app.api.deps import get_current_user

router = APIRouter(prefix="/projects", tags=["AI Cost Optimization"])

@router.get("/{project_id}/optimize-cost", response_model=OptimizationResponse)
def optimize_project_cost(
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

    cost_est = CostService.calculate_cost_estimate(conn, bim_data, location, quality)
    return OptimizeService.generate_cost_optimizations(bim_data, cost_est.total_rule_based_cost, quality)
