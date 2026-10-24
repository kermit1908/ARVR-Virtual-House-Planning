import pytest
from backend.app.services.bim_service import BIMService
from backend.app.services.quantity_service import QuantityService
from backend.app.schemas.common import BIMHouseConfig, FloorInput, RoomInput

def test_bim_element_generation():
    config = BIMHouseConfig(
        floors_count=1,
        floor_height=3.0,
        wall_thickness=0.23,
        quality="Standard",
        floors=[
            FloorInput(
                level=0,
                height=3.0,
                rooms=[
                    RoomInput(name="Living Room", type="LivingRoom", width=5.0, length=4.0),
                    RoomInput(name="Master Bedroom", type="Bedroom", width=4.0, length=3.5)
                ]
            )
        ]
    )

    bim = BIMService.generate_bim_model("test_proj_001", config)
    assert bim["project_id"] == "test_proj_001"
    assert bim["builtup_area"] == 5.0 * 4.0 + 4.0 * 3.5 # 20.0 + 14.0 = 34.0 m2
    assert len(bim["elements"]) > 0

    # Verify element structure
    types = {e["type"] for e in bim["elements"]}
    assert "Wall" in types
    assert "Floor" in types
    assert "Ceiling" in types
    assert "Door" in types
    assert "Window" in types

def test_qto_calculations():
    config = BIMHouseConfig(
        floors_count=1,
        floor_height=3.0,
        wall_thickness=0.23,
        quality="Standard",
        floors=[
            FloorInput(
                level=0,
                height=3.0,
                rooms=[
                    RoomInput(name="Hall", type="LivingRoom", width=6.0, length=5.0),
                    RoomInput(name="Kitchen", type="Kitchen", width=3.0, length=3.0)
                ]
            )
        ]
    )
    bim = BIMService.generate_bim_model("test_proj_002", config)
    qto = QuantityService.calculate_qto(bim)

    builtup = 39.0 # 30 + 9
    assert qto.builtup_area == builtup

    q_map = {item.category: item for item in qto.items}
    assert "Concrete" in q_map
    assert "Steel" in q_map
    assert "Masonry" in q_map
    assert "Flooring" in q_map

    # Test IS 456 Structural Concrete ratio (0.40 m3 / m2)
    expected_concrete = builtup * 0.40
    assert abs(q_map["Concrete"].raw_quantity - expected_concrete) < 0.1

    # Test Steel ratio (40 kg / m2)
    expected_steel = builtup * 40.0
    assert abs(q_map["Steel"].raw_quantity - expected_steel) < 0.1

    # Test Tile area wastage (10%)
    assert q_map["Flooring"].wastage_percent == 10.0
    assert q_map["Flooring"].net_quantity > q_map["Flooring"].raw_quantity
