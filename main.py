from fastapi import FastAPI, HTTPException
from cost_engine import calculate_cost, load_regional_prices
from boq_engine import calculate_boq
from schedule_engine import calculate_schedule
from models import HouseDesign

app = FastAPI(
    title="AR/VR House Planning System",
    description="Backend for house visualization, regional pricing, cost estimation, BOQ and construction planning",
    version="1.2.0"
)


@app.get("/")
def home():
    return {
        "project": "AR/VR Based Virtual House Visualization and Planning System",
        "status": "Backend is running",
        "version": "1.2.0"
    }


@app.get("/locations")
def get_locations():
    data = load_regional_prices()

    locations = []

    for country, states in data.items():
        for state, cities in states.items():
            for city in cities:
                locations.append({
                    "country": country,
                    "state": state,
                    "city": city
                })

    return {
        "count": len(locations),
        "locations": locations
    }


@app.get("/materials")
def get_materials(
    country: str = "India",
    state: str = "Karnataka",
    city: str = "Bengaluru"
):
    data = load_regional_prices()

    try:
        city_data = data[country][state][city]
    except KeyError:
        raise HTTPException(
            status_code=404,
            detail=f"Pricing data not available for {city}, {state}, {country}"
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
        "materials": city_data["materials"]
    }


@app.get("/calculate-cost")
def get_cost(
    area_sqft: float,
    flooring: str = "standard",
    wall_material: str = "standard",
    country: str = "India",
    state: str = "Karnataka",
    city: str = "Bengaluru"
):
    try:
        return calculate_cost(
            area_sqft=area_sqft,
            flooring=flooring,
            wall_material=wall_material,
            country=country,
            state=state,
            city=city
        )
    except ValueError as error:
        raise HTTPException(
            status_code=404,
            detail=str(error)
        )


@app.get("/calculate-boq")
def get_boq(
    area_sqft: float,
    number_of_rooms: int,
    number_of_bathrooms: int,
    material_quality: str = "standard",
    country: str = "India",
    state: str = "Karnataka",
    city: str = "Bengaluru"
):
    try:
        return calculate_boq(
            area_sqft=area_sqft,
            number_of_rooms=number_of_rooms,
            number_of_bathrooms=number_of_bathrooms,
            material_quality=material_quality,
            country=country,
            state=state,
            city=city
        )
    except ValueError as error:
        raise HTTPException(
            status_code=404,
            detail=str(error)
        )


@app.get("/calculate-schedule")
def get_schedule(
    material_quality: str = "standard"
):
    return calculate_schedule(
        material_quality=material_quality
    )


@app.get("/analyze-house")
def analyze_house(
    area_sqft: float,
    number_of_rooms: int,
    number_of_bathrooms: int,
    flooring: str = "standard",
    wall_material: str = "standard",
    material_quality: str = "standard",
    country: str = "India",
    state: str = "Karnataka",
    city: str = "Bengaluru"
):
    try:
        cost = calculate_cost(
            area_sqft=area_sqft,
            flooring=flooring,
            wall_material=wall_material,
            country=country,
            state=state,
            city=city
        )

        boq = calculate_boq(
            area_sqft=area_sqft,
            number_of_rooms=number_of_rooms,
            number_of_bathrooms=number_of_bathrooms,
            material_quality=material_quality,
            country=country,
            state=state,
            city=city
        )

        schedule = calculate_schedule(
            material_quality=material_quality
        )

        return {
            "project": "AR/VR House Planning System",
            "house": {
                "area_sqft": area_sqft,
                "number_of_rooms": number_of_rooms,
                "number_of_bathrooms": number_of_bathrooms,
                "flooring": flooring,
                "wall_material": wall_material,
                "material_quality": material_quality,
                "country": country,
                "state": state,
                "city": city
            },
            "cost": cost,
            "boq": boq,
            "schedule": schedule
        }

    except ValueError as error:
        raise HTTPException(
            status_code=404,
            detail=str(error)
        )


@app.post("/analyze-house")
def analyze_house_json(house: HouseDesign):
    try:
        cost = calculate_cost(
            area_sqft=house.area_sqft,
            flooring=house.flooring,
            wall_material=house.wall_material,
            country=house.country,
            state=house.state,
            city=house.city
        )

        boq = calculate_boq(
            area_sqft=house.area_sqft,
            number_of_rooms=house.number_of_rooms,
            number_of_bathrooms=house.number_of_bathrooms,
            material_quality=house.material_quality,
            country=house.country,
            state=house.state,
            city=house.city
        )

        schedule = calculate_schedule(
            material_quality=house.material_quality
        )

        return {
            "project": "AR/VR House Planning System",
            "house": house.model_dump(),
            "cost": cost,
            "boq": boq,
            "schedule": schedule
        }

    except ValueError as error:
        raise HTTPException(
            status_code=404,
            detail=str(error)
        )