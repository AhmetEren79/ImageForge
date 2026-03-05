"""
config.py — Uygulama konfigürasyonu.
.env dosyasından Cloudflare R2, API ve model ayarlarını okur.
"""

import os
from pathlib import Path
from pydantic_settings import BaseSettings
from dotenv import load_dotenv

# .env dosyasını yükle (proje kök dizininden)
env_path = Path(__file__).resolve().parent.parent.parent / ".env"
if env_path.exists():
    load_dotenv(env_path)
else:
    # Docker'da .env mount edilmiş olabilir
    load_dotenv()


class Settings(BaseSettings):
    # ─── R2 ───
    r2_account_id: str = ""
    r2_access_key_id: str = ""
    r2_secret_access_key: str = ""
    r2_bucket_name: str = "imageforge-dev"
    r2_endpoint: str = ""
    r2_public_url: str = ""

    # ─── Callback ───
    callback_base_url: str = "http://localhost:5265"

    # ─── Model Paths ───
    base_model: str = "stabilityai/stable-diffusion-xl-base-1.0"
    lora_dir: str = "./lora_weights"

    # ─── LoRA Configs ───
    disco_elysium_lora: str = "DiscoElysium.safetensors"
    slay_the_princess_lora: str = "SlayThePrincess.safetensors"
    lora_network_dim: int = 64
    lora_network_alpha: int = 32

    class Config:
        env_prefix = ""
        case_sensitive = False


settings = Settings()
