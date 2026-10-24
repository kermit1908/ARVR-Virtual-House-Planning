import uuid
from datetime import datetime, timezone
import sqlite3
from fastapi import APIRouter, Depends, HTTPException, status
from backend.app.database.database import get_db
from backend.app.schemas.common import UserCreate, UserLogin, UserResponse, Token
from backend.app.core.security import verify_password, get_password_hash, create_access_token
from backend.app.api.deps import get_current_user

router = APIRouter(prefix="/auth", tags=["Authentication"])

@router.post("/signup", response_model=Token, status_code=status.HTTP_201_CREATED)
def signup(user_in: UserCreate, conn: sqlite3.Connection = Depends(get_db)):
    cursor = conn.cursor()
    cursor.execute("SELECT id FROM users WHERE email = ?", (user_in.email.lower(),))
    if cursor.fetchone():
        raise HTTPException(status_code=400, detail="An account with this email already exists.")

    user_id = str(uuid.uuid4())
    hashed_pwd = get_password_hash(user_in.password)
    now_str = datetime.now(timezone.utc).strftime("%Y-%m-%d %H:%M:%S")

    cursor.execute(
        "INSERT INTO users (id, email, hashed_password, full_name, role, created_at, updated_at) VALUES (?, ?, ?, ?, ?, ?, ?)",
        (user_id, user_in.email.lower(), hashed_pwd, user_in.full_name, "user", now_str, now_str)
    )
    conn.commit()

    user_resp = UserResponse(
        id=user_id,
        email=user_in.email.lower(),
        full_name=user_in.full_name,
        role="user",
        created_at=now_str
    )
    token = create_access_token(user_id)
    return Token(access_token=token, user=user_resp)

@router.post("/login", response_model=Token)
def login(login_in: UserLogin, conn: sqlite3.Connection = Depends(get_db)):
    cursor = conn.cursor()
    cursor.execute("SELECT id, email, hashed_password, full_name, role, created_at FROM users WHERE email = ?", (login_in.email.lower(),))
    row = cursor.fetchone()

    if not row or not verify_password(login_in.password, row["hashed_password"]):
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="Incorrect email or password.",
            headers={"WWW-Authenticate": "Bearer"},
        )

    user_resp = UserResponse(
        id=row["id"],
        email=row["email"],
        full_name=row["full_name"],
        role=row["role"],
        created_at=str(row["created_at"])
    )
    token = create_access_token(row["id"])
    return Token(access_token=token, user=user_resp)

@router.get("/me", response_model=UserResponse)
def get_me(current_user: dict = Depends(get_current_user)):
    return UserResponse(
        id=current_user["id"],
        email=current_user["email"],
        full_name=current_user["full_name"],
        role=current_user["role"],
        created_at=str(current_user["created_at"])
    )
