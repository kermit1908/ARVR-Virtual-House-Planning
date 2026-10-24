import uuid
import pytest
from fastapi.testclient import TestClient
from backend.app.main import app

client = TestClient(app)

def test_health_check():
    response = client.get("/api/health")
    assert response.status_code == 200
    assert response.json()["status"] == "healthy"

def test_auth_and_project_lifecycle():
    # 1. Signup unique user
    unique_email = f"architect_{uuid.uuid4().hex[:8]}@smartbuild.edu"
    signup_payload = {
        "email": unique_email,
        "password": "StrongPassword123!",
        "full_name": "Dr. Sarah Chen"
    }
    signup_res = client.post("/api/auth/signup", json=signup_payload)
    assert signup_res.status_code == 201
    token = signup_res.json()["access_token"]
    headers = {"Authorization": f"Bearer {token}"}

    # 2. Verify /api/auth/me
    me_res = client.get("/api/auth/me", headers=headers)
    assert me_res.status_code == 200
    assert me_res.json()["email"] == unique_email

    # 3. Create Project
    proj_payload = {
        "name": "Eco-Villa Prototype",
        "description": "2-Storey Sustainable Residential Villa",
        "location": "Bengaluru",
        "plot_width": 15.0,
        "plot_length": 20.0
    }
    proj_res = client.post("/api/projects", json=proj_payload, headers=headers)
    assert proj_res.status_code == 201
    proj_id = proj_res.json()["id"]
    assert proj_res.json()["plot_area"] == 300.0

    # 4. Generate BIM Building Model
    bim_config = {
        "floors_count": 2,
        "floor_height": 3.0,
        "wall_thickness": 0.23,
        "quality": "Standard",
        "floors": [
            {
                "level": 0,
                "height": 3.0,
                "rooms": [
                    {"name": "Living Room", "type": "LivingRoom", "width": 6.0, "length": 4.5},
                    {"name": "Kitchen", "type": "Kitchen", "width": 3.5, "length": 3.0}
                ]
            },
            {
                "level": 1,
                "height": 3.0,
                "rooms": [
                    {"name": "Master Bed", "type": "Bedroom", "width": 4.5, "length": 4.0},
                    {"name": "Guest Bed", "type": "Bedroom", "width": 3.5, "length": 3.5}
                ]
            }
        ]
    }
    bim_res = client.post(f"/api/projects/{proj_id}/generate-house", json=bim_config, headers=headers)
    assert bim_res.status_code == 200
    bim_data = bim_res.json()
    assert bim_data["builtup_area"] > 50.0
    assert len(bim_data["elements"]) > 0

    # 5. Get Quantities & BOQ
    qto_res = client.get(f"/api/projects/{proj_id}/quantities", headers=headers)
    assert qto_res.status_code == 200
    assert len(qto_res.json()["items"]) >= 5

    boq_res = client.get(f"/api/projects/{proj_id}/boq", headers=headers)
    assert boq_res.status_code == 200
    assert boq_res.json()["grand_total"] > 500000.0

    # 6. Run AI Cost Prediction
    ai_res = client.post(f"/api/projects/{proj_id}/ai-predict", headers=headers)
    assert ai_res.status_code == 200
    assert ai_res.json()["predicted_cost"] > 1000000.0
    assert ai_res.json()["confidence_upper"] > ai_res.json()["confidence_lower"]

    # 7. Get Cost Optimization
    opt_res = client.get(f"/api/projects/{proj_id}/optimize-cost", headers=headers)
    assert opt_res.status_code == 200
    assert len(opt_res.json()["recommendations"]) > 0
    assert opt_res.json()["total_potential_savings"] > 0

    # 8. Get 4D Construction Schedule
    sched_res = client.get(f"/api/projects/{proj_id}/schedule", headers=headers)
    assert sched_res.status_code == 200
    assert sched_res.json()["total_duration_days"] > 60
    assert len(sched_res.json()["tasks"]) == 11

    # 9. Get Full Project Report
    rep_res = client.get(f"/api/projects/{proj_id}/report", headers=headers)
    assert rep_res.status_code == 200
    assert "project" in rep_res.json()
    assert "academic_evaluation" in rep_res.json()

def test_new_project_has_immediate_working_modules():
    # Verify that a newly created project immediately has active BIM, QTO, Cost, AI, Schedule
    unique_email = f"auto_{uuid.uuid4().hex[:8]}@smartbuild.edu"
    signup_res = client.post("/api/auth/signup", json={
        "email": unique_email,
        "password": "Password123!",
        "full_name": "Immediate Verification User"
    })
    assert signup_res.status_code == 201
    headers = {"Authorization": f"Bearer {signup_res.json()['access_token']}"}

    # Create project
    proj_res = client.post("/api/projects", json={
        "name": "Immediate Working Villa",
        "description": "Test immediate active state",
        "location": "Bengaluru",
        "plot_width": 12.0,
        "plot_length": 15.0
    }, headers=headers)
    assert proj_res.status_code == 201
    pid = proj_res.json()["id"]

    # Verify BIM model immediately available
    bim_res = client.get(f"/api/projects/{pid}/bim", headers=headers)
    assert bim_res.status_code == 200
    assert len(bim_res.json()["elements"]) > 0

    # Verify QTO immediately available
    qto_res = client.get(f"/api/projects/{pid}/quantities", headers=headers)
    assert qto_res.status_code == 200

    # Verify Cost Estimate immediately available
    cost_res = client.get(f"/api/projects/{pid}/cost-estimate", headers=headers)
    assert cost_res.status_code == 200

    # Verify Schedule immediately available
    sched_res = client.get(f"/api/projects/{pid}/schedule", headers=headers)
    assert sched_res.status_code == 200
