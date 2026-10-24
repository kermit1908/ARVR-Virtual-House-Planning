from typing import List, Optional, Dict, Any
from pydantic import BaseModel, Field

# --- User & Auth Schemas ---
class UserCreate(BaseModel):
    email: str = Field(..., min_length=4)
    password: str = Field(..., min_length=6)
    full_name: str = Field(..., min_length=2)

class UserLogin(BaseModel):
    email: str
    password: str

class UserResponse(BaseModel):
    id: str
    email: str
    full_name: str
    role: str
    created_at: str

class Token(BaseModel):
    access_token: str
    token_type: str = "bearer"
    user: UserResponse

# --- Project Schemas ---
class ProjectCreate(BaseModel):
    name: str = Field(..., min_length=1, max_length=255)
    description: Optional[str] = ""
    location: str = "Bengaluru"
    plot_width: float = Field(12.0, gt=0)
    plot_length: float = Field(15.0, gt=0)

class ProjectUpdate(BaseModel):
    name: Optional[str] = None
    description: Optional[str] = None
    location: Optional[str] = None
    plot_width: Optional[float] = Field(None, gt=0)
    plot_length: Optional[float] = Field(None, gt=0)

class ProjectResponse(BaseModel):
    id: str
    owner_id: str
    name: str
    description: Optional[str]
    location: str
    plot_width: float
    plot_length: float
    plot_area: float
    status: str
    created_at: str
    updated_at: str
    has_bim: bool = False
    estimated_cost: Optional[float] = None

# --- BIM Building Schemas ---
class RoomInput(BaseModel):
    name: str
    type: str # LivingRoom, Bedroom, Kitchen, Bathroom, DiningRoom, Balcony, Garage
    width: float = Field(..., gt=0.5)
    length: float = Field(..., gt=0.5)
    floor_level: int = 0
    wall_color: Optional[str] = "#F3F4F6"
    floor_material: Optional[str] = "Tiles" # Tiles, Wood, Marble, Granite

class FloorInput(BaseModel):
    level: int
    height: float = Field(3.0, gt=1.5, lt=6.0)
    rooms: List[RoomInput]

class BIMHouseConfig(BaseModel):
    floors_count: int = Field(1, ge=1, le=4)
    floor_height: float = Field(3.0, gt=2.0, lt=5.0)
    wall_thickness: float = Field(0.23, gt=0.1, lt=0.6)
    quality: str = "Standard" # Standard, Premium, Luxury
    floors: List[FloorInput]

class BIMElement(BaseModel):
    id: str
    type: str # Wall, Floor, Ceiling, Door, Window, Column, Roof
    floor_level: int
    room_name: Optional[str] = None
    length: Optional[float] = None
    width: Optional[float] = None
    height: Optional[float] = None
    thickness: Optional[float] = None
    area: float
    volume: float
    material: str
    position: List[float] # [x, y, z]
    rotation: List[float] # [rx, ry, rz]
    unit_cost: Optional[float] = None
    estimated_cost: Optional[float] = None

class BIMModelResponse(BaseModel):
    project_id: str
    floors_count: int
    floor_height: float
    wall_thickness: float
    builtup_area: float
    ground_coverage_area: float
    total_elements_count: int
    elements: List[BIMElement]
    raw_config: Dict[str, Any]
    updated_at: str

# --- QTO & BOQ Schemas ---
class QTOItem(BaseModel):
    item_code: str
    category: str
    description: str
    raw_quantity: float
    wastage_percent: float
    net_quantity: float
    unit: str
    calculation_basis: str

class QTOResponse(BaseModel):
    project_id: str
    builtup_area: float
    items: List[QTOItem]
    assumptions: List[str]

class BOQItem(BaseModel):
    item_id: str
    category: str
    description: str
    quantity: float
    unit: str
    unit_rate: float
    amount: float

class BOQResponse(BaseModel):
    project_id: str
    location: str
    items: List[BOQItem]
    subtotal: float
    contingency_percent: float
    contingency_amount: float
    grand_total: float
    disclaimer: str

# --- 5D Cost Engine Schemas ---
class CostCategoryBreakdown(BaseModel):
    category: str
    amount: float
    percentage: float

class CostEstimateResponse(BaseModel):
    project_id: str
    location: str
    quality: str
    builtup_area: float
    material_cost: float
    labour_cost: float
    services_cost: float # Electrical + Plumbing
    finishing_cost: float
    contingency_cost: float
    total_rule_based_cost: float
    cost_per_sqm: float
    breakdown: List[CostCategoryBreakdown]
    disclaimer: str

# --- AI Prediction Schemas ---
class AIPredictResponse(BaseModel):
    project_id: str
    predicted_cost: float
    confidence_lower: float
    confidence_upper: float
    margin_of_error: float
    rule_based_cost: float
    variance_amount: float
    variance_percent: float
    model_used: str
    r2_score: float
    mape: float
    disclaimer: str

class ModelMetricItem(BaseModel):
    model_name: str
    r2_score: float
    cv_r2_score: float
    mae_inr: float
    mape_percent: float
    rmse_inr: float

class ModelComparisonResponse(BaseModel):
    dataset_size: int
    best_model: str
    models: List[ModelMetricItem]
    top_features: Dict[str, float]

class OptimizationItem(BaseModel):
    category: str
    title: str
    current_spec: str
    proposed_spec: str
    potential_savings: float
    impact_level: str # Low, Medium, High
    recommendation_details: str

class OptimizationResponse(BaseModel):
    project_id: str
    current_total_cost: float
    optimized_potential_cost: float
    total_potential_savings: float
    savings_percentage: float
    recommendations: List[OptimizationItem]

# --- 4D Schedule Schemas ---
class TaskItem(BaseModel):
    id: str
    name: str
    stage: str
    duration_days: int
    start_day: int
    end_day: int
    dependencies: List[str]
    progress: int = 0
    assigned_crew: str

class ScheduleResponse(BaseModel):
    project_id: str
    total_duration_days: int
    start_date: str
    estimated_completion_date: str
    tasks: List[TaskItem]
    critical_path: List[str]

# --- Report Schemas ---
class ProjectReportResponse(BaseModel):
    project: ProjectResponse
    bim_summary: Dict[str, Any]
    qto: QTOResponse
    boq: BOQResponse
    cost_estimate: CostEstimateResponse
    ai_prediction: AIPredictResponse
    optimization: OptimizationResponse
    schedule: ScheduleResponse
    academic_evaluation: Dict[str, Any]
    generated_at: str
