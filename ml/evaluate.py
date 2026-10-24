"""
Model Evaluation and Research Analysis Script
Generates academic comparative tables and metrics for dissertation documentation.
"""

import os
import json

def generate_evaluation_report():
    base_dir = os.path.dirname(__file__)
    metrics_path = os.path.join(base_dir, 'models', 'metrics.json')

    if not os.path.exists(metrics_path):
        print("Metrics file not found. Run train.py first.")
        return

    with open(metrics_path, 'r') as f:
        data = json.load(f)

    print("=" * 70)
    print("SMARTBUILD AR/VR - AI/ML MODEL EVALUATION REPORT")
    print("=" * 70)
    print(f"Total Dataset Size: {data.get('dataset_size')} projects")
    print(f"Best Performing Architecture: {data.get('best_model')}")
    print("-" * 70)
    print(f"{'Model Name':<22} | {'R²':<7} | {'CV R²':<7} | {'MAE (INR)':<14} | {'MAPE':<8} | {'RMSE (INR)':<14}")
    print("-" * 70)

    for model_name, metrics in data.get('evaluation_metrics', {}).items():
        print(f"{model_name:<22} | {metrics['r2']:<7.4f} | {metrics['cv_r2']:<7.4f} | "
              f"{metrics['mae_formatted']:<14} | {metrics['mape_formatted']:<8} | {metrics['rmse_formatted']:<14}")
    print("-" * 70)

    print("\nTOP FEATURE CONTRIBUTIONS (Feature Importance):")
    top_features = data.get('top_features_importance_percent', {})
    for feat, imp in list(top_features.items())[:8]:
        bar = "█" * int(imp / 2)
        print(f"  {feat:<24} : {imp:5.2f}% {bar}")

    print("=" * 70)

if __name__ == '__main__':
    generate_evaluation_report()
