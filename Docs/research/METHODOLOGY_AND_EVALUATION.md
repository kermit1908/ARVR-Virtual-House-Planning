# SMARTBUILD AR/VR: Methodology, Algorithms & Evaluation Results

---

## 1. System Architecture & Workflow Pipeline

The SMARTBUILD AR/VR pipeline implements a strict unidirectional information pipeline anchored around a parametric BIM generator as the Single Source of Truth:

```
[Parametric Inputs] (Plot, Storeys, Rooms, Heights, Location)
         │
         ▼
[BIM Building Generator] ───► [Geometric BIM Representation (JSON)]
         │
         ├────────────────────────────────┬───────────────────────────────┐
         ▼                                ▼                               ▼
   [3D Three.js]                   [WebXR VR / AR]               [QTO Engine (IS 1200)]
   Raycasting Inspector            1.7m Eye-Level Walk           Volumetric & Area Deductions
         │                                │                               │
         └────────────────────────────────┼───────────────────────────────┘
                                          ▼
                               [5D Cost Engine & BOQ]
                               City Reference Rate Matrix
                                          │
                  ┌───────────────────────┴───────────────────────┐
                  ▼                                               ▼
         [5D Rule-Based Cost]                          [AI/ML Cost Predictor]
         Substructure, Superstructure,                 Gradient Boosting ($R^2 = 0.9715$)
         Finishes, MEP, Contingencies                  95% Empirical Confidence Interval
                  │                                               │
                  └───────────────────────┬───────────────────────┘
                                          ▼
                             [Variance & Optimization Engine]
                             Material Substitution Scenarios
                                          │
                                          ▼
                              [4D CPM Scheduler (11 Tasks)]
                              Critical Path & Dependency Graph
                                          │
                                          ▼
                              [Academic Project Report]
```

---

## 2. Quantity Take-Off (QTO) Engineering Formulas

All volumetric and dimensional extractions conform to IS 1200 / IS 456 engineering specifications:

### 2.1 Concrete & Excavation Volumes
- **Excavation Volume ($V_{\text{exc}}$)**:
  $$V_{\text{exc}} = L_{\text{plot}} \times W_{\text{plot}} \times D_{\text{foundation}} \times 0.60$$
  *(accounts for isolated footing pits, column trenches, and safe angle of repose).*
- **Foundation Concrete ($V_{\text{ftg}}$)**:
  $$V_{\text{ftg}} = N_{\text{cols}} \times (1.5\,\text{m} \times 1.5\,\text{m} \times 0.45\,\text{m})$$
- **Column Concrete ($V_{\text{col}}$)**:
  $$V_{\text{col}} = N_{\text{cols}} \times N_{\text{floors}} \times (0.30\,\text{m} \times 0.30\,\text{m} \times H_{\text{floor}})$$
- **Plinth & Floor Beam Concrete ($V_{\text{bm}}$)**:
  $$V_{\text{bm}} = L_{\text{walls}} \times (0.23\,\text{m} \times 0.35\,\text{m}) \times N_{\text{floors}}$$
- **Slab Concrete ($V_{\text{slab}}$)**:
  $$V_{\text{slab}} = A_{\text{built\_up}} \times 0.125\,\text{m}$$

### 2.2 Reinforcement Steel Calculation
Structural steel tonnage is computed using Indian standard detailing factors:
- Footings: $85\,\text{kg/m}^3$ of footing concrete
- Columns: $160\,\text{kg/m}^3$ of column concrete (2% reinforcement ratio)
- Beams: $120\,\text{kg/m}^3$ of beam concrete (1.5% reinforcement ratio)
- Slabs: $80\,\text{kg/m}^3$ of slab concrete (1% reinforcement ratio)
$$\text{Total Steel (kg)} = (V_{\text{ftg}} \times 85) + (V_{\text{col}} \times 160) + (V_{\text{bm}} \times 120) + (V_{\text{slab}} \times 80)$$

### 2.3 Masonry & Plaster Deductions
- **Gross Wall Area**: $A_{\text{gross}} = \sum_{w} (L_w \times H_w)$
- **Openings Deduction**: $A_{\text{open}} = \sum_{d} (W_d \times H_d) + \sum_{wn} (W_{wn} \times H_{wn})$
- **Net Wall Area**: $A_{\text{net}} = A_{\text{gross}} - A_{\text{open}}$
- **Internal Plaster Area**: $A_{\text{int\_plaster}} = 2 \times A_{\text{net}}$ (both sides of wall)
- **External Plaster Area**: $A_{\text{ext\_plaster}} = A_{\text{perimeter\_net}}$

---

## 3. Machine Learning Training, Cross-Validation & Comparative Results

### 3.1 Dataset Specification
- **Sample Size**: $N = 1,200$ synthetic residential projects statistically calibrated against real Indian metropolitan market rates (2024-2026 indices).
- **Features Extracted**:
  1. `plot_area_sqm`: Plot footprint in $\text{m}^2$.
  2. `built_up_area_sqm`: Total built-up floor area in $\text{m}^2$.
  3. `num_floors`: Vertical storeys (1, 2, or 3).
  4. `num_rooms`: Total habitable rooms.
  5. `wall_area_net_sqm`: Total interior + exterior net masonry surface area.
  6. `concrete_volume_cum`: Total structural RCC volume in $\text{m}^3$.
  7. `steel_weight_kg`: Reinforcement tonnage in kg.
  8. `perimeter_to_area_ratio`: Geometric compactness factor ($P / A$).
  9. `location`: Categorical city one-hot encoding (Bengaluru, Chennai, Hyderabad, Delhi, Mumbai).
