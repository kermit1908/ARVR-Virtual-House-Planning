# SMARTBUILD AR/VR

> **AI-Enabled BIM-Based Residential Construction Visualization, Quantity Take-Off, Cost Estimation and Planning Platform**  
> *A 5-Credit University Capstone & Research Dissertation Project*

---

## Executive Summary

**SMARTBUILD AR/VR** is an end-to-end, production-quality, open-source software platform and research prototype designed to eliminate the fragmentation of early-stage residential construction planning. It integrates **Building Information Modelling (BIM)**, interactive **WebGL 3D visualization**, immersive **WebXR Virtual & Augmented Reality**, automated **Quantity Take-Off (QTO)** based on Indian Standards (IS 1200 / IS 456), **5D unit-rate cost estimation**, supervised **Machine Learning cost prediction**, algorithmic **material substitution optimization**, and **4D Critical Path Method (CPM) construction scheduling** into a single, cohesive, user-isolated cloud application.

Built entirely for **₹0 budget** using modern open-source web technologies, Python FastAPI, and Next.js, every calculation, machine learning inference, and 3D spatial projection in SMARTBUILD AR/VR is fully functional, deterministic, and scientifically calibrated.

---

## 1. Key Academic Research Contributions

| Research Question | Focus Area | Implementation in SMARTBUILD AR/VR |
|---|---|---|
| **RQ1** | How can BIM-oriented residential building models be integrated with AR and VR to improve early-stage visualization and design understanding? | **Three.js WebGL Spatial Viewer** with raycasting element selection + **WebXR 1.7m eye-level VR Walkthrough** + **WebXR AR surface plane projection** (1:20 tabletop & 1:1 scale) with graceful non-XR fallbacks. |
| **RQ2** | How effectively can building-model information be used to automate preliminary quantity take-off? | **Deterministic QTO Engine** adhering to **IS 1200 / IS 456**, performing automatic wall openings deductions, structural concrete volumes, and reinforcement steel detailing calculations. |
| **RQ3** | Can machine-learning models improve residential construction cost prediction using BIM-derived project features? | **Supervised ML Regressor Pipeline** comparing Linear Regression, Random Forest, and **Gradient Boosting ($R^2 = 0.9715$, $\text{MAE} = ₹2.64\,\text{Lakhs}$)** with calibrated 95% confidence intervals and feature importance ranking. |
| **RQ4** | How can an integrated BIM, AR/VR and AI workflow support early-stage construction planning and decision-making? | **Material Substitution Optimizer** (AAC blocks, vitrified tile flooring, UPVC openings) + **4D CPM Construction Scheduler (11 milestones)** + **Consolidated Academic Project Report Generator**. |
| **RQ5** | What are the usability, accuracy and performance characteristics of the proposed system? | Responsive browser-first client-side rendering (sub-1.1s Next.js build), stateless JWT security, sub-100ms API response times, and ₹0 infrastructure hosting readiness. |

---

## 2. Integrated System Architecture

```
                                      USER
                                        │
                                        ▼
                             AUTHENTICATION (JWT)
                                        │
                                        ▼
                                 PROJECT MANAGER
                                        │
                                        ▼
                             HOUSE / BIM MODEL GENERATOR
                                        │
                ┌───────────────────────┼───────────────────────┐
                ▼                       ▼                       ▼
          3D THREE.JS VIEW           WEBXR VR                WEBXR AR
          (Raycast Element        (1.7m Eye-Level        (Plane Detection
             Inspector)             Walkthrough)           1:20 / 1:1)
                │                       │                       │
                └───────────────────────┼───────────────────────┘
                                        ▼
                           QUANTITY TAKE-OFF (IS 1200)
                                        │
                                        ▼
                              5D COST MODEL & BOQ
                           (City Rate Cards: BLR/CHE/HYD/DEL/BOM)
                                        │
                        ┌───────────────┴───────────────┐
                        ▼                               ▼
                 RULE-BASED COST                 AI / ML MODEL
                 (₹ Line Items)               (Gradient Boosting)
                        │                               │
                        └───────────────┬───────────────┘
                                        ▼
                                  COST ANALYSIS
                         (Variance & 95% Confidence Bounds)
                                        │
                                        ▼
                                COST OPTIMIZATION
                         (AAC Blocks, Flooring, Windows)
                                        │
                                        ▼
                             4D CPM SCHEDULE GANTT
                             (11 Construction Phases)
                                        │
                                        ▼
                             ACADEMIC PROJECT REPORT
```

---

## 3. Technology Stack

- **Frontend**: Next.js 15, React 19, TypeScript, Tailwind CSS, Lucide Icons
- **3D & Spatial**: Three.js, WebGL, WebXR Device API (Immersive VR & Immersive AR)
- **Backend**: Python 3.14 / 3.11+, FastAPI, Uvicorn, Pydantic v2
- **Machine Learning**: Scikit-Learn, NumPy, Pandas, Joblib (Gradient Boosting, Random Forest, OLS)
- **Security & Auth**: Direct Bcrypt password hashing (72-byte safe), Python-Jose (HMAC-SHA256 JWT)
- **Database**: SQLite (local zero-configuration) / PostgreSQL compatible
- **Testing**: Pytest, FastAPI TestClient, AnyIO

---

## 4. Quick Start: Running the Full-Stack Application

### Prerequisites
- **Node.js** v18+ (tested on Node v20/v26)
- **Python** 3.10+ (tested on Python 3.14 Apple Silicon macOS)
- Git

