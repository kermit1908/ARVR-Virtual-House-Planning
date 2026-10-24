import uuid
import json
from datetime import datetime, timezone
import sqlite3
from typing import List, Dict, Any, Tuple
from backend.app.schemas.common import BIMHouseConfig, BIMElement, BIMModelResponse

class BIMService:
    @staticmethod
    def generate_bim_model(project_id: str, config: BIMHouseConfig) -> Dict[str, Any]:
        """
        Generates a BIM-oriented parametric building model.
        Every architectural element possesses unique identifier, exact 3D geometry coordinates,
        spatial relationships, materials, areas, and volumes.
        """
        elements: List[Dict[str, Any]] = []
        total_builtup_area = 0.0
        ground_coverage = 0.0

        wall_th = config.wall_thickness
        floor_h = config.floor_height

        for floor in config.floors:
            level = floor.level
            base_y = level * floor_h
            current_x = 0.0
            max_floor_width = 0.0
            max_floor_length = 0.0
            floor_area_sum = 0.0

            # Arrange rooms in a functional layout
            # 2 rooms per row layout
            row_y_offset = 0.0
            col_x_offset = 0.0
            max_row_length = 0.0

            for r_idx, room in enumerate(floor.rooms):
                r_w = room.width
                r_l = room.length
                r_area = r_w * r_l
                floor_area_sum += r_area

                # Layout calculation
                pos_x = col_x_offset
                pos_z = row_y_offset

                # 1. Room Floor Element
                floor_elem_id = f"floor_{level}_{r_idx}_{uuid.uuid4().hex[:6]}"
                elements.append({
                    "id": floor_elem_id,
                    "type": "Floor",
                    "floor_level": level,
                    "room_name": room.name,
                    "length": r_l,
                    "width": r_w,
                    "height": 0.15, # Slab thickness
                    "thickness": 0.15,
                    "area": round(r_area, 2),
                    "volume": round(r_area * 0.15, 3),
                    "material": room.floor_material or "Tiles",
                    "position": [round(pos_x + r_w / 2, 2), round(base_y + 0.075, 2), round(pos_z + r_l / 2, 2)],
                    "rotation": [0.0, 0.0, 0.0]
                })

                # 2. Room Ceiling Element
                ceil_elem_id = f"ceil_{level}_{r_idx}_{uuid.uuid4().hex[:6]}"
                elements.append({
                    "id": ceil_elem_id,
                    "type": "Ceiling",
                    "floor_level": level,
                    "room_name": room.name,
                    "length": r_l,
                    "width": r_w,
                    "height": 0.15,
                    "thickness": 0.15,
                    "area": round(r_area, 2),
                    "volume": round(r_area * 0.15, 3),
                    "material": "Plaster & Paint",
                    "position": [round(pos_x + r_w / 2, 2), round(base_y + floor_h - 0.075, 2), round(pos_z + r_l / 2, 2)],
                    "rotation": [0.0, 0.0, 0.0]
                })

                # 3. Four Walls per Room (North, South, East, West)
                wall_specs = [
                    ("Wall_N", r_w, wall_th, [pos_x + r_w / 2, base_y + floor_h / 2, pos_z + r_l], [0, 0, 0]),
                    ("Wall_S", r_w, wall_th, [pos_x + r_w / 2, base_y + floor_h / 2, pos_z], [0, 0, 0]),
                    ("Wall_E", wall_th, r_l, [pos_x + r_w, base_y + floor_h / 2, pos_z + r_l / 2], [0, 0, 0]),
                    ("Wall_W", wall_th, r_l, [pos_x, base_y + floor_h / 2, pos_z + r_l / 2], [0, 0, 0])
                ]

                for w_name, w_len, w_th, w_pos, w_rot in wall_specs:
                    w_actual_len = w_len if w_len > wall_th else r_l
                    w_area = w_actual_len * floor_h
                    w_vol = w_area * wall_th

                    elements.append({
                        "id": f"wall_{level}_{r_idx}_{w_name}_{uuid.uuid4().hex[:6]}",
                        "type": "Wall",
                        "floor_level": level,
                        "room_name": room.name,
                        "length": round(w_actual_len, 2),
                        "width": round(w_th, 2),
                        "height": round(floor_h, 2),
                        "thickness": round(wall_th, 2),
                        "area": round(w_area, 2),
                        "volume": round(w_vol, 3),
                        "material": "Brick Masonry",
                        "position": [round(w_pos[0], 2), round(w_pos[1], 2), round(w_pos[2], 2)],
                        "rotation": w_rot
                    })

                # 4. Standard Door for each room
                elements.append({
                    "id": f"door_{level}_{r_idx}_{uuid.uuid4().hex[:6]}",
                    "type": "Door",
                    "floor_level": level,
                    "room_name": room.name,
                    "length": 1.0,
                    "width": wall_th,
                    "height": 2.1,
                    "thickness": 0.05,
                    "area": 2.1,
                    "volume": round(2.1 * 0.05, 3),
                    "material": "Wood Flush Door",
                    "position": [round(pos_x + 0.6, 2), round(base_y + 1.05, 2), round(pos_z, 2)],
                    "rotation": [0, 0, 0]
                })

                # 5. Window for exterior wall
                elements.append({
                    "id": f"window_{level}_{r_idx}_{uuid.uuid4().hex[:6]}",
                    "type": "Window",
                    "floor_level": level,
                    "room_name": room.name,
                    "length": 1.5,
                    "width": wall_th,
                    "height": 1.2,
                    "thickness": 0.06,
                    "area": 1.8,
                    "volume": round(1.8 * 0.06, 3),
                    "material": "UPVC Glazed",
                    "position": [round(pos_x + r_w / 2, 2), round(base_y + 1.6, 2), round(pos_z + r_l, 2)],
                    "rotation": [0, 0, 0]
                })

                # Advance layout offset (2 rooms per row)
                col_x_offset += r_w + wall_th
                max_row_length = max(max_row_length, r_l)
                if (r_idx + 1) % 2 == 0:
                    row_y_offset += max_row_length + wall_th
                    col_x_offset = 0.0
                    max_row_length = 0.0

            total_builtup_area += floor_area_sum
            if level == 0:
                ground_coverage = floor_area_sum

        # 6. Roof Slab Element at the top
        roof_level = len(config.floors)
        roof_y = roof_level * floor_h
        roof_area = ground_coverage if ground_coverage > 0 else total_builtup_area / max(1, len(config.floors))
        elements.append({
            "id": f"roof_slab_{uuid.uuid4().hex[:6]}",
            "type": "Roof",
            "floor_level": roof_level,
            "room_name": "Terrace Roof",
            "length": round((roof_area ** 0.5), 2),
            "width": round((roof_area ** 0.5), 2),
            "height": 0.15,
            "thickness": 0.15,
            "area": round(roof_area, 2),
            "volume": round(roof_area * 0.15, 3),
            "material": "RCC Slab + Waterproofing",
            "position": [round((roof_area ** 0.5) / 2, 2), round(roof_y + 0.075, 2), round((roof_area ** 0.5) / 2, 2)],
            "rotation": [0, 0, 0]
        })

        return {
            "project_id": project_id,
            "floors_count": config.floors_count,
            "floor_height": config.floor_height,
            "wall_thickness": config.wall_thickness,
            "builtup_area": round(total_builtup_area, 2),
            "ground_coverage_area": round(ground_coverage, 2),
            "total_elements_count": len(elements),
            "elements": elements,
            "raw_config": config.model_dump(),
            "updated_at": "now"
        }

    @staticmethod
    def get_or_create_bim_model(conn: sqlite3.Connection, project_id: str) -> Dict[str, Any]:
        cursor = conn.cursor()
        cursor.execute("SELECT model_data FROM bim_models WHERE project_id = ?", (project_id,))
        row = cursor.fetchone()
        if row:
            return json.loads(row["model_data"])

        default_cfg = BIMService.get_default_config()
        bim_dict = BIMService.generate_bim_model(project_id, default_cfg)
        model_json = json.dumps(bim_dict)
        now_str = datetime.now(timezone.utc).strftime("%Y-%m-%d %H:%M:%S")
        bim_id = str(uuid.uuid4())
        cursor.execute(
            """INSERT INTO bim_models 
               (id, project_id, floors_count, floor_height, wall_thickness, builtup_area, model_data, created_at, updated_at)
               VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)""",
            (bim_id, project_id, default_cfg.floors_count, default_cfg.floor_height, default_cfg.wall_thickness, bim_dict["builtup_area"], model_json, now_str, now_str)
        )
        cursor.execute("UPDATE projects SET status = 'model_generated', updated_at = ? WHERE id = ?", (now_str, project_id))
        conn.commit()
        return bim_dict

    @staticmethod
    def get_default_config() -> BIMHouseConfig:
        return BIMHouseConfig(
            floors_count=1,
            floor_height=3.1,
            wall_thickness=0.23,
            quality="Standard",
            floors=[
                {
                    "level": 0,
                    "height": 3.1,
                    "rooms": [
                        {"name": "Living Room", "type": "LivingRoom", "width": 5.5, "length": 4.5, "floor_level": 0, "floor_material": "Tiles"},
                        {"name": "Kitchen & Dining", "type": "Kitchen", "width": 4.0, "length": 3.5, "floor_level": 0, "floor_material": "Tiles"},
                        {"name": "Master Bedroom", "type": "Bedroom", "width": 4.5, "length": 4.0, "floor_level": 0, "floor_material": "Tiles"},
                        {"name": "Attached Bathroom", "type": "Bathroom", "width": 2.2, "length": 1.8, "floor_level": 0, "floor_material": "Tiles"}
                    ]
                }
            ]
        )
        return BIMHouseConfig(
            floors_count=1,
            floor_height=3.1,
            wall_thickness=0.23,
            quality="Standard",
            floors=[
                {
                    "level": 0,
                    "height": 3.1,
                    "rooms": [
                        {"name": "Living Room", "type": "LivingRoom", "width": 5.5, "length": 4.5, "floor_level": 0, "floor_material": "Tiles"},
                        {"name": "Kitchen & Dining", "type": "Kitchen", "width": 4.0, "length": 3.5, "floor_level": 0, "floor_material": "Tiles"},
                        {"name": "Master Bedroom", "type": "Bedroom", "width": 4.5, "length": 4.0, "floor_level": 0, "floor_material": "Tiles"},
                        {"name": "Attached Bathroom", "type": "Bathroom", "width": 2.2, "length": 1.8, "floor_level": 0, "floor_material": "Tiles"}
                    ]
                }
            ]
        )
