import json
import uuid
from datetime import datetime, timezone
import sqlite3
from fastapi import APIRouter, Depends, HTTPException, status
from backend.app.database.database import get_db
from backend.app.schemas.common import BIMHouseConfig, BIMModelResponse
from backend.app.services.bim_service import BIMService
from backend.app.services.cost_service import CostService
from backend.app.api.deps import get_current_user

router = APIRouter(prefix="/projects", tags=["BIM Building Model"])

@router.post("/{project_id}/generate-house", response_model=BIMModelResponse)
def generate_house(
    project_id: str,
    config: BIMHouseConfig,
    current_user: dict = Depends(get_current_user),
    conn: sqlite3.Connection = Depends(get_db)
):
    cursor = conn.cursor()
    cursor.execute("SELECT * FROM projects WHERE id = ? AND owner_id = ?", (project_id, current_user["id"]))
    project = cursor.fetchone()
    if not project:
        raise HTTPException(status_code=404, detail="Project not found or unauthorized.")

    # 1. Generate BIM building model
    bim_dict = BIMService.generate_bim_model(project_id, config)
    model_json = json.dumps(bim_dict)
    now_str = datetime.now(timezone.utc).strftime("%Y-%m-%d %H:%M:%S")

    # 2. Persist BIM model in database
    cursor.execute("SELECT id FROM bim_models WHERE project_id = ?", (project_id,))
    existing_model = cursor.fetchone()

    if existing_model:
        cursor.execute(
            """UPDATE bim_models 
               SET floors_count = ?, floor_height = ?, wall_thickness = ?, builtup_area = ?, model_data = ?, updated_at = ?
               WHERE project_id = ?""",
            (config.floors_count, config.floor_height, config.wall_thickness, bim_dict["builtup_area"], model_json, now_str, project_id)
        )
    else:
        bim_id = str(uuid.uuid4())
        cursor.execute(
            """INSERT INTO bim_models 
               (id, project_id, floors_count, floor_height, wall_thickness, builtup_area, model_data, created_at, updated_at)
               VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)""",
            (bim_id, project_id, config.floors_count, config.floor_height, config.wall_thickness, bim_dict["builtup_area"], model_json, now_str, now_str)
        )

    # 3. Synchronize preliminary cost estimate in project_estimates
    cost_est = CostService.calculate_cost_estimate(conn, bim_dict, project["location"], config.quality)
    cursor.execute("SELECT id FROM project_estimates WHERE project_id = ?", (project_id,))
    existing_est = cursor.fetchone()

    if existing_est:
        cursor.execute(
            """UPDATE project_estimates
               SET material_cost = ?, labour_cost = ?, finishing_cost = ?, services_cost = ?, 
                   contingency_cost = ?, total_rule_based_cost = ?, updated_at = ?
               WHERE project_id = ?""",
            (cost_est.material_cost, cost_est.labour_cost, cost_est.finishing_cost, cost_est.services_cost,
             cost_est.contingency_cost, cost_est.total_rule_based_cost, now_str, project_id)
        )
    else:
        est_id = str(uuid.uuid4())
        cursor.execute(
            """INSERT INTO project_estimates 
               (id, project_id, material_cost, labour_cost, finishing_cost, services_cost, contingency_cost, total_rule_based_cost, updated_at)
               VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)""",
            (est_id, project_id, cost_est.material_cost, cost_est.labour_cost, cost_est.finishing_cost, cost_est.services_cost,
             cost_est.contingency_cost, cost_est.total_rule_based_cost, now_str)
        )

    # Update project status
    cursor.execute("UPDATE projects SET status = 'model_generated', updated_at = ? WHERE id = ?", (now_str, project_id))
    conn.commit()

    return BIMModelResponse(
        project_id=project_id,
        floors_count=config.floors_count,
        floor_height=config.floor_height,
        wall_thickness=config.wall_thickness,
        builtup_area=bim_dict["builtup_area"],
        ground_coverage_area=bim_dict["ground_coverage_area"],
        total_elements_count=bim_dict["total_elements_count"],
        elements=bim_dict["elements"],
        raw_config=config.model_dump(),
        updated_at=now_str
    )

@router.get("/{project_id}/bim", response_model=BIMModelResponse)
def get_bim_model(
    project_id: str,
    current_user: dict = Depends(get_current_user),
    conn: sqlite3.Connection = Depends(get_db)
):
    cursor = conn.cursor()
    cursor.execute("SELECT id FROM projects WHERE id = ? AND owner_id = ?", (project_id, current_user["id"]))
    if not cursor.fetchone():
        raise HTTPException(status_code=404, detail="Project not found or unauthorized.")

    cursor.execute("SELECT * FROM bim_models WHERE project_id = ?", (project_id,))
    row = cursor.fetchone()
    if not row:
        # Automatically generate default BIM house model for this project
        default_cfg = BIMService.get_default_config()
        return generate_house(project_id, default_cfg, current_user, conn)

    data = json.loads(row["model_data"])
    return BIMModelResponse(
        project_id=project_id,
        floors_count=int(row["floors_count"]),
        floor_height=float(row["floor_height"]),
        wall_thickness=float(row["wall_thickness"]),
        builtup_area=float(row["builtup_area"]),
        ground_coverage_area=float(data.get("ground_coverage_area", 0.0)),
        total_elements_count=int(data.get("total_elements_count", len(data.get("elements", [])))),
        elements=data.get("elements", []),
        raw_config=data.get("raw_config", {}),
        updated_at=str(row["updated_at"])
    )
