import json
import sqlite3
from fastapi import APIRouter, Depends, HTTPException
from backend.app.database.database import get_db
from backend.app.schemas.common import AIPredictResponse, ModelComparisonResponse
from backend.app.services.ai_service import AIService
from backend.app.services.cost_service import CostService
from backend.app.services.bim_service import BIMService
from backend.app.api.deps import get_current_user

router = APIRouter(tags=["AI / Machine Learning Cost Prediction"])

@router.post("/projects/{project_id}/ai-predict", response_model=AIPredictResponse)
def predict_cost(
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
    pred_response = AIService.predict_project_cost(dict(project), bim_data, cost_est.total_rule_based_cost)

    # Save to project_estimates
    cursor.execute(
        """UPDATE project_estimates 
           SET ml_predicted_cost = ?, ml_confidence_lower = ?, ml_confidence_upper = ?
           WHERE project_id = ?""",
        (pred_response.predicted_cost, pred_response.confidence_lower, pred_response.confidence_upper, project_id)
    )
    conn.commit()

    return pred_response

@router.get("/ai/models-comparison", response_model=ModelComparisonResponse)
def get_models_comparison():
    return AIService.get_models_comparison()
