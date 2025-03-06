import json
import os
import requests

API_KEY = os.getenv("POEDITOR_API_KEY")
PROJECT_ID = os.getenv("POEDITOR_PROJECT_ID")
GITHUB_OUTPUT = os.getenv('GITHUB_OUTPUT')

API_URL = "https://api.poeditor.com/v2/projects/export"
LANGUAGE = "en"
LOCAL_FILE_PATH = "src/PixiEditor/Data/Localization/Languages/en.json"

def fetch_poeditor_json():
    """Fetches the latest en.json from POEditor API (remote data)"""
    if not API_KEY or not PROJECT_ID:
        print("::error::Missing API_KEY or PROJECT_ID in environment variables.")
        return None

    response = requests.post(API_URL, data={
        "api_token": API_KEY,
        "id": PROJECT_ID,
        "type": "key_value_json",
        "language": LANGUAGE
    })

    if response.status_code == 200:
        data = response.json()
        if "result" in data and "url" in data["result"]:
            download_url = data["result"]["url"]
            latest_response = requests.get(download_url)
            return latest_response.json() if latest_response.status_code == 200 else None
    return None

def load_local_json():
    """Loads the local en.json file (authoritative source)"""
    try:
        with open(LOCAL_FILE_PATH, "r", encoding="utf-8") as file:
            return json.load(file)
    except FileNotFoundError:
        print("::error::Local en.json file not found!")
        return {}

def compare_json(local_data, remote_data):
    """Compares the local and remote JSON data, detecting added, removed, and modified keys"""
    modifications = []
    additions = []
    deletions = []
    
    # Check for modified keys (key exists in both, but value changed)
    for key, local_value in local_data.items():
        remote_value = remote_data.get(key)
        if remote_value is not None and local_value != remote_value:
            modifications.append(f"🔄 {key}: '{remote_value}' → '{local_value}'")

    # Check for added keys (exist in local but missing in POEditor)
    for key in local_data.keys() - remote_data.keys():
        additions.append(f"➕ {key}: '{local_data[key]}'")

    # Check for removed keys (exist in POEditor but missing locally)
    for key in remote_data.keys() - local_data.keys():
        deletions.append(f"❌ {key}: '{remote_data[key]}'")

    return modifications, additions, deletions

def print_group(title, items):
    """Prints grouped items using GitHub Actions logging format"""
    if items:
        print(f"::group::{len(items)} {title}")
        for item in items:
            print(item)
        print("::endgroup::")

def main():
    remote_json = fetch_poeditor_json()
    if remote_json is None:
        print("::error::Failed to fetch POEditor en.json")
        exit(1)
        return
    
    local_json = load_local_json()
    
    modifications, additions, deletions = compare_json(local_json, remote_json)
    has_changes = (modifications or additions or deletions)

    with open(GITHUB_OUTPUT, "a") as f:
        f.write(f"HAS_CHANGES={str(has_changes).lower()}")

    if not has_changes:
        print("✅ No changes detected. Local and remote are in sync.")
    else:
        print_group("Key(s) Modified", modifications)
        print_group("Key(s) to be Added", additions)
        print_group("Key(s) to be Removed", deletions)

if __name__ == "__main__":
    main()
