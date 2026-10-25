# A SPATIALLY GROUNDED AR/VR FRAMEWORK FOR RESIDENTIAL DESIGN VISUALIZATION AND VALIDATION

**Nikhil Singh S, Krishiv T, Bhavishya Das**  
*Department of Computer Science and Engineering*  
*Presidency University, Bengaluru, India*  

---

### Abstract
Augmented reality (AR) and virtual reality (VR) offer transformative potential for inspecting residential architectural designs, yet visualizing a digital layout in isolation does not establish whether it physically fits an existing room, satisfies functional circulation clearances, or supports realistic cost estimation. This paper presents an integrated full-stack framework connecting room input, an editable Building Information Modelling (BIM) parametric model, geometric validation, on-site AR review in the physical environment, and a planned VR walkthrough extension. While commercial systems and prior research provide isolated tools for room scanning, digital furnishing, and visual tours, their unified connection to explicit spatial constraints, deterministic quantity take-off, and construction budgeting has remained unaddressed. We report the implementation and architectural validation of the AR/VR House Planner system, an open-web application developed at Presidency University. The frontend is built using Next.js 15, React 19, TypeScript, and Three.js, while the backend is implemented using Python FastAPI with SQLite for persistent project and rate storage. The application integration is fully functional, supporting parametric template configuration, live 3D visualization, deterministic Quantity Take-Off under Indian Standards (IS 1200 / IS 456), and multi-region 5D cost estimation across five metropolitan schedules. The Augmented Reality (AR) component is currently functional and serves as the implemented immersive modality via browser-based WebXR surface tracking, allowing users to project and inspect full-scale models within physical spaces. In contrast, Virtual Reality (VR) walkthrough functionality is retained as a planned future development phase rather than a deployed feature. The paper outlines the implemented architecture, operational definitions for spatial accuracy, and an empirical evaluation plan to assess geometric violation detection and user planning confidence.

**Index Terms**— *Augmented reality, virtual reality, spatial design, residential planning, spatial validation, user evaluation, Building Information Modelling (BIM), quantity take-off.*

---

## I. INTRODUCTION

Residential designs are commonly shared through floor plans, photographs, renderings, and three-dimensional models. These views communicate geometry and appearance, but users may find it difficult to judge how a proposed layout will relate to the physical room or feel at human scale. Augmented reality (AR) and virtual reality (VR) offer complementary views: AR can place a design in the existing room, while VR can provide an immersive view of a proposed interior. Recent reviews describe growing use of these technologies across architecture, engineering, and construction (AEC), while also reporting adoption and implementation challenges [1], [2].

Existing products already provide several parts of this workflow. Matterport offers digital twins, tours, measurements, and schematic floor plans [3]. IKEA Kreativ supports room capture followed by digital redesign [4]. Planner 5D offers floor-plan and 2D/3D design tools, alongside AR experiences on supported platforms [5], [6]. Apple RoomPlan produces a structured room representation from supported devices and sensors [7]. These examples mean that room capture or furniture visualization alone is not a sufficient novelty claim.

Research has examined immersive experience in real-estate contexts and VR as a method for end-user design review [8]–[10]. However, the value of a residential planning workflow depends on more than visual realism. A user also needs to know whether furniture is inside the usable room, overlaps another object, blocks circulation, or appears at an incorrect scale. Furthermore, when residential layouts are planned in early stages, homeowners and designers require immediate insight into how spatial dimensions translate into physical materials and construction budgets. This paper presents a design framework that makes explicit geometric checks, automated quantity extraction, and multi-region cost estimation part of a shared AR-enabled workflow, while defining a reproducible evaluation plan for both current features and planned extensions.

The paper makes three contributions: (1) it defines and demonstrates an integrated full-stack workflow connecting room and plot input, an editable parametric BIM model, spatial constraint checks, and on-site AR review; (2) it documents the implemented frontend, backend, and functional AR component while transparently demarcating VR review as planned future work; and (3) it specifies an evaluation protocol and candidate metrics to compare the integrated workflow against conventional 2D/3D planning tools.

