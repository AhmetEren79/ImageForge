"""
pipeline.py — SDXL 1.0 + LoRA görsel üretim pipeline'ı.
Diffusers kütüphanesi ile SDXL base model üzerine LoRA adaptörleri yükler.
İki model desteklenir: DiscoElysium ve SlayThePrincess.
"""

import io
import logging
import random
from pathlib import Path
from typing import Optional

logger = logging.getLogger(__name__)

# Lazy imports — torch ve PIL sadece ihtiyaç anında yüklenir
torch = None
Image = None

def _ensure_imports():
    global torch, Image
    if torch is None:
        import torch as _torch
        torch = _torch
    if Image is None:
        from PIL import Image as _Image
        Image = _Image


# Global pipeline referansı
_pipeline = None
_current_lora: Optional[str] = None


# LoRA model konfigürasyonları
LORA_CONFIGS = {
    "DiscoElysium": {
        "file": "DiscoElysium.safetensors",
        "trigger_words": "disco elysium style, painterly, expressive brushstrokes",
        "default_negative": "photorealistic, 3d render, smooth, blurry",
        "adapter_name": "disco_elysium",
    },
    "SlayThePrincess": {
        "file": "SlayThePrincess.safetensors",
        "trigger_words": "slay the princess style, dark ink drawing, monochrome sketch",
        "default_negative": "colorful, photorealistic, 3d render, smooth",
        "adapter_name": "slay_the_princess",
    },
}


def load_base_pipeline(base_model: str, device: str = "auto"):
    """SDXL base model'i yükler."""
    _ensure_imports()
    global _pipeline

    if _pipeline is not None:
        return _pipeline

    logger.info(f"SDXL base model yükleniyor: {base_model}")

    try:
        from diffusers import StableDiffusionXLPipeline

        if device == "auto":
            device = "cuda" if torch.cuda.is_available() else "cpu"

        dtype = torch.float16 if device == "cuda" else torch.float32

        _pipeline = StableDiffusionXLPipeline.from_pretrained(
            base_model,
            torch_dtype=dtype,
            use_safetensors=True,
            variant="fp16" if device == "cuda" else None,
        )
        _pipeline = _pipeline.to(device)

        # Bellek optimizasyonu
        if device == "cuda":
            _pipeline.enable_model_cpu_offload()

        logger.info(f"SDXL pipeline yüklendi. Device: {device}, Dtype: {dtype}")
        return _pipeline

    except Exception as e:
        logger.error(f"SDXL pipeline yükleme hatası: {e}")
        raise


def load_lora(pipeline, model_name: str, lora_dir: str):
    """Belirtilen model için LoRA ağırlıklarını yükler."""
    global _current_lora

    if _current_lora == model_name:
        logger.info(f"LoRA zaten yüklü: {model_name}")
        return

    config = LORA_CONFIGS.get(model_name)
    if not config:
        raise ValueError(f"Bilinmeyen model: {model_name}. Desteklenen: {list(LORA_CONFIGS.keys())}")

    lora_path = Path(lora_dir) / config["file"]

    if not lora_path.exists():
        logger.warning(
            f"LoRA dosyası bulunamadı: {lora_path}. "
            f"Base model ile devam ediliyor (LoRA olmadan)."
        )
        _current_lora = model_name
        return

    logger.info(f"LoRA yükleniyor: {model_name} ({lora_path})")

    try:
        # Önceki LoRA'yı kaldır
        if _current_lora is not None:
            try:
                pipeline.unload_lora_weights()
            except Exception:
                pass

        pipeline.load_lora_weights(
            str(lora_path.parent),
            weight_name=config["file"],
            adapter_name=config["adapter_name"],
        )
        pipeline.set_adapters([config["adapter_name"]], adapter_weights=[0.8])
        _current_lora = model_name
        logger.info(f"LoRA yüklendi: {model_name}")

    except Exception as e:
        logger.error(f"LoRA yükleme hatası: {e}")
        _current_lora = model_name  # Base model ile devam


def generate_images(
    pipeline,
    prompt: str,
    negative_prompt: Optional[str],
    model_name: str,
    image_count: int,
    width: int,
    height: int,
    steps: int,
    cfg_scale: float,
    seed: Optional[int],
    lora_dir: str,
) -> list[tuple[bytes, int]]:
    """
    SDXL + LoRA ile görsel üretir.
    Returns: list of (image_bytes, seed) tuples
    """
    _ensure_imports()
    # LoRA yükle
    load_lora(pipeline, model_name, lora_dir)

    config = LORA_CONFIGS.get(model_name, {})
    trigger = config.get("trigger_words", "")
    default_neg = config.get("default_negative", "")

    # Prompt'a trigger words ekle
    full_prompt = f"{trigger}, {prompt}" if trigger else prompt
    full_negative = negative_prompt or default_neg

    results = []

    for i in range(image_count):
        # Seed belirle
        img_seed = seed if seed is not None else random.randint(0, 2**32 - 1)
        if seed is not None and i > 0:
            img_seed = seed + i  # Farklı seed'ler

        generator = torch.Generator(device=pipeline.device).manual_seed(img_seed)

        logger.info(
            f"Görsel üretiliyor [{i+1}/{image_count}] — "
            f"Model: {model_name}, Seed: {img_seed}, Steps: {steps}"
        )

        try:
            output = pipeline(
                prompt=full_prompt,
                negative_prompt=full_negative,
                width=width,
                height=height,
                num_inference_steps=steps,
                guidance_scale=cfg_scale,
                generator=generator,
            )

            image: Image.Image = output.images[0]

            # PNG olarak kaydet
            buf = io.BytesIO()
            image.save(buf, format="PNG", optimize=True)
            image_bytes = buf.getvalue()

            results.append((image_bytes, img_seed))
            logger.info(f"Görsel üretildi [{i+1}/{image_count}] — {len(image_bytes)} bytes")

        except Exception as e:
            logger.error(f"Görsel üretim hatası [{i+1}/{image_count}]: {e}")
            raise

    return results


def is_gpu_available() -> bool:
    try:
        _ensure_imports()
        return torch.cuda.is_available()
    except Exception:
        return False


def is_pipeline_loaded() -> bool:
    return _pipeline is not None
