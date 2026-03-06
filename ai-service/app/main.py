"""
main.py — FastAPI AI servisi ana modülü.
.NET API'den gelen üretim isteklerini alır, görsel üretir,
R2'ye yükler ve callback ile sonucu bildirir.
"""

import asyncio
import logging
import traceback
from contextlib import asynccontextmanager

import httpx
from fastapi import FastAPI, BackgroundTasks, HTTPException
from fastapi.middleware.cors import CORSMiddleware

from app.config import settings
from app.models import (
    GenerateRequest,
    CallbackPayload,
    GeneratedImage,
    HealthResponse,
)
from app.pipeline import (
    load_base_pipeline,
    generate_images,
    is_gpu_available,
    is_pipeline_loaded,
)
from app.storage import upload_image

logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s [%(levelname)s] %(name)s: %(message)s",
)
logger = logging.getLogger(__name__)


@asynccontextmanager
async def lifespan(app: FastAPI):
    """Uygulama başlangıcında bilgi logla. Model lazy load edilecek."""
    logger.info("AI servisi başlatılıyor...")
    logger.info(f"GPU mevcut: {is_gpu_available()}")
    logger.info(f"R2 Bucket: {settings.r2_bucket_name}")
    logger.info(f"R2 Endpoint: {settings.r2_endpoint}")
    logger.info(f"Base model: {settings.base_model}")
    logger.info("Model ilk üretim isteğinde lazy-load edilecek.")

    yield
    logger.info("AI servisi kapatılıyor...")


app = FastAPI(
    title="ImageForge AI Service",
    description="SDXL 1.0 + LoRA ile görsel üretim servisi",
    version="1.0.0",
    lifespan=lifespan,
)

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_methods=["*"],
    allow_headers=["*"],
)


@app.get("/health", response_model=HealthResponse)
async def health_check():
    """Sağlık kontrolü."""
    return HealthResponse(
        status="ok",
        models_loaded=is_pipeline_loaded(),
        gpu_available=is_gpu_available(),
    )


@app.post("/api/generate")
async def generate(request: GenerateRequest, background_tasks: BackgroundTasks):
    """
    Görsel üretim isteğini alır, arka planda işler.
    Üretim tamamlandığında callback_url'e POST yapar.
    """
    logger.info(
        f"Üretim isteği alındı — PromptId: {request.prompt_id}, "
        f"Model: {request.model}, ImageCount: {request.image_count}"
    )

    # Desteklenen model kontrolü
    if request.model not in ("DiscoElysium", "SlayThePrincess"):
        raise HTTPException(
            status_code=400,
            detail=f"Desteklenmeyen model: {request.model}. "
                   f"Desteklenen: DiscoElysium, SlayThePrincess"
        )

    # Arka planda işle (Modal veya Yerel)
    import os
    if os.getenv("MODAL_TOKEN_ID"):
        import modal
        logger.info("Modal backend kullanılıyor: imageforge-ai-service -> process_generation_modal")
        f = modal.Function.from_name("imageforge-ai-service", "process_generation_modal")
        f.spawn(request.model_dump())
    else:
        background_tasks.add_task(process_generation, request)

    return {"status": "accepted", "prompt_id": request.prompt_id}


async def process_generation(request: GenerateRequest):
    """Arka planda görsel üretim, R2 yükleme ve callback."""
    try:
        logger.info(f"Üretim başlıyor — PromptId: {request.prompt_id}")

        # Pipeline'ı yükle (lazy load)
        pipeline = load_base_pipeline(settings.base_model)

        # Görselleri üret (CPU-bound, thread pool'da çalıştır)
        loop = asyncio.get_event_loop()
        results = await loop.run_in_executor(
            None,
            generate_images,
            pipeline,
            request.prompt,
            request.negative_prompt,
            request.model,
            request.image_count,
            request.width,
            request.height,
            request.steps,
            request.cfg_scale,
            request.seed,
            settings.lora_dir,
        )

        # R2'ye yükle
        images = []
        for i, (image_bytes, seed) in enumerate(results):
            upload_result = upload_image(image_bytes, request.prompt_id, i)
            images.append(
                GeneratedImage(
                    url=upload_result["url"],
                    storage_key=upload_result["storage_key"],
                    file_name=upload_result["file_name"],
                    seed=seed,
                    width=request.width,
                    height=request.height,
                    file_size=upload_result["file_size"],
                )
            )

        # Başarılı callback gönder
        callback = CallbackPayload(
            prompt_id=request.prompt_id,
            status="completed",
            images=images,
        )
        await send_callback(request.callback_url, callback)

        logger.info(
            f"Üretim tamamlandı — PromptId: {request.prompt_id}, "
            f"{len(images)} görsel üretildi"
        )

    except Exception as e:
        logger.error(
            f"Üretim hatası — PromptId: {request.prompt_id}: "
            f"{traceback.format_exc()}"
        )

        # Hata callback'i gönder
        callback = CallbackPayload(
            prompt_id=request.prompt_id,
            status="failed",
            error=str(e),
        )
        try:
            await send_callback(request.callback_url, callback)
        except Exception as cb_err:
            logger.error(f"Hata callback'i gönderilemedi: {cb_err}")


async def send_callback(url: str, payload: CallbackPayload):
    """Webhook callback'i .NET API'ye gönderir."""
    logger.info(f"Callback gönderiliyor → {url}")

    async with httpx.AsyncClient(timeout=30) as client:
        response = await client.post(
            url,
            json=payload.model_dump(),
        )
        logger.info(
            f"Callback yanıtı: {response.status_code} — "
            f"PromptId: {payload.prompt_id}"
        )
        response.raise_for_status()


if __name__ == "__main__":
    import uvicorn
    uvicorn.run("app.main:app", host="0.0.0.0", port=8000, reload=True)
