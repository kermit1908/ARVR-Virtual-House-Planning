import pytest
from ml.predict import get_cost_predictor
from backend.app.services.ai_service import AIService

def test_ml_predictor_loads():
    predictor = get_cost_predictor()
    assert predictor is not None
    assert predictor.pipeline is not None
    assert predictor.metadata.get("best_model") == "Gradient Boosting"

def test_ml_prediction_bounds():
    predictor = get_cost_predictor()
    sample_features = {
        "plot_area": 180.0,
        "builtup_area": 120.0,
        "floors": 1,
        "bedrooms": 2,
        "bathrooms": 2,
        "room_count": 5,
        "concrete_quantity": 48.0,
        "steel_quantity": 4800.0,
        "brick_quantity": 26400,
        "cement_quantity": 528,
        "sand_quantity": 69.6,
        "tile_area": 108.0,
        "paint_area": 336.0,
        "location": "Bengaluru",
        "quality": "Standard"
    }

    res = predictor.predict(sample_features)
    assert res["predicted_cost"] > 1500000.0 # Realistic min for 120m2 house
    assert res["confidence_lower"] < res["predicted_cost"]
    assert res["confidence_upper"] > res["predicted_cost"]
    assert res["r2_score"] > 0.95
    assert "Lakh" in res["predicted_cost_inr_formatted"] or "Cr" in res["predicted_cost_inr_formatted"]

def test_ai_service_comparison():
    comp = AIService.get_models_comparison()
    assert comp.dataset_size == 1200
    assert len(comp.models) == 3 # Linear Regression, Random Forest, Gradient Boosting
    assert comp.best_model == "Gradient Boosting"
