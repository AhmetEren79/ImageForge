"""
storage.py — Cloudflare R2'ye görsel yükleme servisi.
boto3 S3 uyumlu client kullanarak dosyaları yükler.
"""

import io
import uuid
import logging
import boto3
from botocore.config import Config as BotoConfig
from app.config import settings

logger = logging.getLogger(__name__)


def get_r2_client():
    """Boto3 S3-compatible client for Cloudflare R2."""
    return boto3.client(
        "s3",
        endpoint_url=settings.r2_endpoint,
        aws_access_key_id=settings.r2_access_key_id,
        aws_secret_access_key=settings.r2_secret_access_key,
        region_name="auto",
        config=BotoConfig(
            signature_version="s3v4",
            retries={"max_attempts": 3, "mode": "standard"}
        )
    )


def upload_image(image_bytes: bytes, prompt_id: str, index: int) -> dict:
    """
    Görsel bytes'ını R2'ye yükler.
    Returns: { url, storage_key, file_name, file_size }
    """
    client = get_r2_client()
    file_name = f"{prompt_id}_{index}_{uuid.uuid4().hex[:8]}.png"
    storage_key = f"generations/{prompt_id}/{file_name}"

    try:
        client.upload_fileobj(
            io.BytesIO(image_bytes),
            settings.r2_bucket_name,
            storage_key,
            ExtraArgs={"ContentType": "image/png"}
        )

        public_url = f"{settings.r2_public_url}/{storage_key}"

        logger.info(f"R2'ye yüklendi: {storage_key} ({len(image_bytes)} bytes)")

        return {
            "url": public_url,
            "storage_key": storage_key,
            "file_name": file_name,
            "file_size": len(image_bytes),
        }
    except Exception as e:
        logger.error(f"R2 yükleme hatası: {e}")
        raise
