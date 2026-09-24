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


def get_city_data(
    country: str,
    state: str,
    city: str
):
    data = load_regional_prices()

    try:
        return data[country][state][city]
    except KeyError:
        raise ValueError(
            f"Pricing data not available for {city}, {state}, {country}"
        )


def calculate_cost(
    area_sqft: float,
    flooring: str = "standard",
    wall_material: str = "standard",
    country: str = "India",
    state: str = "Karnataka",
    city: str = "Bengaluru"
):
    city_data = get_city_data(
        country,
        state,
        city
    )

    materials = city_data["materials"]

    flooring_price = materials["tiles"]["price"]

    if flooring.lower() == "standard":
        flooring_price = materials["tiles"]["min_price"]

    if flooring.lower() == "premium":
        flooring_price = materials["tiles"]["max_price"]

    wall_price = materials["aac_block"]["price"]

    if wall_material.lower() == "standard":
        wall_price = materials["aac_block"]["min_price"]

    if wall_material.lower() == "premium":
        wall_price = materials["aac_block"]["max_price"]

    paint_price = materials["paint"]["price"]

    flooring_cost = area_sqft * flooring_price
    wall_cost = area_sqft * wall_price
    paint_cost = area_sqft * paint_price

    total_cost = (
        flooring_cost
        + wall_cost
        + paint_cost
    )

    return {
        "location": {
            "country": country,
            "state": state,
            "city": city
        },
        "currency": city_data["currency"],
        "last_updated": city_data["last_updated"],
        "source": city_data["source"],
        "flooring": {
            "type": flooring,
            "unit_price": flooring_price,
            "unit": "sqft",
            "cost": round(flooring_cost, 2)
        },
        "wall": {
            "type": wall_material,
            "unit_price": wall_price,
            "unit": "sqft",
            "cost": round(wall_cost, 2)
        },
        "paint": {
            "unit_price": paint_price,
            "unit": "sqft",
            "cost": round(paint_cost, 2)
        },
        "total_cost": round(total_cost, 2)
    }