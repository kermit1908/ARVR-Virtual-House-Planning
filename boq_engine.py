import json
import os


def load_regional_prices():
    path = os.path.join(
        os.path.dirname(__file__),
        "data",
        "regional_prices.json"
    )

    with open(path, "r", encoding="utf-8") as file:
        return json.load(file)


def get_city_materials(
    country: str,
    state: str,
    city: str
):
    data = load_regional_prices()

    try:
        return data[country][state][city]["materials"]
    except KeyError:
        raise ValueError(
            f"Pricing data not available for {city}, {state}, {country}"
        )


def calculate_boq(
    area_sqft: float,
    number_of_rooms: int,
    number_of_bathrooms: int,
    material_quality: str = "standard",
    country: str = "India",
    state: str = "Karnataka",
    city: str = "Bengaluru"
):
    materials = get_city_materials(
        country,
        state,
        city
    )

    quality = material_quality.lower()

    cement_quantity = area_sqft * 0.40
    steel_quantity = area_sqft * 3.5
    sand_quantity = area_sqft * 0.45
    tiles_quantity = area_sqft * 1.05
    paint_quantity = area_sqft * 2.5

    doors = number_of_rooms + number_of_bathrooms + 1
    windows = number_of_rooms + 2

    tile_price = materials["tiles"]["price"]

    if quality == "standard":
        tile_price = materials["tiles"]["min_price"]

    if quality == "premium":
        tile_price = materials["tiles"]["max_price"]

    return {
        "location": {
            "country": country,
            "state": state,
            "city": city
        },
        "cement": {
            "quantity": round(cement_quantity, 2),
            "unit": "50kg bag",
            "unit_price": materials["cement_opc_53"]["price"],
            "estimated_cost": round(
                cement_quantity * materials["cement_opc_53"]["price"],
                2
            )
        },
        "steel": {
            "quantity": round(steel_quantity, 2),
            "unit": "kg",
            "unit_price": materials["tmt_fe500d"]["price"],
            "estimated_cost": round(
                steel_quantity * materials["tmt_fe500d"]["price"],
                2
            )
        },
        "sand": {
            "quantity": round(sand_quantity, 2),
            "unit": "cft",
            "unit_price": materials["m_sand"]["price"],
            "estimated_cost": round(
                sand_quantity * materials["m_sand"]["price"],
                2
            )
        },
        "tiles": {
            "quantity": round(tiles_quantity, 2),
            "unit": "sqft",
            "unit_price": tile_price,
            "estimated_cost": round(
                tiles_quantity * tile_price,
                2
            )
        },
        "paint": {
            "quantity": round(paint_quantity, 2),
            "unit": "sqft",
            "unit_price": materials["paint"]["price"],
            "estimated_cost": round(
                paint_quantity * materials["paint"]["price"],
                2
            )
        },
        "doors": {
            "quantity": doors,
            "unit": "piece"
        },
        "windows": {
            "quantity": windows,
            "unit": "piece"
        }
    }