import os
import requests

# Configuration
API_KEY = os.getenv("POEDITOR_API_KEY")
PROJECT_ID = os.getenv("POEDITOR_PROJECT_ID")
UPLOAD_API_URL = "https://api.poeditor.com/v2/projects/upload"
LOCAL_FILE_PATH = "src/PixiEditor/Data/Localization/Languages/en.json"

def upload_en_json():
    if not API_KEY or not PROJECT_ID:
        print("::error::Missing POEDITOR_API_KEY or POEDITOR_PROJECT_ID environment variables.")
        return

    try:
        with open(LOCAL_FILE_PATH, "rb") as file:
            files = {
                "file": ("en.json", file, "application/json")
            }
            data = {
                "api_token": API_KEY,
                "id": PROJECT_ID,
                "updating": "terms_translations",  # Updates both terms and translations.
                "language": "en",                  # Specify language as English.
                "overwrite": 1,                    # Overwrite existing terms/translations.
                "sync_terms": 1,                   # Sync terms: delete terms not in the uploaded file.
                "fuzzy_trigger": 1                 # Mark translations in other languages as fuzzy.
            }
            response = requests.post(UPLOAD_API_URL, data=data, files=files)
    except FileNotFoundError:
        print(f"::error::Local file not found: {LOCAL_FILE_PATH}")
        return

    if response.status_code == 200:
        result = response.json()
        if result.get("response", {}).get("status") == "success":
            print("✅ Upload succeeded:")
            print(result)
        else:
            print("::error::Upload failed:")
            print(result)
    else:
        print("::error::HTTP Error:", response.status_code)
        print(response.text)

if __name__ == "__main__":
    upload_en_json()
