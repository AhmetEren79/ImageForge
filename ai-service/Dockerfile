# ─── ImageForge AI Service Dockerfile ───
# SDXL + LoRA ile görsel üretim servisi (CPU build — GPU için requirements-gpu.txt kullanın)

FROM python:3.12-slim AS base

WORKDIR /app

# Sistem bağımlılıkları
RUN apt-get update && apt-get install -y --no-install-recommends \
    libgl1 libglib2.0-0 libsm6 libxext6 libxrender1 curl \
    && rm -rf /var/lib/apt/lists/*

# Python bağımlılıkları (CPU-only torch)
COPY requirements.txt .
RUN pip install --no-cache-dir \
    --extra-index-url https://download.pytorch.org/whl/cpu \
    -r requirements.txt

# Uygulama kodu
COPY app/ ./app/

# LoRA ağırlıkları dizini (mount edilecek)
RUN mkdir -p /app/lora_weights

# Ortam değişkenleri
ENV PYTHONUNBUFFERED=1
ENV LORA_DIR=/app/lora_weights

EXPOSE 8000

HEALTHCHECK --interval=30s --timeout=10s --retries=3 \
    CMD curl -f http://localhost:8000/health || exit 1

CMD ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "8000"]
