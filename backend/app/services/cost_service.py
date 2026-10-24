import sqlite3
from typing import Dict, Any, List
from backend.app.schemas.common import CostEstimateResponse, CostCategoryBreakdown, QTOResponse, BOQResponse
from backend.app.services.quantity_service import QuantityService

class CostService:
    @staticmethod
    def get_location_rates(conn: sqlite3.Connection, location: str) -> Dict[str, float]:
        """Fetches reference rates for the specified city from the database."""
        cursor = conn.cursor()
        cursor.execute("SELECT item_name, unit_rate FROM material_rates WHERE location = ?", (location,))
        rows = cursor.fetchall()

        if not rows:
            # Fallback to Bengaluru if city not found
            cursor.execute("SELECT item_name, unit_rate FROM material_rates WHERE location = 'Bengaluru'")
            rows = cursor.fetchall()

        rate_map = {row["item_name"]: float(row["unit_rate"]) for row in rows}
        return rate_map

    @classmethod
    def calculate_cost_estimate(
        cls,
        conn: sqlite3.Connection,
        bim_data: Dict[str, Any],
        location: str = "Bengaluru",
        quality: str = "Standard"
    ) -> CostEstimateResponse:
        """
        Calculates preliminary 5D BIM construction cost estimate.
        Links structural BIM quantities with location-specific unit price reference tables.
        """
        project_id = bim_data.get("project_id", "")
        builtup_area = float(bim_data.get("builtup_area", 100.0))

        rate_map = cls.get_location_rates(conn, location)
        qto = QuantityService.calculate_qto(bim_data)
        boq = QuantityService.generate_boq(qto, rate_map, location)

        # Quality multiplier
        quality_mults = {"Standard": 1.0, "Premium": 1.25, "Luxury": 1.60}
        q_factor = quality_mults.get(quality, 1.0)

        # Subcategories
        material_base = 0.0
        finishing_base = 0.0

        for b_item in boq.items:
            if b_item.category in ["Flooring", "Finishing", "Openings"]:
                finishing_base += b_item.amount
            else:
                material_base += b_item.amount

        # Apply quality adjustments to finishes and fixtures
        finishing_cost = round(finishing_base * q_factor, 2)
        material_cost = round(material_base * (1.0 + (q_factor - 1.0) * 0.4), 2)

        # Labour: Civil construction labour per m2
        labour_rate = rate_map.get("Civil Construction Labour", 3800.0)
        labour_cost = round(builtup_area * labour_rate * (1.0 + (q_factor - 1.0) * 0.3), 2)

        # Services: MEP (Mechanical, Electrical, Plumbing)
        services_rate = rate_map.get("Electrical & Plumbing Rough-in & Fixtures", 1850.0)
        services_cost = round(builtup_area * services_rate * (1.0 + (q_factor - 1.0) * 0.5), 2)

        subtotal = material_cost + labour_cost + services_cost + finishing_cost
        contingency_cost = round(subtotal * 0.05, 2) # 5% contingency
        total_rule_based_cost = round(subtotal + contingency_cost, 2)

        cost_per_sqm = round(total_rule_based_cost / max(1.0, builtup_area), 2)

        breakdown = [
            CostCategoryBreakdown(category="Structural Materials", amount=material_cost, percentage=round((material_cost / total_rule_based_cost) * 100, 1)),
            CostCategoryBreakdown(category="Civil Labour", amount=labour_cost, percentage=round((labour_cost / total_rule_based_cost) * 100, 1)),
            CostCategoryBreakdown(category="Interior & Architectural Finishing", amount=finishing_cost, percentage=round((finishing_cost / total_rule_based_cost) * 100, 1)),
            CostCategoryBreakdown(category="MEP Services (Electrical & Plumbing)", amount=services_cost, percentage=round((services_cost / total_rule_based_cost) * 100, 1)),
            CostCategoryBreakdown(category="Project Contingency (5%)", amount=contingency_cost, percentage=round((contingency_cost / total_rule_based_cost) * 100, 1))
        ]

        return CostEstimateResponse(
            project_id=project_id,
            location=location,
            quality=quality,
            builtup_area=builtup_area,
            material_cost=material_cost,
            labour_cost=labour_cost,
            services_cost=services_cost,
            finishing_cost=finishing_cost,
            contingency_cost=contingency_cost,
            total_rule_based_cost=total_rule_based_cost,
            cost_per_sqm=cost_per_sqm,
            breakdown=breakdown,
            disclaimer="Preliminary 5D BIM estimate based on configured regional reference rates and model-derived quantities. Not a certified structural contractor quote."
        )