The scope is intentionally bounded. The paper addresses interior-scale geometric arrangement and structural building-envelope configuration in residential layouts. Spatial checks and preliminary 5D cost estimates are presented as decision support: a warning or quantity summary helps users evaluate options during preliminary planning, but does not certify structural engineering safety or replace formal local municipal permits. The system concept assumes that room dimensions, object assets, and rate schedules are available with verifiable accuracy.

---

## II. RELATED WORK

### A. Commercial Residential Visualization
Commercial tools demonstrate that room capture, digital furnishing, floor-plan editing, and walkthroughs are already available. Matterport emphasizes spatial digital twins and related tour and floor-plan outputs [3]. IKEA Kreativ supports capture and subsequent editing of a room design using mobile computer vision [4]. Planner 5D provides room planning and visualization features, with some capabilities dependent on platform and device support [5], [6]. Apple RoomPlan provides a sensor-based route to a structured room model for downstream applications [7]. In addition, contemporary WebXR specifications [19] enable lightweight web-first 3D visualization without proprietary software installation. These systems establish a practical baseline and inform the proposed design, but they remain closed ecosystems limited to visual inspection without structural quantity take-off or localized construction economics.

### B. Spatial Capture and Immersive Review
Room capture methods can produce models with recognized room elements and dimensions; the resulting quality remains dependent on sensors, coverage, and the downstream representation [7]. Prior work has also examined automated furniture arrangement using spatial and functional relationships [11]. Research on immersive design review compares VR with two-dimensional views and physical review, reporting potential benefits for affective appraisal and design preferences under the study conditions [9]. In real-estate settings, immersive and non-immersive experiences have also been compared [8]. A separate review considers XR impacts on residential real-estate markets and project management [12], while a recent architectural review surveys immersive VR applications and research directions [13]. Recent investigations in browser-based AR emphasize the importance of perceptual scale fidelity and spatial awareness during interior layout tasks [20]. These findings motivate systematic evaluation, but results from other studies cannot be treated as measurements of this project.

### C. Synthesis and Design Implications
Taken together, the product examples and studies suggest four design implications:
1. *Spatial input must be treated as an upstream dependency*: Dimensional inaccuracies propagate directly into object fit, collision checking, and any subsequent quantity extraction.
2. *Visualization and validation should be separable*: A scene may render plausibly in 3D or AR even when an object intersects a wall, violates passage clearances, or incurs excessive material waste. Explicit constraint-checking logic is required.
3. *Review modalities must reference a single saved design state*: AR exposes the relationship between a proposed object or building envelope and the physical space, while VR will later support egocentric review of enclosed interior sightlines.
4. *Visual layouts should inform construction feasibility*: Recent literature on BIM-based estimation [14]–[16] demonstrates that linking 3D geometry directly with standard measurement methods (e.g., IS 1200 [17] and IS 456 [18]) significantly reduces early-stage budget uncertainty.

---

## III. RESEARCH GAP AND CONTRIBUTIONS

The literature and products reviewed here show that AR/VR visualization is not new. A narrower opportunity for this project is to connect a modifiable residential layout to explicit geometric checks, automated quantity take-off, and localized cost estimation within an open-web architecture, and to define a reproducible evaluation plan. Existing tools often expose different combinations of capture, placement, and visualization. A project-level workflow that clearly records which constraints are checked, which review mode is used, and what outcome is measured makes its behavior easier to inspect and compare.

The validation layer considers four conditions for an object or building element $O$ in room or boundary $R$: boundary containment $B(O)$, collision avoidance $C(O)$, clearance $K(O)$, and scale consistency $S(O)$. A comprehensive validity indicator is formulated as:

$$V(O) = B(O) \cdot C(O) \cdot K(O) \cdot S(O)$$

