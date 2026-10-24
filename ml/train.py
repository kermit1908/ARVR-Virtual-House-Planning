"""
ML Model Training and Evaluation Pipeline
Trains Linear Regression, Random Forest, and Gradient Boosting models
for BIM-based residential construction cost prediction.
Outputs metrics, comparative performance, and serialized artifacts.
"""

import os
import json
import joblib
import numpy as np
import pandas as pd
from sklearn.model_selection import train_test_split, cross_val_score
from sklearn.preprocessing import StandardScaler, OneHotEncoder
from sklearn.compose import ColumnTransformer
from sklearn.pipeline import Pipeline
from sklearn.linear_model import LinearRegression
from sklearn.ensemble import RandomForestRegressor, GradientBoostingRegressor
from sklearn.metrics import mean_absolute_error, mean_squared_error, r2_score

def train_and_evaluate_models():
    base_dir = os.path.dirname(__file__)
    data_path = os.path.join(base_dir, 'data', 'raw', 'residential_construction_data.csv')
    models_dir = os.path.join(base_dir, 'models')
    os.makedirs(models_dir, exist_ok=True)

    print(f"[ML Train] Loading dataset from: {data_path}")
    df = pd.read_csv(data_path)

    # Feature definitions
    num_features = [
        'plot_area', 'builtup_area', 'floors', 'bedrooms', 'bathrooms',
        'room_count', 'concrete_quantity', 'steel_quantity', 'brick_quantity',
        'cement_quantity', 'sand_quantity', 'tile_area', 'paint_area'
    ]
    cat_features = ['location', 'quality']
    target = 'total_cost'

    X = df[num_features + cat_features]
    y = df[target]

    # Train / Test split (80/20)
    X_train, X_test, y_train, y_test = train_test_split(
        X, y, test_size=0.20, random_state=42
    )
    print(f"[ML Train] Split: {len(X_train)} training samples, {len(X_test)} testing samples.")

    # Preprocessing
    preprocessor = ColumnTransformer(
        transformers=[
            ('num', StandardScaler(), num_features),
            ('cat', OneHotEncoder(drop='first', sparse_output=False, handle_unknown='ignore'), cat_features)
        ]
    )

    candidate_models = {
        'Linear Regression': LinearRegression(),
        'Random Forest': RandomForestRegressor(n_estimators=120, max_depth=12, random_state=42),
        'Gradient Boosting': GradientBoostingRegressor(n_estimators=150, learning_rate=0.08, max_depth=5, random_state=42)
    }

    results = {}
    best_model_name = None
    best_r2 = -float('inf')
    best_pipeline = None

    for name, model in candidate_models.items():
        pipeline = Pipeline(steps=[
            ('preprocessor', preprocessor),
            ('model', model)
        ])

        # Train
        pipeline.fit(X_train, y_train)

        # Predict
        y_pred = pipeline.predict(X_test)

        # Metrics
        mae = float(mean_absolute_error(y_test, y_pred))
        rmse = float(np.sqrt(mean_squared_error(y_test, y_pred)))
        r2 = float(r2_score(y_test, y_pred))
        mape = float(np.mean(np.abs((y_test - y_pred) / y_test)) * 100)

        # 5-fold Cross-validation on train set
        cv_scores = cross_val_score(pipeline, X_train, y_train, cv=5, scoring='r2')
        cv_r2 = float(np.mean(cv_scores))

        results[name] = {
            'mae': mae,
            'rmse': rmse,
            'r2': r2,
            'mape': mape,
            'cv_r2': cv_r2,
            'mae_formatted': f"₹{mae:,.0f}",
            'rmse_formatted': f"₹{rmse:,.0f}",
            'mape_formatted': f"{mape:.2f}%"
        }

        print(f"\n--- {name} ---")
        print(f"  R² Score:  {r2:.4f} (CV R²: {cv_r2:.4f})")
        print(f"  MAE:       ₹{mae:,.0f} ({mape:.2f}%)")
        print(f"  RMSE:      ₹{rmse:,.0f}")

        # Save individual model
        save_filename = name.lower().replace(' ', '_') + '.pkl'
        joblib.dump(pipeline, os.path.join(models_dir, save_filename))

        if r2 > best_r2:
            best_r2 = r2
            best_model_name = name
            best_pipeline = pipeline

    # Save best model
    best_path = os.path.join(models_dir, 'best_model.pkl')
    joblib.dump(best_pipeline, best_path)
    print(f"\n[ML Train] Best Model selected: {best_model_name} (R² = {best_r2:.4f})")
    print(f"[ML Train] Saved to: {best_path}")

    # Feature Importance for tree models
    feature_names = num_features.copy()
    cat_encoder = best_pipeline.named_steps['preprocessor'].named_transformers_['cat']
    encoded_cat_names = list(cat_encoder.get_feature_names_out(cat_features))
    all_feature_names = feature_names + encoded_cat_names

    feature_importances = {}
    if hasattr(best_pipeline.named_steps['model'], 'feature_importances_'):
        importances = best_pipeline.named_steps['model'].feature_importances_
        sorted_idx = np.argsort(importances)[::-1]
        for idx in sorted_idx:
            feature_importances[all_feature_names[idx]] = float(round(importances[idx] * 100, 2))

    # Save summary metadata
    metadata = {
        'best_model': best_model_name,
        'evaluation_metrics': results,
        'top_features_importance_percent': feature_importances,
        'num_features': num_features,
        'cat_features': cat_features,
        'dataset_size': len(df),
        'residual_std': float(np.std(y_test - best_pipeline.predict(X_test)))
    }

    metrics_path = os.path.join(models_dir, 'metrics.json')
    with open(metrics_path, 'w') as f:
        json.dump(metadata, f, indent=2)
    print(f"[ML Train] Metrics & metadata saved to: {metrics_path}")

if __name__ == '__main__':
    train_and_evaluate_models()
