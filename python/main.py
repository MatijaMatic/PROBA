"""
main.py – FastAPI ML service for delay prediction.

Setup:
    pip install -r requirements.txt
    python train_model.py    # trains and saves delay_model.pkl
    uvicorn main:app --reload --port 8000

Endpoints:
    GET  /health       – health check
    POST /predict      – predict delay probability
"""

import os
import pickle
import logging
from contextlib import asynccontextmanager

import numpy as np
from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel, Field

# ---------------------------------------------------------------------------
# Logging
# ---------------------------------------------------------------------------

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

# ---------------------------------------------------------------------------
# Model loading
# ---------------------------------------------------------------------------

MODEL_PATH = os.path.join(os.path.dirname(__file__), "delay_model.pkl")

_model_data: dict = {}


def load_model() -> None:
    if not os.path.exists(MODEL_PATH):
        raise FileNotFoundError(
            f"Model file not found at '{MODEL_PATH}'. "
            "Run 'python train_model.py' first."
        )
    with open(MODEL_PATH, "rb") as f:
        data = pickle.load(f)
    _model_data.update(data)
    logger.info("Model loaded from %s", MODEL_PATH)


# ---------------------------------------------------------------------------
# App lifecycle
# ---------------------------------------------------------------------------


@asynccontextmanager
async def lifespan(app: FastAPI):
    load_model()
    yield


app = FastAPI(
    title="AI Logistics – Delay Prediction Service",
    description="Machine learning microservice for transport delay prediction.",
    version="1.0.0",
    lifespan=lifespan,
)

# Allow ASP.NET backend to call this service
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_methods=["*"],
    allow_headers=["*"],
)


# ---------------------------------------------------------------------------
# Request / Response schemas
# ---------------------------------------------------------------------------


class PredictRequest(BaseModel):
    routeFrom: str = Field(..., example="Beograd")
    routeTo: str = Field(..., example="Novi Sad")
    carrier: str = Field(..., example="DHL")
    departureHour: str = Field(..., example="14")
    season: str = Field(..., example="Summer")
    weatherCondition: str = Field(..., example="Clear")
    cargoType: str = Field(..., example="General")


class PredictResponse(BaseModel):
    delayProbability: float = Field(..., description="Probability of delay (0–1)")
    delayPredicted: bool = Field(..., description="True if delay is predicted")
    confidence: str = Field(..., description="High / Medium / Low")
    inputFeatures: dict


# ---------------------------------------------------------------------------
# Helper: encode a single value with fallback for unseen labels
# ---------------------------------------------------------------------------


def safe_encode(encoder, value: str) -> int:
    classes = list(encoder.classes_)
    if value in classes:
        return int(encoder.transform([value])[0])
    logger.warning("Unseen label '%s', using 0 as fallback", value)
    return 0


# ---------------------------------------------------------------------------
# Endpoints
# ---------------------------------------------------------------------------


@app.get("/health")
def health():
    return {"status": "ok", "model_loaded": bool(_model_data)}


@app.post("/predict", response_model=PredictResponse)
def predict(req: PredictRequest):
    if not _model_data:
        raise HTTPException(status_code=503, detail="Model not loaded.")

    model = _model_data["model"]
    encoders = _model_data["encoders"]

    try:
        departure_hour = int(req.departureHour)
    except ValueError:
        raise HTTPException(status_code=422, detail="departureHour must be an integer (0–23).")

    # Encode categorical inputs
    enc_carrier = safe_encode(encoders["carrier"], req.carrier)
    enc_from = safe_encode(encoders["route_from"], req.routeFrom)
    enc_to = safe_encode(encoders["route_to"], req.routeTo)
    enc_season = safe_encode(encoders["season"], req.season)
    enc_weather = safe_encode(encoders["weather_condition"], req.weatherCondition)
    enc_cargo = safe_encode(encoders["cargo_type"], req.cargoType)

    features = np.array([[enc_carrier, enc_from, enc_to, enc_season, enc_weather, enc_cargo, departure_hour]])

    proba = model.predict_proba(features)[0]
    delay_proba = float(proba[1])  # probability of class 1 (delayed)
    delay_predicted = bool(delay_proba >= 0.5)

    if delay_proba >= 0.70:
        confidence = "High"
    elif delay_proba >= 0.45:
        confidence = "Medium"
    else:
        confidence = "Low"

    logger.info(
        "Prediction: %s → %s | %s | %.2f",
        req.routeFrom, req.routeTo, req.carrier, delay_proba
    )

    return PredictResponse(
        delayProbability=round(delay_proba, 4),
        delayPredicted=delay_predicted,
        confidence=confidence,
        inputFeatures={
            "routeFrom": req.routeFrom,
            "routeTo": req.routeTo,
            "carrier": req.carrier,
            "departureHour": departure_hour,
            "season": req.season,
            "weatherCondition": req.weatherCondition,
            "cargoType": req.cargoType,
        },
    )
