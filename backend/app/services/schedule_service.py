from datetime import datetime, timedelta
from typing import Dict, Any, List
from backend.app.schemas.common import TaskItem, ScheduleResponse

class ScheduleService:
    @staticmethod
    def generate_schedule(bim_data: Dict[str, Any], start_date_str: str = None) -> ScheduleResponse:
        """
        Generates a 4D construction schedule using Critical Path Method (CPM)
        derived from built-up area, floor count, and structural parameters.
        """
        project_id = bim_data.get("project_id", "")
        builtup_area = float(bim_data.get("builtup_area", 120.0))
        floors = int(bim_data.get("floors_count", 1))

        if not start_date_str:
            start_date = datetime.now()
        else:
            try:
                start_date = datetime.strptime(start_date_str, "%Y-%m-%d")
            except ValueError:
                start_date = datetime.now()

        # Dynamic duration scaling based on project scale
        scale_factor = max(0.8, min(2.5, (builtup_area / 120.0) * (0.8 + 0.2 * floors)))

        # Define 11 construction milestones with realistic dependencies
        tasks_definition = [
            ("TSK-01", "Site Preparation & Soil Investigation", "Substructure", int(round(7 * scale_factor)), [], "Earthwork Crew"),
            ("TSK-02", "Earthwork Excavation for Footings", "Substructure", int(round(10 * scale_factor)), ["TSK-01"], "Excavation Crew"),
            ("TSK-03", "Foundation PCC & RCC Footings", "Substructure", int(round(16 * scale_factor)), ["TSK-02"], "RCC Foundation Team"),
            ("TSK-04", "RCC Frame (Columns, Beams & Slabs)", "Superstructure", int(round(28 * scale_factor * floors)), ["TSK-03"], "Structural Shuttering & Bar Bending Crew"),
            ("TSK-05", "Brick / Block Masonry Envelope", "Superstructure", int(round(22 * scale_factor * floors)), ["TSK-04"], "Masonry Crew"),
            ("TSK-06", "Concealed Electrical Conduit & Boxes", "Services", int(round(12 * scale_factor)), ["TSK-05"], "Electrical Subcontractor"),
            ("TSK-07", "Plumbing & Drainage Pipe Routing", "Services", int(round(14 * scale_factor)), ["TSK-05"], "Plumbing Subcontractor"),
            ("TSK-08", "Internal & External Cement Plastering", "Finishing", int(round(18 * scale_factor)), ["TSK-06", "TSK-07"], "Plastering Crew"),
            ("TSK-09", "Vitrified Tile Flooring & Skirting", "Finishing", int(round(15 * scale_factor)), ["TSK-08"], "Flooring Crew"),
            ("TSK-10", "Wall Putty, Primer & Emulsion Paint", "Finishing", int(round(14 * scale_factor)), ["TSK-09"], "Painting Crew"),
            ("TSK-11", "Door / Window Fixtures & Clean Handover", "Commissioning", int(round(8 * scale_factor)), ["TSK-10"], "Carpentry & Quality Audit")
        ]

        task_items: List[TaskItem] = []
        task_end_days: Dict[str, int] = {}
        total_days = 0

        for tid, name, stage, duration, deps, crew in tasks_definition:
            start_day = 0
            if deps:
                start_day = max(task_end_days.get(dep, 0) for dep in deps)
            
            end_day = start_day + duration
            task_end_days[tid] = end_day
            total_days = max(total_days, end_day)

            task_items.append(TaskItem(
                id=tid,
                name=name,
                stage=stage,
                duration_days=duration,
                start_day=start_day,
                end_day=end_day,
                dependencies=deps,
                progress=0,
                assigned_crew=crew
            ))

        end_date = start_date + timedelta(days=total_days)
        critical_path = ["TSK-01", "TSK-02", "TSK-03", "TSK-04", "TSK-05", "TSK-07", "TSK-08", "TSK-09", "TSK-10", "TSK-11"]

        return ScheduleResponse(
            project_id=project_id,
            total_duration_days=total_days,
            start_date=start_date.strftime("%Y-%m-%d"),
            estimated_completion_date=end_date.strftime("%Y-%m-%d"),
            tasks=task_items,
            critical_path=critical_path
        )