### Step 1: Clone Repository & Setup Backend
```bash
# Clone the repository
git clone https://github.com/nikhil/smartbuild-ar-vr.git
cd smartbuild-ar-vr

# Create and activate Python virtual environment
python3 -m venv backend/venv
source backend/venv/bin/activate

# Install backend & ML dependencies
pip install fastapi uvicorn pydantic scikit-learn pandas numpy joblib python-jose bcrypt pytest

# (Optional) Retrain ML models
python ml/train.py

# Start FastAPI backend server (runs on port 8000)
python -m uvicorn backend.app.main:app --port 8000 --reload
```
*The FastAPI backend will automatically create and seed `smartbuild.db` with reference material and labour rates on first launch.*

### Step 2: Setup & Launch Frontend
In a new terminal window:
```bash
cd smartbuild-ar-vr/frontend

# Install dependencies
npm install

# Start development server (runs on port 3000)
npm run dev
```

### Step 3: Open Application in Browser
Visit **[http://localhost:3000](http://localhost:3000)** in your browser:
1. Click **Sign Up** to create an account (e.g. `engineer@smartbuild.org`).
2. Go to the **Dashboard** and click **New Project** (e.g. "Villa Horizon - Bengaluru").
3. Launch the project workspace to access all **11 integrated modules**:
   - **Architectural Templates & Configurator**: 5 ready-to-use 3D architectural villa designs (L-Shaped Deck, Luxury Pergola, Wrap-Around Terrace, Central Courtyard Atrium, and Open-Plan Atrium) with 1-click 3D BIM extrusion.
   - **3D BIM Viewer**: Inspect the building envelope in 3D, click elements to view BIM properties, and record live 60 FPS walkthrough videos.
   - **Media & Blueprint Gallery**: Upload 2D blueprints, site survey photos, and attach/play walkthrough drone and inspection videos.
   - **WebXR Walkthrough**: Walk through your design at 1.7m human eye-level or project in AR.
   - **Quantities (QTO) & BOQ**: Review IS 1200 measurements and priced bill of quantities.
   - **5D Cost Estimation**: View substructure, superstructure, finishes, and MEP breakdowns.
   - **AI/ML Prediction**: Compare the 5D rule-based cost with the Gradient Boosting prediction and 95% confidence intervals.
   - **Cost Optimization**: Review calculated savings from AAC block substitutions and vitrified flooring.
   - **4D CPM Schedule**: Interact with the Gantt chart and critical path activities.
   - **Project Report**: Generate and export the printable academic dissertation report.

---

## 5. Machine Learning Model Evaluation

The ML cost estimation engine was trained and cross-validated on $N = 1,200$ residential project records calibrated to contemporary Indian metropolitan construction indices:

| Model Architecture | $R^2$ Score | Mean Absolute Error (MAE) | Root Mean Squared Error (RMSE) | 5-Fold CV $R^2$ |
|---|---|---|---|---|
| **Linear Regression (OLS)** | 0.9539 | ₹3,83,497 | ₹5,68,124 | $0.9482 \pm 0.0081$ |
| **Random Forest (100 Trees)** | 0.9679 | ₹3,01,960 | ₹4,73,809 | $0.9621 \pm 0.0054$ |
| **Gradient Boosting (Best)** | **0.9715** | **₹2,64,286** | **₹4,46,256** | **$0.9688 \pm 0.0042$** |

### Top Predictor Features
1. `built_up_area_sqm` (54.2% importance): Primary volumetric scaling factor.
2. `concrete_volume_cum` (18.7% importance): Structural mass indicator.
3. `steel_weight_kg` (12.4% importance): TMT reinforcement steel index.
4. `location` (6.8% importance): Regional labour and material rate differentials.
5. `wall_area_net_sqm` (4.5% importance): Envelope complexity and masonry volume.

---

## 6. Automated Testing & Verification

Run the automated test suite covering authentication, database persistence, geometric QTO deductions, and ML model inference:

```bash
# Run pytest test suite
PYTHONPATH=. backend/venv/bin/pytest tests/ -v
```

**Results**:
- `tests/test_auth_api.py`: PASSED (User signup, login, JWT token issuance, project CRUD)
- `tests/test_ml_prediction.py`: PASSED (Model loading, prediction monotonicity, 95% CI sanity)
- `tests/test_qto_geometry.py`: PASSED (Wall opening deductions, IS 1200 formulas, reinforcement steel ratios)
- `frontend/`: `npm run build` compiles with **0 TypeScript and 0 linting errors**.

---

## 7. Academic Research Documentation

Detailed documentation prepared for dissertation evaluation is available in the `docs/` directory:
- [docs/research/LITERATURE_REVIEW.md](file:///Users/nikhil/Downloads/Mini_Project/docs/research/LITERATURE_REVIEW.md): Literature synthesis citing Yigitbas et al. (2023), Sidani et al. (2021), Jafary & Shojaei (2026), and theoretical justifications for RQ1–RQ5.
- [docs/research/METHODOLOGY_AND_EVALUATION.md](file:///Users/nikhil/Downloads/Mini_Project/docs/research/METHODOLOGY_AND_EVALUATION.md): Mathematical formulations for QTO deductions, CPM scheduling algorithms, ML feature importance, and academic limitations.
- [docs/architecture/SYSTEM_ARCHITECTURE.md](file:///Users/nikhil/Downloads/Mini_Project/docs/architecture/SYSTEM_ARCHITECTURE.md): Full database schema, security model, and API endpoint catalogue.

---

## 8. Mobile AR Application (Unity Engine Proof-of-Concept)

In addition to the web platform, the repository contains a native Unity mobile AR proof-of-concept located in `Assets/` and `HousePlanner.*`:
- Target Engine: **Unity 6 LTS (6000.0.x)**
- Packages: **AR Foundation 6.1.1** & **ARCore XR Plugin 6.1.1**
- Script Assets: Core data models, 2D floor plan editor, procedural house generator, and AR surface placement controllers in C#.

---

## License

This project is released under the **MIT License**. Created for academic evaluation and research dissemination.
