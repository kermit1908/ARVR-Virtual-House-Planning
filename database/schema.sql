-- SMARTBUILD AR/VR Database Schema
-- Compatible with PostgreSQL and SQLite

CREATE TABLE IF NOT EXISTS users (
    id VARCHAR(36) PRIMARY KEY,
    email VARCHAR(255) UNIQUE NOT NULL,
    hashed_password VARCHAR(255) NOT NULL,
    full_name VARCHAR(255) NOT NULL,
    role VARCHAR(50) DEFAULT 'user',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS projects (
    id VARCHAR(36) PRIMARY KEY,
    owner_id VARCHAR(36) NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    location VARCHAR(100) NOT NULL DEFAULT 'Bengaluru',
    plot_width NUMERIC(8,2) NOT NULL DEFAULT 12.0,
    plot_length NUMERIC(8,2) NOT NULL DEFAULT 15.0,
    plot_area NUMERIC(10,2) NOT NULL DEFAULT 180.0,
    status VARCHAR(50) DEFAULT 'draft',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS bim_models (
    id VARCHAR(36) PRIMARY KEY,
    project_id VARCHAR(36) UNIQUE NOT NULL REFERENCES projects(id) ON DELETE CASCADE,
    floors_count INT NOT NULL DEFAULT 1,
    floor_height NUMERIC(5,2) NOT NULL DEFAULT 3.0,
    wall_thickness NUMERIC(5,2) NOT NULL DEFAULT 0.23,
    builtup_area NUMERIC(10,2) NOT NULL DEFAULT 0.0,
    model_data TEXT NOT NULL, -- Full BIM structured JSON
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS material_rates (
    id VARCHAR(36) PRIMARY KEY,
    location VARCHAR(100) NOT NULL,
    category VARCHAR(100) NOT NULL,
    item_name VARCHAR(255) NOT NULL,
    unit VARCHAR(50) NOT NULL,
    unit_rate NUMERIC(10,2) NOT NULL,
    currency VARCHAR(10) DEFAULT 'INR',
    effective_date DATE DEFAULT CURRENT_DATE
);

CREATE TABLE IF NOT EXISTS project_estimates (
    id VARCHAR(36) PRIMARY KEY,
    project_id VARCHAR(36) UNIQUE NOT NULL REFERENCES projects(id) ON DELETE CASCADE,
    material_cost NUMERIC(12,2) NOT NULL,
    labour_cost NUMERIC(12,2) NOT NULL,
    finishing_cost NUMERIC(12,2) NOT NULL,
    services_cost NUMERIC(12,2) NOT NULL,
    contingency_cost NUMERIC(12,2) NOT NULL,
    total_rule_based_cost NUMERIC(12,2) NOT NULL,
    ml_predicted_cost NUMERIC(12,2),
    ml_confidence_lower NUMERIC(12,2),
    ml_confidence_upper NUMERIC(12,2),
    qto_data TEXT,
    boq_data TEXT,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS project_schedules (
    id VARCHAR(36) PRIMARY KEY,
    project_id VARCHAR(36) UNIQUE NOT NULL REFERENCES projects(id) ON DELETE CASCADE,
    total_duration_days INT NOT NULL,
    start_date DATE NOT NULL,
    end_date DATE NOT NULL,
    tasks_json TEXT NOT NULL,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_projects_owner ON projects(owner_id);
CREATE INDEX IF NOT EXISTS idx_rates_loc ON material_rates(location, category);
