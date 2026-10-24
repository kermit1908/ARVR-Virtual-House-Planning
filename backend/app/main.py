from contextlib import asynccontextmanager
from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from backend.app.core.config import settings
from backend.app.database.database import init_db
from backend.app.api import auth, projects, bim, quantities, costs, ai, optimize, schedule, reports

@asynccontextmanager
async def lifespan(app: FastAPI):
    # Startup: initialize database tables and seed reference rates
    init_db()
    yield

app = FastAPI(
    title=settings.PROJECT_NAME,
    version=settings.VERSION,
    description="AI-Enabled BIM-Based Residential Construction Visualization, Quantity Take-Off, Cost Estimation and Planning Platform API",
    lifespan=lifespan
)

# Enable CORS for local and production web clients
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Register API Routers
app.include_router(auth.router, prefix=settings.API_V1_STR)
app.include_router(projects.router, prefix=settings.API_V1_STR)
app.include_router(bim.router, prefix=settings.API_V1_STR)
app.include_router(quantities.router, prefix=settings.API_V1_STR)
app.include_router(costs.router, prefix=settings.API_V1_STR)
app.include_router(ai.router, prefix=settings.API_V1_STR)
app.include_router(optimize.router, prefix=settings.API_V1_STR)
app.include_router(schedule.router, prefix=settings.API_V1_STR)
app.include_router(reports.router, prefix=settings.API_V1_STR)

@app.get("/api/health", tags=["System"])
def health_check():
    return {
        "status": "healthy",
        "service": settings.PROJECT_NAME,
        "version": settings.VERSION,
        "environment": "production-ready"
    }
