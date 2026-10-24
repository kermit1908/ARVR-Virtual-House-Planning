from datetime import datetime, timezone
from typing import Dict, Any
import sqlite3
from backend.app.schemas.common import ProjectReportResponse, ProjectResponse
from backend.app.services.quantity_service import QuantityService
from backend.app.services.cost_service import CostService
from backend.app.services.ai_service import AIService
from backend.app.services.optimize_service import OptimizeService
from backend.app.services.schedule_service import ScheduleService

class ReportService:
    @staticmethod
    def generate_full_report(
        conn: sqlite3.Connection,
        project_dict: Dict[str, Any],
        bim_data: Dict[str, Any]
    ) -> ProjectReportResponse:
        """
        Assembles all project lifecycle stages into a unified,
        academically rigorous project report document.
        """
        location = project_dict.get("location", "Bengaluru")
        quality = bim_data.get("raw_config", {}).get("quality", "Standard")

        qto = QuantityService.calculate_qto(bim_data)
        rate_map = CostService.get_location_rates(conn, location)
        boq = QuantityService.generate_boq(qto, rate_map, location)
        cost_estimate = CostService.calculate_cost_estimate(conn, bim_data, location, quality)
        ai_prediction = AIService.predict_project_cost(project_dict, bim_data, cost_estimate.total_rule_based_cost)
        optimization = OptimizeService.generate_cost_optimizations(bim_data, cost_estimate.total_rule_based_cost, quality)
        schedule = ScheduleService.generate_schedule(bim_data)

        # Academic evaluation context
        academic_eval = {
            "research_domain": "Building Information Modelling (BIM) + Artificial Intelligence (AI) in Residential Planning",
            "primary_literature": [
                "Yigitbas et al. (2023) - Supporting Construction and Architectural Visualization through BIM and AR/VR",
                "Sidani et al. (2021) - Recent tools and techniques of BIM-Based Augmented Reality",
                "Jafary & Shojaei (2026) - AI and ML in BIM-based construction cost estimation"
            ],
            "methodology": "Single Source of Truth (SSOT) Parametric BIM -> Deterministic QTO -> 5D Cost Model -> Supervised ML Gradient Boosting Ensemble",
            "limitations": [
                "Preliminary engineering calculations intended for conceptual budgeting; does not substitute certified PE structural design.",
                "Material prices reflect configured state PWD DSR schedules and regional averages; subject to dynamic market inflation.",
                "AR/VR functionality depends on browser WebXR API support and device sensor calibration."
            ]
        }

        proj_response = ProjectResponse(
            id=project_dict["id"],
            owner_id=project_dict["owner_id"],
            name=project_dict["name"],
            description=project_dict.get("description", ""),
            location=project_dict["location"],
            plot_width=float(project_dict["plot_width"]),
            plot_length=float(project_dict["plot_length"]),
            plot_area=float(project_dict["plot_area"]),
            status=project_dict.get("status", "draft"),
            created_at=str(project_dict.get("created_at", "")),
            updated_at=str(project_dict.get("updated_at", "")),
            has_bim=True,
            estimated_cost=cost_estimate.total_rule_based_cost
        )

        bim_summary = {
            "floors_count": bim_data.get("floors_count", 1),
            "floor_height": bim_data.get("floor_height", 3.0),
            "wall_thickness": bim_data.get("wall_thickness", 0.23),
            "builtup_area": bim_data.get("builtup_area", 0.0),
            "total_elements_count": bim_data.get("total_elements_count", 0),
            "quality_grade": quality
        }

        return ProjectReportResponse(
            project=proj_response,
            bim_summary=bim_summary,
            qto=qto,
            boq=boq,
            cost_estimate=cost_estimate,
            ai_prediction=ai_prediction,
            optimization=optimization,
            schedule=schedule,
            academic_evaluation=academic_eval,
            generated_at=datetime.now(timezone.utc).strftime("%Y-%m-%d %H:%M:%S UTC")
        )
