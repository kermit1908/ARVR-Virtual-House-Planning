"""
Dataset Generation Script for Residential Construction Cost Prediction
Calibrated against Indian Standards (IS 1200 / IS 456) and CPWD DSR (Delhi Schedule of Rates)
Generates documented dataset of 1,200 residential projects.
"""

import os
import random
import numpy as np
import pandas as pd

def generate_residential_dataset(n_samples=1200, seed=42):
    random.seed(seed)
    np.random.seed(seed)

    locations = ['Bengaluru', 'Chennai', 'Hyderabad', 'Delhi', 'Mumbai']
    location_weights = [0.25, 0.20, 0.20, 0.18, 0.17]
    loc_cost_multiplier = {
        'Bengaluru': 1.00,
        'Chennai': 0.97,
        'Hyderabad': 0.94,
        'Delhi': 1.03,
        'Mumbai': 1.09
    }

    qualities = ['Standard', 'Premium', 'Luxury']
    quality_weights = [0.50, 0.35, 0.15]
    quality_multipliers = {
        'Standard': 1.00,
        'Premium': 1.25,
        'Luxury': 1.60
    }

    records = []

    for i in range(1, n_samples + 1):
        pid = f"PRJ-{i:05d}"
        location = np.random.choice(locations, p=location_weights)
        quality = np.random.choice(qualities, p=quality_weights)
        floors = int(np.random.choice([1, 2, 3], p=[0.40, 0.45, 0.15]))

        # Ground footprint and plot
        ground_area = round(np.random.uniform(50.0, 150.0), 2)
        plot_area = round(ground_area * np.random.uniform(1.2, 2.0), 2)
        builtup_area = round(ground_area * floors, 2)

        # Rooms
        if builtup_area < 80:
            bedrooms = 1
            bathrooms = 1
        elif builtup_area < 130:
            bedrooms = 2
            bathrooms = 2
        elif builtup_area < 220:
            bedrooms = 3
            bathrooms = 2 if random.random() < 0.4 else 3
        else:
            bedrooms = 4 if random.random() < 0.7 else 5
            bathrooms = 3 if random.random() < 0.5 else 4

        room_count = bedrooms + bathrooms + (1 * floors) + 1 # + living + kitchen

        # Structural engineering quantities (IS 456 / IS 1200 ratios)
        # Concrete: 0.38 - 0.45 m3 per m2 builtup
        concrete_m3 = round(builtup_area * np.random.uniform(0.38, 0.44), 2)

        # Steel: 36 - 44 kg per m2 builtup
        steel_kg = round(builtup_area * np.random.uniform(36.0, 44.0), 2)

        # Bricks: 200 - 240 bricks per m2 builtup
        brick_count = int(builtup_area * np.random.uniform(205.0, 235.0))

        # Cement bags (approx 4.5 bags per m2)
        cement_bags = int(builtup_area * np.random.uniform(4.2, 4.8))

        # Sand: 0.55 - 0.65 m3 per m2
        sand_m3 = round(builtup_area * np.random.uniform(0.55, 0.65), 2)

        # Tile area (approx 0.88 of builtup + 10% wastage)
        tile_area = round(builtup_area * np.random.uniform(0.85, 0.95), 2)

        # Wall Paint area (approx 2.6 - 3.1 times builtup)
        paint_area = round(builtup_area * np.random.uniform(2.6, 3.1), 2)

        # Base Cost per m2 (2026 reference baseline for standard grade in INR)
        # Basic civil structure ~ 14,000 - 16,500 / m2
        # Finishing & MEP ~ 6,500 - 9,000 / m2
        base_rate_per_m2 = np.random.uniform(21500.0, 24000.0)
        
        raw_base_cost = builtup_area * base_rate_per_m2
        quality_mult = quality_multipliers[quality]
        loc_mult = loc_cost_multiplier[location]

        # Add modest random market fluctuation (±3.5%)
        noise = np.random.normal(1.0, 0.035)
        total_cost = round(raw_base_cost * quality_mult * loc_mult * noise, -2) # rounded to nearest 100

        # Derived split costs for reporting
        material_cost = round(total_cost * np.random.uniform(0.58, 0.63), 2)
        labour_cost = round(total_cost * np.random.uniform(0.24, 0.28), 2)
        services_contingency = round(total_cost - (material_cost + labour_cost), 2)

        records.append({
            'project_id': pid,
            'location': location,
            'quality': quality,
            'plot_area': plot_area,
            'builtup_area': builtup_area,
            'floors': floors,
            'bedrooms': bedrooms,
            'bathrooms': bathrooms,
            'room_count': room_count,
            'concrete_quantity': concrete_m3,
            'steel_quantity': steel_kg,
            'brick_quantity': brick_count,
            'cement_quantity': cement_bags,
            'sand_quantity': sand_m3,
            'tile_area': tile_area,
            'paint_area': paint_area,
            'material_cost': material_cost,
            'labour_cost': labour_cost,
            'services_contingency': services_contingency,
            'total_cost': total_cost
        })

    df = pd.DataFrame(records)
    return df

if __name__ == '__main__':
    out_dir = os.path.join(os.path.dirname(__file__), 'data', 'raw')
    os.makedirs(out_dir, exist_ok=True)
    df = generate_residential_dataset(1200)
    out_path = os.path.join(out_dir, 'residential_construction_data.csv')
    df.to_csv(out_path, index=False)
    print(f"Generated {len(df)} project records saved to: {out_path}")
    print("Summary:")
    print(df[['builtup_area', 'floors', 'total_cost']].describe())
