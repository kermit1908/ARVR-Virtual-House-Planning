import os
import sqlite3
from typing import Generator
from backend.app.core.config import settings

_db_initialized = False

def get_db_connection() -> sqlite3.Connection:
    global _db_initialized
    if not _db_initialized:
        init_db()
        _db_initialized = True

    conn = sqlite3.connect(settings.DB_PATH, check_same_thread=False)
    conn.row_factory = sqlite3.Row
    return conn

def get_db() -> Generator[sqlite3.Connection, None, None]:
    conn = get_db_connection()
    try:
        yield conn
    finally:
        conn.close()

def init_db():
    """Initializes the database using schema.sql and seed.sql if needed."""
    base_dir = settings.BASE_DIR
    schema_path = os.path.join(base_dir, "database", "schema.sql")
    seed_path = os.path.join(base_dir, "database", "seed.sql")

    conn = sqlite3.connect(settings.DB_PATH, check_same_thread=False)
    cursor = conn.cursor()

    if os.path.exists(schema_path):
        with open(schema_path, "r") as f:
            schema_sql = f.read()
        cursor.executescript(schema_sql)
        conn.commit()

    # Check if material_rates is empty
    cursor.execute("SELECT COUNT(*) FROM material_rates")
    count = cursor.fetchone()[0]

    if count == 0 and os.path.exists(seed_path):
        with open(seed_path, "r") as f:
            seed_sql = f.read()
        cursor.executescript(seed_sql)
        conn.commit()
        print("[DB Init] Seeded initial reference material rates.")

    conn.close()
    print(f"[DB Init] Database initialized at: {settings.DB_PATH}")
