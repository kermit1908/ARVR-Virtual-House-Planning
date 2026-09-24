from pydantic import BaseModel, Field


class HouseDesign(BaseModel):
    area_sqft: float = Field(gt=0)
    number_of_rooms: int = Field(gt=0)
    number_of_bathrooms: int = Field(ge=0)
    flooring: str = "standard"
    wall_material: str = "standard"
    material_quality: str = "standard"
    country: str = "India"
    state: str = "Karnataka"
    city: str = "Bengaluru"