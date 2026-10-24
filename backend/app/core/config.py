import os

class Settings:
    PROJECT_NAME: str = "SMARTBUILD AR/VR API"
    VERSION: str = "1.0.0"
    API_V1_STR: str = "/api"
    SECRET_KEY: str = os.getenv("SECRET_KEY", "smartbuild-secret-key-university-evaluation-2026-secure-jwt")
    ALGORITHM: str = "HS256"
    ACCESS_TOKEN_EXPIRE_MINUTES: int = 60 * 24 * 7 # 7 days

    # SQLite database default (works out of the box with ₹0 budget, supports PostgreSQL via DATABASE_URL)
    BASE_DIR = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", ".."))
    DB_PATH = os.path.join(BASE_DIR, "smartbuild.db")
    DATABASE_URL: str = os.getenv("DATABASE_URL", f"sqlite:///{DB_PATH}")

settings = Settings()
