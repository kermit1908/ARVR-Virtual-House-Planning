export interface User {
  id: string;
  email: string;
  full_name: string;
  role: string;
  created_at: string;
}

export interface AuthResponse {
  access_token: string;
  token_type: string;
  user: User;
}

export interface Project {
  id: string;
  owner_id: string;
  name: string;
  description: string | null;
  location: string;
  plot_width: number;
  plot_length: number;
  plot_area: number;
  status: string;
  created_at: string;
  updated_at: string;
  has_bim: boolean;
  estimated_cost: number | null;
}

export interface RoomInput {
  name: string;
  type: string;
  width: number;
  length: number;
  floor_level: number;
  wall_color?: string;
  floor_material?: string;
}

export interface FloorInput {
  level: number;
  height: number;
  rooms: RoomInput[];
}

export interface BIMHouseConfig {
  floors_count: number;
  floor_height: number;
  wall_thickness: number;
  quality: string;
  floors: FloorInput[];
}

export interface BIMElement {
  id: string;
  type: 'Wall' | 'Floor' | 'Ceiling' | 'Door' | 'Window' | 'Roof' | 'Column';
  floor_level: number;
  room_name?: string;
  length?: number;
  width?: number;
  height?: number;
  thickness?: number;
  area: number;
  volume: number;
  material: string;
  position: [number, number, number];
  rotation: [number, number, number];
  unit_cost?: number;
  estimated_cost?: number;
}

export interface BIMModelResponse {
  project_id: string;
  floors_count: number;
  floor_height: number;
  wall_thickness: number;
  builtup_area: number;
  ground_coverage_area: number;
  total_elements_count: number;
  elements: BIMElement[];
  raw_config: BIMHouseConfig;
  updated_at: string;
}

export interface QTOItem {
  item_code: string;
  category: string;
  description: string;
  raw_quantity: number;
  wastage_percent: number;
  net_quantity: number;
  unit: string;
  calculation_basis: string;
}

export interface QTOResponse {
  project_id: string;
  builtup_area: number;
  items: QTOItem[];
  assumptions: string[];
}

export interface BOQItem {
  item_id: string;
  category: string;
  description: string;
  quantity: number;
  unit: string;
  unit_rate: number;
  amount: number;
}

export interface BOQResponse {
  project_id: string;
  location: string;
  items: BOQItem[];
  subtotal: number;
  contingency_percent: number;
  contingency_amount: number;
  grand_total: number;
  disclaimer: string;
}

export interface CostCategoryBreakdown {
  category: string;
  amount: number;
  percentage: number;
}

export interface CostEstimateResponse {
  project_id: string;
  location: string;
  quality: string;
  builtup_area: number;
  material_cost: number;
  labour_cost: number;
  services_cost: number;
  finishing_cost: number;
  contingency_cost: number;
  total_rule_based_cost: number;
  cost_per_sqm: number;
  breakdown: CostCategoryBreakdown[];
  disclaimer: string;
}

export interface AIPredictResponse {
  project_id: string;
  predicted_cost: number;
  confidence_lower: number;
  confidence_upper: number;
  margin_of_error: number;
  rule_based_cost: number;
  variance_amount: number;
  variance_percent: number;
  model_used: string;
  r2_score: number;
  mape: number;
  disclaimer: string;
}

export interface ModelMetricItem {
  model_name: string;
  r2_score: number;
  cv_r2_score: number;
  mae_inr: number;
  mape_percent: number;
  rmse_inr: number;
}

export interface ModelComparisonResponse {
  dataset_size: number;
  best_model: string;
  models: ModelMetricItem[];
  top_features: Record<string, number>;
}

export interface OptimizationItem {
  category: string;
  title: string;
  current_spec: string;
  proposed_spec: string;
  potential_savings: number;
  impact_level: 'Low' | 'Medium' | 'High';
  recommendation_details: string;
}

export interface OptimizationResponse {
  project_id: string;
  current_total_cost: number;
  optimized_potential_cost: number;
  total_potential_savings: number;
  savings_percentage: number;
  recommendations: OptimizationItem[];
}

export interface TaskItem {
  id: string;
  name: string;
  stage: string;
  duration_days: number;
  start_day: number;
  end_day: number;
  dependencies: string[];
  progress: number;
  assigned_crew: string;
}

export interface ScheduleResponse {
  project_id: string;
  total_duration_days: number;
  start_date: string;
  estimated_completion_date: string;
  tasks: TaskItem[];
  critical_path: string[];
}

export interface ProjectReportResponse {
  project: Project;
  bim_summary: Record<string, any>;
  qto: QTOResponse;
  boq: BOQResponse;
  cost_estimate: CostEstimateResponse;
  ai_prediction: AIPredictResponse;
  optimization: OptimizationResponse;
  schedule: ScheduleResponse;
  academic_evaluation: Record<string, any>;
  generated_at: string;
}
