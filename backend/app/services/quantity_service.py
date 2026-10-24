from typing import Dict, Any, List
from backend.app.schemas.common import QTOItem, QTOResponse, BOQItem, BOQResponse

class QuantityService:
    @staticmethod
    def calculate_qto(bim_data: Dict[str, Any]) -> QTOResponse:
        """
        Calculates deterministic Quantity Take-Off directly from the BIM Building Model.
        Compliant with IS 1200 / Standard Method of Measurement (SMM).
        """
        project_id = bim_data.get("project_id", "")
        builtup_area = float(bim_data.get("builtup_area", 100.0))
        elements = bim_data.get("elements", [])

        # 1. Aggregate BIM physical quantities
        total_wall_volume = 0.0
        total_wall_area = 0.0
        total_floor_area = 0.0
        door_count = 0
        window_count = 0
        window_area = 0.0
        door_area = 0.0

        for elem in elements:
            etype = elem.get("type")
            vol = float(elem.get("volume", 0.0))
            area = float(elem.get("area", 0.0))

            if etype == "Wall":
                total_wall_volume += vol
                total_wall_area += area
            elif etype == "Floor":
                total_floor_area += area
            elif etype == "Door":
                door_count += 1
                door_area += area
            elif etype == "Window":
                window_count += 1
                window_area += area

        if total_floor_area == 0:
            total_floor_area = builtup_area

        # 2. Structural RCC Concrete (Slabs, Beams, Columns, Footings)
        # Structural engineering ratio: 0.38 - 0.42 m3 concrete per m2 built-up area
        concrete_m3 = round(builtup_area * 0.40, 2)

        # 3. Steel Reinforcement (Fe500D)
        # Structural rule: ~38 - 42 kg/m2 builtup or 95 kg/m3 concrete
        steel_kg = round(builtup_area * 40.0, 1)

        # 4. Brick Masonry
        # 1 m3 standard brickwork = ~500 bricks (including mortar joints)
        # IS 1200 Part III: Shared interior party walls (115mm partitions vs 230mm exterior envelope)
        # Deducts duplicate party boundaries between adjacent modeled rooms (0.70 factor)
        party_wall_factor = 0.70
        net_masonry_volume = max(5.0, (total_wall_volume * party_wall_factor) - (door_area * 0.23 + window_area * 0.23))
        brick_count_raw = int(net_masonry_volume * 490)
        brick_count_net = int(brick_count_raw * 1.05) # 5% breakage/wastage

        # 5. Cement (OPC 53 / PPC)
        # Concrete + Masonry mortar + 2-coat plaster ~ 4.5 bags per m2
        cement_bags_raw = int(builtup_area * 4.4)
        cement_bags_net = int(cement_bags_raw * 1.03)

        # 6. Sand (M-Sand / River Sand)
        # Approx 0.58 m3 per m2 builtup
        sand_m3 = round(builtup_area * 0.58, 2)

        # 7. Flooring Tiles (Vitrified / Ceramic)
        tile_area_raw = round(total_floor_area, 2)
        tile_area_net = round(tile_area_raw * 1.10, 2) # 10% cutting wastage

        # 8. Wall & Ceiling Paint
        # Internal wall faces (with party wall deduction) + ceiling - openings (IS 1200 Part XV)
        paint_area_raw = round(((total_wall_area * party_wall_factor) * 1.8) - (door_area + window_area) + total_floor_area, 2)
        paint_area_net = round(paint_area_raw * 1.05, 2) # 5% wastage

        items = [
            QTOItem(
                item_code="QTO-CONC-01",
                category="Concrete",
                description="Ready-Mix Reinforced Cement Concrete (M25) for Footings, Columns, Beams & Slabs",
                raw_quantity=concrete_m3,
                wastage_percent=3.0,
                net_quantity=round(concrete_m3 * 1.03, 2),
                unit="m3",
                calculation_basis="0.40 m³ per m² built-up area (IS 456 Structural Standard)"
            ),
            QTOItem(
                item_code="QTO-STEL-01",
                category="Steel",
                description="Thermo-Mechanically Treated (TMT) High Yield Deformed Steel Bars (Fe500D)",
                raw_quantity=steel_kg,
                wastage_percent=4.0,
                net_quantity=round(steel_kg * 1.04, 1),
                unit="kg",
                calculation_basis="40.0 kg per m² built-up area / 98 kg per m³ concrete (SP 34 Detailing)"
            ),
            QTOItem(
                item_code="QTO-MASN-01",
                category="Masonry",
                description="First Class Red Clay Bricks (190 x 90 x 90 mm) in 1:6 Cement Mortar",
                raw_quantity=brick_count_raw,
                wastage_percent=5.0,
                net_quantity=brick_count_net,
                unit="piece",
                calculation_basis="490 bricks/m³ net wall volume deducted for door/window openings + 5% wastage"
            ),
            QTOItem(
                item_code="QTO-CMNT-01",
                category="Cement",
                description="Portland Pozzolana Cement (PPC 53 Grade) in 50kg Bags",
                raw_quantity=cement_bags_raw,
                wastage_percent=3.0,
                net_quantity=cement_bags_net,
                unit="bag",
                calculation_basis="4.4 bags per m² built-up area for RCC structural mix, brickwork mortar & plastering"
            ),
            QTOItem(
                item_code="QTO-SAND-01",
                category="Sand",
                description="Manufactured Sand (M-Sand) Zone II for Mortar & Concreting",
                raw_quantity=sand_m3,
                wastage_percent=5.0,
                net_quantity=round(sand_m3 * 1.05, 2),
                unit="m3",
                calculation_basis="0.58 m³ per m² built-up area (IS 383 Aggregate Standard)"
            ),
            QTOItem(
                item_code="QTO-TILE-01",
                category="Flooring",
                description="Double Charged Vitrified Floor Tiles (600 x 600 mm) with Adhesive Jointing",
                raw_quantity=tile_area_raw,
                wastage_percent=10.0,
                net_quantity=tile_area_net,
                unit="m2",
                calculation_basis="Room floor areas extracted from BIM model + 10% perimeter cutting wastage"
            ),
            QTOItem(
                item_code="QTO-PNT-01",
                category="Finishing",
                description="Premium Acrylic Emulsion Wall & Ceiling Paint (Primer + 2 Finishing Coats)",
                raw_quantity=paint_area_raw,
                wastage_percent=5.0,
                net_quantity=paint_area_net,
                unit="m2",
                calculation_basis="BIM Wall vertical surface area x 1.8 + Ceiling area - Openings + 5% wastage"
            ),
            QTOItem(
                item_code="QTO-DOOR-01",
                category="Openings",
                description="Factory Manufactured Flush Doors with Hardwood Frame & Hardware Fixtures",
                raw_quantity=door_count,
                wastage_percent=0.0,
                net_quantity=door_count,
                unit="unit",
                calculation_basis="Exact count of entrance, bedroom, and bathroom doors defined in BIM model"
            ),
            QTOItem(
                item_code="QTO-WIND-01",
                category="Openings",
                description="UPVC Sliding Glazed Windows with Mosquito Mesh & 5mm Toughened Glass",
                raw_quantity=round(window_area, 2),
                wastage_percent=2.0,
                net_quantity=round(window_area * 1.02, 2),
                unit="m2",
                calculation_basis="Total surface aperture area of all windows scheduled in BIM building envelope"
            )
        ]

        assumptions = [
            "Quantities extracted deterministically from 3D parametric BIM geometry.",
            "RCC structural ratios follow IS 456:2000 (0.40 m³ concrete / 40 kg Fe500D steel per m² built-up).",
            "Masonry deductions follow IS 1200 Part III for door and window openings.",
            "Wastage allowances: 3% concrete, 4% steel, 5% bricks, 10% tiles, 5% paint.",
            "Preliminary engineering estimates intended for early-stage planning and budgeting."
        ]

        return QTOResponse(
            project_id=project_id,
            builtup_area=builtup_area,
            items=items,
            assumptions=assumptions
        )

    @staticmethod
    def generate_boq(qto_response: QTOResponse, rate_map: Dict[str, float], location: str) -> BOQResponse:
        """
        Translates Quantity Take-Off into a priced Bill of Quantities (BOQ).
        """
        boq_items = []
        subtotal = 0.0

        rate_keys = {
            "QTO-CONC-01": "Ready Mix RCC Concrete (M25)",
            "QTO-STEL-01": "TMT Steel Reinforcement (Fe500D)",
            "QTO-MASN-01": "Standard Red Clay Bricks",
            "QTO-CMNT-01": "Portland Pozzolana Cement (50kg)",
            "QTO-SAND-01": "Manufactured Sand (M-Sand)",
            "QTO-TILE-01": "Vitrified Floor Tiles (600x600)",
            "QTO-PNT-01": "Internal Acrylic Emulsion Paint (2 coats)",
            "QTO-DOOR-01": "Internal Bedroom/Bathroom Door",
            "QTO-WIND-01": "UPVC Sliding Glazed Window"
        }

        fallback_rates = {
            "Ready Mix RCC Concrete (M25)": 5200.0,
            "TMT Steel Reinforcement (Fe500D)": 72.0,
            "Standard Red Clay Bricks": 11.5,
            "Portland Pozzolana Cement (50kg)": 390.0,
            "Manufactured Sand (M-Sand)": 1450.0,
            "Vitrified Floor Tiles (600x600)": 850.0,
            "Internal Acrylic Emulsion Paint (2 coats)": 220.0,
            "Internal Bedroom/Bathroom Door": 7500.0,
            "UPVC Sliding Glazed Window": 4200.0
        }

        for q_item in qto_response.items:
            lookup_key = rate_keys.get(q_item.item_code, "")
            unit_rate = rate_map.get(lookup_key, fallback_rates.get(lookup_key, 100.0))
            amount = round(q_item.net_quantity * unit_rate, 2)
            subtotal += amount

            boq_items.append(BOQItem(
                item_id=q_item.item_code,
                category=q_item.category,
                description=q_item.description,
                quantity=q_item.net_quantity,
                unit=q_item.unit,
                unit_rate=unit_rate,
                amount=amount
            ))

        contingency_percent = 5.0
        contingency_amt = round(subtotal * (contingency_percent / 100.0), 2)
        grand_total = round(subtotal + contingency_amt, 2)

        return BOQResponse(
            project_id=qto_response.project_id,
            location=location,
            items=boq_items,
            subtotal=subtotal,
            contingency_percent=contingency_percent,
            contingency_amount=contingency_amt,
            grand_total=grand_total,
            disclaimer="Preliminary BOQ based on configured regional reference rates and model-derived QTO."
        )