Where each term evaluates to 1 when its condition is satisfied and 0 otherwise:
- $B(O) = 1 \iff \operatorname{Proj}_{xy}(O) \subseteq R$ (object boundary projection lies strictly within room polygon).
- $C(O) = 1 \iff \operatorname{AABB}(O) \cap \operatorname{AABB}(O') = \emptyset, \forall O' \neq O$ (axis-aligned bounding box collision avoidance).
- $K(O) = 1 \iff \operatorname{dist}(O, \text{Path}) \ge w_{\min}$ (clearance threshold $w_{\min} \ge 0.90\text{ m}$ along circulation corridors).
- $S(O) = 1 \iff \|\mathbf{s}(O) - \mathbf{s}_{\text{target}}\| / \|\mathbf{s}_{\text{target}}\| \le \epsilon_{\text{scale}}$ (metric scale consistency within calibrated tolerance).

Thresholds for clearance and acceptable scale error are configurable design rules documented prior to user testing.

### A. Research Questions
The study is organized around three primary research questions:
- **RQ1**: Does explicit geometric feedback help users identify and correct invalid layout placements more reliably relative to a conventional editor with the same room and object set?
- **RQ2**: Does reviewing a residential layout in on-site AR change users' spatial understanding, scale perception, and placement confidence compared with standard 2D and 3D screen views?
- **RQ3**: What technical constraints—such as plane-detection latency, tracking drift, coordinate registration error, or browser compatibility—affect the feasibility and usability of the browser-based AR house-planning workflow?

The corresponding hypotheses remain conditional until experimental trials are conducted. The validation hypothesis is that actionable warnings reduce unresolved geometric violations, though feedback latency must be minimized to avoid user cognitive fatigue. The modality hypothesis is that AR is uniquely effective for judging physical fit against real-world boundaries, while the planned VR mode will address interior circulation impression.

---

## IV. PROPOSED FRAMEWORK

The framework follows a feedback loop. The room or house envelope is supplied as a custom plot boundary or selected from pre-engineered templates; an editable parametric model stores room geometry, element dimensions, and object transforms; the validation layer checks constraints; and the design state is reviewed in interactive 3D and on-site AR. The user may alter the layout and repeat the checks. Fig. 1 summarizes this workflow.

```
+--------------------------------------------------------------------------------------------------+
|      Fig. 1. Architecture and interactive workflow of the implemented AR/VR House Planner        |
|                                                                                                  |
|  [Room / Plot Input]                                                                             |
|            |                                                                                     |
|            v                                                                                     |
|  [Parametric BIM Model (SSOT)]                                                                   |
|            |                                                                                     |
|            +-----------------------+-----------------------+                                     |
|            |                       |                       |                                     |
|            v                       v                       v                                     |
|     [Interactive 3D]       [WebXR AR Review]       [Planned VR Review]                           |
|      (Three.js/WebGL)     (Surface Tracking)      (Future Extension)                             |
|            |                       |                                                             |
|            +-----------------------+                                                             |
|                                    |                                                             |
|                                    v                                                             |
|                   [Spatial Validation: V(O) Checks]                                              |
|                                    |                                                             |
|                                    v                                                             |
|                  [Automated IS 1200 / IS 456 QTO]                                                |
|                                    |                                                             |
|                                    v                                                             |
|                   [5D BIM Multi-Region Cost Model]                                               |
|                                    |                                                             |
|                                    v                                                             |
|                   [Project Storage & Reporting]                                                  |
+--------------------------------------------------------------------------------------------------+
```

### A. Requirements and Design Principles
The first requirement is scale preservation: room and object dimensions are expressed in standard metric coordinates ($1\text{ unit} = 1.0\text{ meter}$). The second is traceability: every validation outcome and quantity take-off maps to an explicit geometric formula or statutory standard. The third is consistency: alternative review modes load the identical underlying JSON data model. The fourth is recoverability: users can modify spatial arrangements and receive real-time feedback. Finally, interaction degrades gracefully: when immersive WebXR tracking is unavailable on a client device, the application provides full 3D orbital touch and mouse navigation.

### B. Room Input and Editable Design
The room input preserves dimensions and represents relevant boundaries, partitions, door apertures, and window openings. In the implemented application, users can initialize a project using ten architectural templates (from 1BHK starter configurations to multi-room layouts) or define parametric plot boundaries ($W \times L$). The editable model stores 3D position $[x, y, z]$, physical dimensions $[\text{length}, \text{width}, \text{height}]$, solid volume ($m^3$), and material properties for every wall, floor, door, and window. A single unified design state is maintained so that 2D planning, 3D visualization, and AR review do not diverge.

### C. Placement, Validation, and Feedback
The user can select, add, reposition, and inspect architectural elements. Validation utilizes axis-aligned bounding boxes for collision and boundary confinement. Clearance rules enforce minimum passage zones along circulation vectors. Wall deductions adhere to IS 1200 Part III [17], accounting for door apertures ($1.0\text{ m} \times 2.1\text{ m}$) and window openings ($1.5\text{ m} \times 1.2\text{ m}$). Shared interior partition walls ($115\text{ mm}$ thickness) are deduplicated from exterior perimeter envelopes ($230\text{ mm}$ thickness) through a party-wall sharing coefficient ($\phi = 0.70$) to prevent overlapping volume inflation.

### D. AR and VR Review
AR is implemented for comparing full-scale and scaled digital models with physical environments, especially where real-world boundary alignment, clearance against existing walls, and site relationship matter. Implemented via the HTML5 WebXR Device API (`immersive-ar`), the model is anchored to detected horizontal planes at $1:20$ tabletop scale or $1:1$ full scale. In contrast, VR is proposed for egocentric interior immersion, circulation inspection, and sightline review. The same saved layout will underlie both review modes. These roles represent distinct design modalities: AR is functional in the current system, while VR review is retained as planned future work.

### E. Conceptual Data Flow
A minimal data model represents the project, BIM entities, validation outcomes, and review events. The project record stores identifier, plot dimensions, and regional location. The BIM model stores hierarchical level and element arrays. The validation service returns rule outcomes, while the QTO service outputs net material quantities and priced Bills of Quantities (BOQ). Fig. 2 illustrates this entity flow.

```
+--------------------------------------------------------------------------------------------------+
|                     Fig. 2. Conceptual Data Model and Service Flow                               |
|                                                                                                  |
|   +-------------------+       +--------------------+       +---------------------+               |
|   |   Project Model   | ----> |   BIM Model JSON   | ----> |  Validation Service |               |
|   | (ID, Plot, City)  |       | (Elements, Levels) |       | (Boundaries, Clear) |               |
|   +-------------------+       +--------------------+       +---------------------+               |
|                                         |                             |                          |
|                     +-------------------+-------------------+         |                          |
|                     |                                       |         v                          |
|                     v                                       v   +--------------------+           |
|            +-----------------+                    +-----------+ | 3D / AR Viewports  |           |
|            |   QTO Service   |                    | 5D Costing| | (Three.js, WebXR)  |           |
|            | (IS 1200/IS 456)|                    |  Service  | +--------------------+           |
|            +-----------------+                    +-----------+                                  |
|                     |                                   |                                        |
|                     v                                   v                                        |
|            +-----------------+                    +-----------+                                  |
|            |  Priced BOQ     |                    | Regional  |                                  |
|            |  Spreadsheet    |                    | Rate DB   |                                  |
|            +-----------------+                    +-----------+                                  |
+--------------------------------------------------------------------------------------------------+
```

---

## V. PROJECT STATUS AND IMPLEMENTATION DIRECTION

The current implementation status encompasses a fully operational, integrated full-stack application. The frontend, backend, database persistence, and Augmented Reality (AR) viewing components are implemented and working. The Virtual Reality (VR) immersive walkthrough represents planned future work.

For complete clarity, this paper uses:
- **“Implemented”** for functionality currently present, tested, and working in the codebase (frontend application, FastAPI backend, SQLite database, Three.js 3D viewer, IS 1200 QTO engine, 5D cost model, and WebXR AR surface tracking).
- **“Partially implemented”** for capabilities functioning in prototype form but undergoing active user refinement (e.g., advanced multi-room manual wall-splitting).
- **“Proposed” / “Planned”** for features designed in the framework but reserved for future milestones (specifically, immersive headset VR walkthroughs, multi-user avatar synchronization, and automated IFC export).

### A. Suggested Component Boundaries
The implemented system is organized across four distinct architectural tiers:
1. *Presentation Tier*: Built using Next.js 15 (React 19, TypeScript, Tailwind CSS). It hosts the project dashboard, house configurator, interactive Three.js 3D canvas, and WebXR session manager.
2. *Application Tier*: Powered by Python 3.14 and FastAPI. It exposes RESTful endpoints for user authentication (JWT with salted hashing), parametric BIM extrusion, automated QTO calculation, and 5D cost evaluation.
3. *Domain Logic & QTO Engines*: Calculates structural concrete ($0.40\text{ m}^3/\text{m}^2$, IS 456 [18]), Fe500D steel ($40.0\text{ kg}/\text{m}^2$, SP 34), brick masonry ($211\text{ bricks/m}^2$, IS 1200 Part III [17]), cement bags ($4.4\text{ bags/m}^2$), M-Sand ($0.58\text{ m}^3/\text{m}^2$), vitrified floor tiles, and interior paint.
4. *Data Persistence Tier*: SQLite (`smartbuild.db`) storing user accounts, project parameters, BIM models, and municipal material rate schedules across Bengaluru, Chennai, Hyderabad, Delhi NCR, and Mumbai.

Fig. 3 illustrates the component boundaries and verified integration points.

```
+--------------------------------------------------------------------------------------------------+
|                     Fig. 3. Implemented System Component Boundaries                             |
|                                                                                                  |
|   +------------------------------------------------------------------------------------------+   |
|   | PRESENTATION LAYER (Implemented - Next.js 15 / Three.js / WebXR AR)                      |   |
|   | - Project Dashboard & 10 Pre-Engineered House Templates                                  |   |
|   | - Interactive 3D BIM Viewer with Bounding Box Collision Wireframes                       |   |
|   | - WebXR AR Session Manager (Plane Detection, 1:20 Miniature & 1:1 Scale)                 |   |
|   | - Tabular QTO, BOQ Spreadsheet, and 5D Cost Breakdown Views                              |   |
|   +------------------------------------------------------------------------------------------+   |
|                                               | (HTTP / REST API / JSON)                         |
|                                               v                                                  |
|   +------------------------------------------------------------------------------------------+   |
|   | APPLICATION SERVICES (Implemented - FastAPI Python Backend)                              |   |
|   | - Auth Service (JWT Token Creation & Validation)                                         |   |
|   | - BIM Extrusion Service (Parametric Wall, Floor, Opening Generation)                     |   |
|   | - Quantity Service (IS 1200 Opening Deductions & Party-Wall Deduplication)               |   |
|   | - 5D Cost Service (Municipal PWD Rate Matrix: BLR, MAA, HYD, DEL, BOM)                   |   |
|   +------------------------------------------------------------------------------------------+   |
|                        |                                             |                           |
|                        v                                             v                           |
|   +---------------------------------------+     +--------------------------------------------+   |
|   | PERSISTENCE (Implemented - SQLite)    |     | PLANNED EXTENSIONS (Future Work)           |   |
|   | - Users, Projects, BIM Geometry JSON  |     | - Immersive 6-DoF Headset VR Walkthrough   |   |
|   | - Municipal District Schedule of Rates|     | - Multi-User Collaborative Design Review   |   |
|   +---------------------------------------+     +--------------------------------------------+   |
+--------------------------------------------------------------------------------------------------+
```

### B. Development Sequence
The project followed an incremental development sequence:
1. *Baseline Verification*: Initialized the full-stack architecture, establishing clean API contracts between Next.js and FastAPI.
2. *Parametric BIM & QTO Module*: Codified structural standards (IS 1200 / IS 456) to extract physical quantities directly from 3D room objects.
3. *5D Cost Model*: Integrated localized PWD rates for five major Indian metropolitan centers.
4. *Augmented Reality Integration*: Implemented WebXR surface tracking, enabling in-browser AR model placement on supported mobile devices without native app downloads.
5. *VR Roadmap*: Preserved the unified data model so that when headset VR review is integrated in a subsequent stage, it consumes the identical design state.

---

## VI. PROPOSED EVALUATION

Evaluation will be executed using the completed functional prototype. The primary comparison evaluates task performance between a conventional 2D/3D design editor baseline and the integrated workflow featuring explicit geometric feedback and on-site AR review. Both conditions utilize the identical room dimensions, furniture catalog, and design goals. Fig. 4 illustrates the comparative study workflow.

```
+--------------------------------------------------------------------------------------------------+
|                    Fig. 4. Proposed Comparative Evaluation Protocol                              |
|                                                                                                  |
|   [Identical Room & Plot Task]                                                                   |
|             |                                                                                    |
|             +---------------------------------+----------------------------------+               |
|             |                                                                    |               |
|             v                                                                    v               |
|   [Condition A: Baseline Editor]                               [Condition B: Proposed Workflow]  |
|   - 2D/3D Screen Controls                                      - Parametric BIM Editor           |
|   - Manual Visual Inspection                                   - Real-Time V(O) Spatial Warnings |
|   - Static Area Estimates                                      - On-Site WebXR AR Review         |
|             |                                                  - Automated IS 1200 QTO & 5D Cost |
|             |                                                                    |               |
|             +---------------------------------+----------------------------------+               |
|                                               |                                                  |
|                                               v                                                  |
|                           [Experimental Performance Evaluation]                                  |
|                           - Technical Measures: Registration Error, Latency                      |
|                           - Task Measures: Completion Time, Unresolved Violations                |
|                           - User Measures: Usability (SUS), Spatial Confidence                   |
+--------------------------------------------------------------------------------------------------+
```

A reproducible task defines room geometry ($12.0\text{ m} \times 10.0\text{ m}$ residential plot), an initial layout, and a functional objective (arranging bedroom, living, and circulation zones while preserving a minimum $0.90\text{ m}$ egress corridor). Participants receive standardized practice tasks before timed trials. Counterbalancing of condition order is employed to mitigate learning effects.

### A. Measures and Operational Definitions
- *Technical Measures*: AR spatial registration drift (measured in centimeters against physical ground markers), API round-trip latency (measured in milliseconds), and WebGL rendering frame rate (FPS).
- *Task Measures*: Total task completion time (seconds), number of object repositioning actions, count of unresolved bounding-box collisions, and quantity take-off variance relative to manual engineering benchmarks.
- *User Measures*: Perceived usability measured via the System Usability Scale (SUS), spatial comprehension confidence rating (5-point Likert scale), and subjective cognitive load.

### B. Analysis and Reporting Plan
Analysis begins with data completeness and device compatibility verification. Descriptive statistics (mean, median, standard deviation) will summarize task times, collision counts, and SUS scores across conditions. Pairwise statistical tests (e.g., Wilcoxon signed-rank test for non-parametric Likert data; paired t-tests for completion times following normality verification) will assess significant differences between Condition A and Condition B. Protocol deviations and hardware limitations will be fully documented.

### C. Ethics and Data Handling
Participant evaluations will adhere to institutional ethical guidelines. No raw camera feeds or personal environment scans leave the client device; all AR plane-detection processing occurs strictly within the sandboxed browser runtime. User project configurations are stored under authenticated identifiers, and participants may discontinue sessions at any time without penalty.

---

## VII. LIMITATIONS AND FUTURE WORK

### A. Threats to Validity
1. *Construct Validity*: Spatial confidence and perceived fit are assessed via structured subjective scales; while standardized, they reflect subjective perception rather than long-term habitability.
2. *Internal Validity*: Performance may be influenced by participant prior familiarity with 3D modeling tools or mobile AR interfaces. Pre-study familiarity surveys and counterbalanced task ordering address this threat.
3. *External Validity*: The current prototype evaluation focuses on residential footprints up to $300\text{ m}^2$; complex multi-storey commercial towers with curved geometry require further algorithmic extensions.
4. *Substructure Assumptions*: Cost models assume standard soil bearing capacity ($150\text{ kN/m}^2$); site-specific geotechnical conditions (e.g., black cotton soil or deep pile requirements) require manual substructure adjustment.

### B. Future Work Priorities
1. *Virtual Reality Implementation*: The foremost development priority is completing the planned VR walkthrough mode, enabling 6-DoF headset exploration of interior sightlines.
2. *Automated IFC Export*: Serializing models to Industry Foundation Classes (IFC 4.3) for seamless interoperability with structural analysis suites (ETABS, STAAD.Pro).
3. *Multi-User Collaboration*: Enabling synchronized multi-user WebXR sessions for remote client-architect design reviews.
4. *Embodied Carbon Assessment*: Linking material quantities with life-cycle assessment (LCA) databases to evaluate environmental impact alongside financial cost.

---

## VIII. CONCLUSION

This paper presented an integrated, spatially grounded framework for residential design visualization, geometric validation, and preliminary cost planning. By coupling an editable parametric BIM model with browser-native WebXR AR surface tracking, automated IS 1200 / IS 456 Quantity Take-Off, and multi-region 5D costing, the system overcomes the longstanding fragmentation between visual planning tools and construction engineering economics.

The project successfully demonstrates that a lightweight, full-stack web architecture (Next.js 15, Three.js, Python FastAPI, SQLite) can deliver responsive 3D visualization, interactive on-site AR, and mathematically standard-compliant quantity calculations without proprietary desktop suites or runtime software licensing costs. While Virtual Reality (VR) walkthrough functionality remains planned future work, the completed frontend, backend, and AR components establish a verified, inspectable foundation for intelligent residential design support.

---

## ACKNOWLEDGMENT
The authors acknowledge the public product documentation and scholarly publications cited in this paper. The authors express their sincere gratitude to the faculty project advisors, reviewers, and the Department of Computer Science and Engineering, School of Engineering, Presidency University, Bengaluru, for their computational infrastructure, academic support, and guidance throughout this project.

---

## REFERENCES
1. D. Sornoza-Parrales, G. O. Baque Vera, M. L. Parrales Poveda, and T. M. Macías-Parrales, “Evolution and research trends in virtual reality and augmented reality technologies for the architecture, engineering, and construction (AEC) industry: A systematic review and science mapping approach,” *Front. Virtual Real.*, vol. 6, Art. no. 1725875, 2025, doi: 10.3389/frvir.2025.1725875.
2. H. Elbadawy and A. Farouk, “Implementation of virtual reality technology in architecture field, and education: A review,” *Arch. Comput. Methods Eng.*, vol. 32, pp. 2457–2465, 2025, doi: 10.1007/s11831-025-10225-8.
3. Matterport, “3D camera and virtual tour platform.” [Online]. Available: `https://go.matterport.com/PS_EN.html`. Accessed: Oct. 23, 2026.
4. IKEA India, “How do I scan a room on IKEA Kreativ?” [Online]. Available: `https://www.ikea.com/in/en/customer-service/knowledge/articles/05de8ed1-e3ac-4ca1-9871-6df0119e0667.html`. Accessed: Oct. 23, 2026.
5. Planner 5D, “Floor plan software: Design 2D and 3D layouts with ease.” [Online]. Available: `https://planner5d.com/use/floor-plan-software`. Accessed: Oct. 23, 2026.
6. Planner 5D Help Center, “AR with Planner 5D (iOS).” [Online]. Available: `https://support.planner5d.com/en/articles/5897583-ar-with-planner-5d-ios`. Accessed: Oct. 23, 2026.
7. Apple, “RoomPlan,” *Apple Developer Documentation*. [Online]. Available: `https://developer.apple.com/documentation/roomplan`. Accessed: Oct. 23, 2026.
8. M. Mauri, G. Rancati, G. Riva, and A. Gaggioli, “Comparing the effects of immersive and non-immersive real estate experience on behavioral intentions,” *Comput. Human Behav.*, vol. 150, Art. no. 107996, 2024, doi: 10.1016/j.chb.2023.107996.
9. J. H. Ma, E. Erdogmus, S. Kangisser, and E. Yang, “A comparative analysis of the effectiveness of immersive virtual reality on end-user design review,” *Build. Environ.*, vol. 267, Art. no. 112237, 2025, doi: 10.1016/j.buildenv.2024.112237.
10. M. Açıkel, H. T. Örmecioğlu, and İ. Erbaş, “Mapping the literature: A systematic review of VR technology in architecture,” *Open House Int.*, vol. 51, no. 2, pp. 210–232, 2026, doi: 10.1108/OHI-09-2024-0314.
11. J. K. T. Tang, W. M. Lau, K. K. Chan, and K. H. To, “AR interior designer: Automatic furniture arrangement using spatial and functional relationships,” in *Proc. 20th Int. Conf. Virtual Syst. Multimedia (VSMM)*, 2014, pp. 345–352, doi: 10.1109/VSMM.2014.7136652.
12. M. Macchiaroli, L. Dolores, and G. De Mare, “Extended reality (XR) impacts on real estate market and economic-financial project management of residential built environments: Systematic review and analysis,” *Smart Sustain. Built Environ.*, 2025, doi: 10.1108/SASBE-07-2024-0258.
13. H. Fan, Y. Wang, H. Fang, Y. Gui, and Y. Song, “Immersive virtual reality technology in architecture: A comprehensive review and framework for HMD and CAVE applications,” *Virtual Reality*, vol. 30, Art. no. 161, 2026, doi: 10.1007/s10055-026-01421-3.
14. E. Yigitbas, K. Nowosad, and G. Engels, “Supporting construction and architectural visualization through BIM and AR/VR: A systematic literature review,” *Autom. Constr.*, vol. 152, Art. no. 104932, 2023, doi: 10.1016/j.autcon.2023.104932.
15. A. Sidani, B. Dinis, J. Duarte, J. P. Santos, and R. Martins, “Recent tools and techniques of BIM-Based Augmented Reality: A systematic review,” *Arch. Comput. Methods Eng.*, vol. 28, no. 7, pp. 3501–3522, 2021, doi: 10.1007/s11831-020-09516-7.
16. H. Jafary and O. Shojaei, “Artificial intelligence (AI) and machine learning in building information modeling (BIM)-based construction cost estimation: A systematic review,” *J. Build. Eng.*, vol. 84, Art. no. 108520, 2026, doi: 10.1016/j.jobe.2024.108520.
17. Bureau of Indian Standards, *IS 1200: Methods of Measurement of Building and Civil Engineering Works (Parts I to XXVIII)*, New Delhi: BIS, 2020.
18. Bureau of Indian Standards, *IS 456: Plain and Reinforced Concrete - Code of Practice*, Fourth Revision, New Delhi: BIS, 2021.
19. W3C, “WebXR Device API,” *W3C Working Group Note*, World Wide Web Consortium, 2024. [Online]. Available: `https://www.w3.org/TR/webxr/`.
20. P. A. Risi and C. R. Couper, “Evaluating spatial awareness and scale perception in browser-based augmented reality for interior design,” *J. Archit. Eng.*, vol. 30, no. 3, Art. no. 04024018, 2024, doi: 10.1061/JAEIED.AEENG-1628.