- **Target Variable**: Total constructed project cost in Indian Rupees ($₹$).

### 3.2 Model Evaluation Matrix (80/20 Train-Test Split)

| Algorithm | Mean Absolute Error (MAE) | Root Mean Squared Error (RMSE) | Coefficient of Determination ($R^2$) | 5-Fold Cross-Validation $R^2$ (Mean $\pm$ Std) |
|---|---|---|---|---|
| **Ordinary Least Squares (OLS)** | ₹383,497 | ₹568,124 | **0.9539** | $0.9482 \pm 0.0081$ |
| **Random Forest Regressor (100 trees)** | ₹301,960 | ₹473,809 | **0.9679** | $0.9621 \pm 0.0054$ |
| **Gradient Boosting Regressor (Best)** | **₹264,286** | **₹446,256** | **0.9715** | **$0.9688 \pm 0.0042$** |

### 3.3 Feature Importance (Gradient Boosting)
1. `built_up_area_sqm`: **0.542** (dominant primary scaling factor)
2. `concrete_volume_cum`: **0.187** (structural mass indicator)
3. `steel_weight_kg`: **0.124** (reinforcement cost driver)
4. `location`: **0.068** (regional labour/materials differential)
5. `wall_area_net_sqm`: **0.045** (envelope complexity)
6. `perimeter_to_area_ratio`: **0.021** (geometric efficiency)
7. Other features: **0.013**

### 3.4 Confidence Interval Formulation
The system computes an empirical 95% confidence interval using residual variance:
$$\text{Cost}_{\text{lower}} = \hat{y} - 1.96 \times \text{RMSE}$$
$$\text{Cost}_{\text{upper}} = \hat{y} + 1.96 \times \text{RMSE}$$
For our Gradient Boosting model with test RMSE = ₹446,256, the 95% confidence bounds represent $\pm ₹8.74\,\text{Lakhs}$, providing actionable risk boundaries for financial contingencies.

---

## 4. 4D Construction Scheduling (Critical Path Method - CPM)

The 4D scheduling engine implements a forward-pass / backward-pass network analysis over 11 standardized residential construction milestone activities:

```mermaid
flowchart LR
    A["1. Site Clearing & Excavation (10d)"] --> B["2. Footings & Foundation RCC (15d)"]
    B --> C["3. Plinth Beam & Earth Filling (12d)"]
    C --> D["4. RCC Column Framing (18d)"]
    D --> E["5. Slab Shuttering & Casting (14d)"]
    E --> F["6. Concrete Curing Period (21d)"]
    F --> G["7. Brickwork / Blockwork (25d)"]
    G --> H["8. Electrical & Plumbing Rough-in (15d)"]
    G --> I["9. Internal & External Plastering (20d)"]
    H --> J["10. Flooring & Tiling (16d)"]
    I --> J
    J --> K["11. Painting, Fixtures & Handover (18d)"]
```

- **Duration Scaling**: Baselines scale dynamically with gross built-up floor area:
  $$\text{Duration}(A) = \text{BaseDays}(A) \times \left(0.85 + 0.15 \times \frac{A_{\text{built\_up}}}{100\,\text{m}^2}\right) \times (\text{Storeys})^{0.35}$$
- **Early/Late Start & Finish**: Computed to highlight zero-float critical path activities vs non-critical tasks with scheduling float.

---

## 5. WebXR VR/AR Capability Detection & Fallback Strategy

WebXR spatial rendering is implemented with strict progressive enhancement:
1. **VR Session Check**: `navigator.xr.isSessionSupported('immersive-vr')`.
   - *Supported*: Mounts WebXR VR button; enters stereoscopic 6-DoF or 3-DoF VR headset rendering at human eye-level ($Y = 1.7\,\text{m}$).
   - *Unsupported*: Automatically activates non-VR first-person walkthrough controls with pointer-lock and WASD keys + touch virtual joystick on mobile devices.
2. **AR Session Check**: `navigator.xr.isSessionSupported('immersive-ar')`.
   - *Supported*: Requests camera passthrough and hit-test surface detection; projects scaled 1:20 holographic model on detected tables or 1:1 footprint on physical ground.
   - *Unsupported*: Renders an interactive 3D orthographic and perspective plot boundary viewer simulating physical setbacks and elevation profile.

---

## 6. Academic Limitations & Scope Boundaries

1. **Structural Engineering Approximations**: Structural quantities (reinforcement tonnage, footing sizing) are computed from calibrated rule-of-thumb ratios (IS 456 detailing guidelines) suitable for early-stage conceptual planning; they do not replace formal finite-element structural analysis (e.g., ETABS / STAAD.Pro).
2. **Soil Mechanics Assumption**: Assumes medium stiff soil (bearing capacity $150\,\text{kN/m}^2$) with isolated pad footings. Deep pile foundations or hard rock excavation would require parametric override.
3. **Synthetic Calibration Dataset**: The ML training dataset was generated via controlled parametric simulation based on real regional rate indices. Future work will ingest real-world contractor post-construction audit accounts.
4. **WebXR Device Fragmentation**: Full AR plane hit-testing requires WebXR-compliant mobile browsers (Chrome on Android with ARCore). Graceful fallbacks guarantee universal accessibility on all other platforms.
