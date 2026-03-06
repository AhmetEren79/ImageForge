import boto3
from botocore.config import Config as BotoConfig
import os

def set_cors():
    endpoint = os.environ.get("R2_ENDPOINT")
    access_key = os.environ.get("R2_ACCESS_KEY_ID")
    secret_key = os.environ.get("R2_SECRET_ACCESS_KEY")
    bucket = os.environ.get("R2_BUCKET_NAME")

    if not all([endpoint, access_key, secret_key, bucket]):
        print("Missing required environment variables for R2.")
        return

    client = boto3.client(
        "s3",
        endpoint_url=endpoint,
        aws_access_key_id=access_key,
        aws_secret_access_key=secret_key,
        region_name="auto",
        config=BotoConfig(signature_version="s3v4")
    )

    cors_configuration = {
        'CORSRules': [{
            'AllowedHeaders': ['*'],
            'AllowedMethods': ['GET', 'HEAD', 'PUT', 'POST', 'DELETE'],
            'AllowedOrigins': ['http://localhost:4200', 'http://127.0.0.1:4200', 'https://localhost:4200'],
            'ExposeHeaders': ['ETag'],
            'MaxAgeSeconds': 3000
        }]
    }

    try:
        client.put_bucket_cors(
            Bucket=bucket,
            CORSConfiguration=cors_configuration
        )
        print(f"CORS updated successfully for bucket {bucket}.")
    except Exception as e:
        print(f"Error updating CORS: {e}")

if __name__ == "__main__":
    set_cors()
