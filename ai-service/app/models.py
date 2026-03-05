"""
models.py — Pydantic request/response modelleri.
.NET API'den gelen istek ve webhook callback payload formatları.
"""

from pydantic import BaseModel, Field
from typing import Optional


class GenerateRequest(BaseModel):
    """NET AiGenerationService'den gelen istek."""
    prompt_id: str
    prompt: str
    negative_prompt: Optional[str] = None
    model: str  # "DiscoElysium" | "SlayThePrincess"
    image_count: int = Field(default=2, ge=1, le=3)
    width: int = 1024
    height: int = 1024
    steps: int = 30
    cfg_scale: float = 7.0
    seed: Optional[int] = None
    callback_url: str


class GeneratedImage(BaseModel):
    """Webhook callback'te gönderilen tek görsel bilgisi."""
    url: str
    storage_key: str
    file_name: str
    seed: Optional[int] = None
    width: int
    height: int
    file_size: int


class CallbackPayload(BaseModel):
    """Webhook callback payload — .NET WebhookController'a gönderilir."""
    prompt_id: str
    status: str  # "completed" | "failed"
    error: Optional[str] = None
    images: list[GeneratedImage] = []


class HealthResponse(BaseModel):
    status: str = "ok"
    models_loaded: bool = False
    gpu_available: bool = False
