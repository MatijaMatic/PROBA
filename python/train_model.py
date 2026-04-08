"""
train_model.py – Trains the delay prediction RandomForestClassifier
and saves it to delay_model.pkl.

Run once before starting the FastAPI server:
    python train_model.py
"""

import os
import pickle
import numpy as np
import pandas as pd
from sklearn.ensemble import RandomForestClassifier
from sklearn.model_selection import train_test_split
from sklearn.preprocessing import LabelEncoder
from sklearn.metrics import accuracy_score, classification_report

# ---------------------------------------------------------------------------
# 1. Generate synthetic training dataset
# ---------------------------------------------------------------------------

np.random.seed(42)
N = 2000

CARRIERS = ["DHL", "FedEx", "DB Cargo", "Schenker", "Kuehne+Nagel"]
CITIES = [
    "Beograd", "Novi Sad", "Niš", "Subotica", "Šabac",
    "Zrenjanin", "Leskovac", "Vranje", "Valjevo", "Loznica",
    "Pančevo", "Sombor", "Kikinda", "Pirot", "Bar",
]
SEASONS = ["Spring", "Summer", "Autumn", "Winter"]
WEATHER = ["Clear", "Rain", "Snow", "Fog", "Storm"]
CARGO_TYPES = ["General", "Food", "Chemicals", "Electronics", "Machinery", "Pharmaceuticals"]

carriers = np.random.choice(CARRIERS, N)
routes_from = np.random.choice(CITIES, N)
routes_to = np.random.choice(CITIES, N)
departure_hours = np.random.randint(0, 24, N)
seasons = np.random.choice(SEASONS, N)
weather = np.random.choice(WEATHER, N)
cargo_types = np.random.choice(CARGO_TYPES, N)

# Simulate delay probability based on features
delay_prob = 0.15 * np.ones(N)

# Bad weather increases delay
delay_prob += np.where(np.isin(weather, ["Rain", "Fog"]), 0.15, 0)
delay_prob += np.where(np.isin(weather, ["Snow", "Storm"]), 0.30, 0)

# Night departures (22-06) more likely to be delayed
delay_prob += np.where((departure_hours >= 22) | (departure_hours <= 6), 0.10, 0)

# Winter/Autumn increases delay
delay_prob += np.where(np.isin(seasons, ["Winter", "Autumn"]), 0.10, 0)

# Some carriers are more reliable
delay_prob += np.where(carriers == "DB Cargo", -0.05, 0)
delay_prob += np.where(carriers == "FedEx", 0.05, 0)

# Chemicals/Machinery more complex → higher delay
delay_prob += np.where(np.isin(cargo_types, ["Chemicals", "Machinery"]), 0.05, 0)

delay_prob = np.clip(delay_prob, 0.05, 0.90)
delay = (np.random.rand(N) < delay_prob).astype(int)

df = pd.DataFrame({
    "carrier": carriers,
    "route_from": routes_from,
    "route_to": routes_to,
    "departure_hour": departure_hours,
    "season": seasons,
    "weather_condition": weather,
    "cargo_type": cargo_types,
    "delay": delay,
})

# ---------------------------------------------------------------------------
# 2. Encode categorical features
# ---------------------------------------------------------------------------

categorical_cols = ["carrier", "route_from", "route_to", "season", "weather_condition", "cargo_type"]

encoders: dict[str, LabelEncoder] = {}
for col in categorical_cols:
    le = LabelEncoder()
    df[col + "_enc"] = le.fit_transform(df[col])
    encoders[col] = le

feature_cols = [col + "_enc" for col in categorical_cols] + ["departure_hour"]

X = df[feature_cols].values
y = df["delay"].values

# ---------------------------------------------------------------------------
# 3. Train model
# ---------------------------------------------------------------------------

X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.2, random_state=42)

model = RandomForestClassifier(
    n_estimators=200,
    max_depth=10,
    min_samples_leaf=5,
    random_state=42,
)
model.fit(X_train, y_train)

y_pred = model.predict(X_test)
acc = accuracy_score(y_test, y_pred)
print(f"Model accuracy: {acc:.3f}")
print(classification_report(y_test, y_pred))

# ---------------------------------------------------------------------------
# 4. Save model and encoders
# ---------------------------------------------------------------------------

output = {
    "model": model,
    "encoders": encoders,
    "feature_cols": feature_cols,
    "categorical_cols": categorical_cols,
}

model_path = os.path.join(os.path.dirname(__file__), "delay_model.pkl")
with open(model_path, "wb") as f:
    pickle.dump(output, f)

print(f"Model saved to {model_path}")
