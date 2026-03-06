"""
modal_app.py - Modal serverless GPU deployment for ImageForge AI Service.
"""
import asyncio
import logging
from modal import Image, App, Volume, Secret, asgi_app

app = App("imageforge-ai-service")
lora_vol = Volume.from_name("imageforge-lora-vol", create_if_missing=True)

# Define the Modal Image
image = (
    Image.debian_slim(python_version="3.11")
    .pip_install(
        "fastapi==0.115.8",
        "uvicorn[standard]==0.34.0",
        "httpx==0.28.1",
        "pydantic==2.11.2",
        "pydantic-settings==2.9.1",
        "boto3==1.38.16",
        "Pillow==11.2.1",
        "torch==2.5.1",
        "diffusers==0.33.1",
        "transformers==4.46.3",
        "accelerate==1.1.1",
        "safetensors==0.4.5"
    )
)

# Bake the SDXL base model into the image to avoid 6GB downloads on cold start
def download_models():
    from diffusers import StableDiffusionXLPipeline
    import torch
    # Download and cache the model weights during the build step
    StableDiffusionXLPipeline.from_pretrained(
        "stabilityai/stable-diffusion-xl-base-1.0",
        torch_dtype=torch.float16,
        use_safetensors=True
    )

image = (
    image.run_function(download_models)
    .add_local_dir("./app", remote_path="/root/app")
)

# Background generation task runs on A10G GPU
@app.function(
    image=image,
    gpu="A10G",
    volumes={"/app/lora_weights": lora_vol},
    secrets=[Secret.from_name("imageforge-r2-secret")],
    timeout=600  # 10 minutes max for generation and upload
)
def process_generation_modal(request_dict: dict):
    from app.models import GenerateRequest
    from app.main import process_generation
    
    req = GenerateRequest(**request_dict)
    
    # process_generation uses asyncio, so we need to run it in the Modal function's event loop
    asyncio.run(process_generation(req))

# Web endpoint runs as an ASGI app
@app.function(
    image=image,
    secrets=[Secret.from_name("imageforge-r2-secret")],
    volumes={"/app/lora_weights": lora_vol},
    min_containers=1 # Keep at least one web instance warm for fast response
)
@asgi_app()
def fastapi_app():
    from app.main import app as web_app
    from app.models import GenerateRequest
    from fastapi import BackgroundTasks, HTTPException
    
    logger = logging.getLogger(__name__)

    # Overwrite the generate route to spawn the Modal background function
    # Find and remove existing route for /api/generate
    routes_to_keep = []
    for r in web_app.router.routes:
        if getattr(r, "path", None) == "/api/generate" and getattr(r, "methods", set()) == {"POST"}:
            continue
        routes_to_keep.append(r)
    web_app.router.routes = routes_to_keep

    @web_app.post("/api/generate")
    async def generate_override(request: GenerateRequest, background_tasks: BackgroundTasks):
        logger.info(
            f"Üretim isteği alındı (Modal) — PromptId: {request.prompt_id}, "
            f"Model: {request.model}"
        )
        if request.model not in ("DiscoElysium", "SlayThePrincess"):
            raise HTTPException(
                status_code=400,
                detail=f"Desteklenmeyen model: {request.model}"
            )
        
        # Fire-and-forget: spawn on a GPU modal instance
        process_generation_modal.spawn(request.model_dump())
        return {"status": "accepted", "prompt_id": request.prompt_id}

    return web_app
