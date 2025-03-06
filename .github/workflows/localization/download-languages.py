import json
import os
import requests
from collections import OrderedDict
from datetime import datetime

API_KEY = os.getenv("POEDITOR_API_KEY")
PROJECT_ID = os.getenv("POEDITOR_PROJECT_ID")
GITHUB_OUTPUT = os.getenv('GITHUB_OUTPUT')

# POEditor API URLs
EXPORT_API_URL = "https://api.poeditor.com/v2/projects/export"
LANGUAGES_LIST_URL = "https://api.poeditor.com/v2/languages/list"

# Paths
LOCALIZATION_DATA_PATH = "src/PixiEditor/Data/Localization/LocalizationData.json"
LOCALES_DIR = "src/PixiEditor/Data/Localization/Languages/"

def load_ordered_json(file_path):
    """Load JSON preserving order (as an OrderedDict) and handle UTF-8 BOM."""
    try:
        with open(file_path, "r", encoding="utf-8-sig") as f:
            return json.load(f, object_pairs_hook=OrderedDict)
    except FileNotFoundError:
        print(f"::error::File not found: {file_path}")
        return OrderedDict()
    except json.JSONDecodeError as e:
        print(f"::error::Failed to parse JSON in {file_path}: {e}")
        return OrderedDict()

def write_ordered_json(file_path, data):
    """Write an OrderedDict to a JSON file in UTF-8 (without BOM)."""
    with open(file_path, "w", encoding="utf-8") as f:
        json.dump(data, f, indent=2, ensure_ascii=False)

def fetch_poeditor_language_json(language_code):
    """
    Fetch the latest key-value JSON for the given language from POEditor.
    Returns a dictionary if successful, otherwise None.
    """
    if not API_KEY or not PROJECT_ID:
        print("::error::Missing API_KEY or PROJECT_ID in environment variables.")
        return None

    response = requests.post(EXPORT_API_URL, data={
        "api_token": API_KEY,
        "id": PROJECT_ID,
        "type": "key_value_json",
        "language": language_code
    })
    if response.status_code == 200:
        data = response.json()
        if "result" in data and "url" in data["result"]:
            download_url = data["result"]["url"]
            remote_response = requests.get(download_url)
            if remote_response.status_code == 200:
                return remote_response.json()
    print(f"::error::Failed to fetch POEditor data for language '{language_code}'")
    return None

def update_locale_file(language):
    """
    For a given language (dict from LocalizationData.json), update its locale file:
      - Only keep keys that exist in the POEditor (remote) file.
      - For keys present both locally and remotely, update with the remote value (preserving the original local order).
      - Append new keys from POEditor at the bottom.
    """
    # Use "remote-code" if available, otherwise default to "code"
    lang_code = language.get("remoteCode", language["code"])
    if language["code"].lower() == "en":
        return  # Skip English (do not update)

    file_name = language["localeFileName"]
    file_path = os.path.join(LOCALES_DIR, file_name)
    local_data = load_ordered_json(file_path)
    remote_data = fetch_poeditor_language_json(lang_code)
    if remote_data is None:
        print(f"::error::Skipping update for {language['name']} ({lang_code}) due to fetch error.")
        return

    # Build new ordered data:
    # 1. Start with keys from local file that exist in remote.
    updated_data = OrderedDict()
    for key in local_data:
        if key in remote_data:
            updated_data[key] = remote_data[key]
    # 2. Append keys from remote that are missing locally.
    for key in remote_data:
        if key not in updated_data:
            updated_data[key] = remote_data[key]

    # Write file if changes exist (or if file was missing)
    if updated_data != local_data:
        write_ordered_json(file_path, updated_data)
        print(f"✅ Updated locale file for {language['name']} ({language['code']}).")
        return False
    else:
        print(f"✅ No changes for {language['name']} ({language['code']}).")
        return False

def fetch_languages_list():
    """
    Fetch the languages list from POEditor and return a mapping of language codes to
    updated dates (formatted as "YYYY-MM-DD hh:MM:ss").
    """
    if not API_KEY or not PROJECT_ID:
        print("::error::Missing API_KEY or PROJECT_ID in environment variables.")
        return {}

    response = requests.post(LANGUAGES_LIST_URL, data={
        "api_token": API_KEY,
        "id": PROJECT_ID
    })
    languages_updates = {}
    if response.status_code == 200:
        data = response.json()
        if "result" in data and "languages" in data["result"]:
            for lang in data["result"]["languages"]:
                code = lang.get("code")
                updated_iso = lang.get("updated")
                if code and updated_iso:
                    try:
                        # Parse ISO8601 format (example: "2015-05-04T14:21:41+0000")
                        dt = datetime.strptime(updated_iso, "%Y-%m-%dT%H:%M:%S%z")
                        formatted = dt.strftime("%Y-%m-%d %H:%M:%S")
                        languages_updates[code.lower()] = formatted
                    except Exception as e:
                        print(f"::error::Failed to parse date for language '{code}': {e}")
    else:
        print("::error::Failed to fetch languages list from POEditor.")
    return languages_updates

def update_localization_data(languages_updates):
    """
    Update the lastUpdated field for each language (except English) in LocalizationData.json.
    """
    localization_data = load_ordered_json(LOCALIZATION_DATA_PATH)
    if "Languages" not in localization_data:
        print("::error::'Languages' key not found in LocalizationData.json")
        return

    for language in localization_data["Languages"]:
        code = language.get("code", "").lower()
        if code == "en":
            continue  # Do not update English
        if code in languages_updates:
            language["lastUpdated"] = languages_updates[code]
    write_ordered_json(LOCALIZATION_DATA_PATH, localization_data)
    print("✅ Updated LocalizationData.json with new lastUpdated values.")

def main():
    # Fetch updated dates for languages from POEditor
    languages_updates = fetch_languages_list()

    # Load LocalizationData.json and update each language file (except English)
    localization_data = load_ordered_json(LOCALIZATION_DATA_PATH)
    if "Languages" not in localization_data:
        print("::error::'Languages' key not found in LocalizationData.json")
        return
    
    has_changes = False

    for language in localization_data["Languages"]:
        if language.get("code", "").lower() == "en":
            continue
        if update_locale_file(language):
            has_changes = True

    with open(GITHUB_OUTPUT, "a") as f:
        f.write(f"HAS_CHANGES={str(has_changes).lower()}")

    # Update lastUpdated field in LocalizationData.json
    update_localization_data(languages_updates)
    print("🎉 All language updates complete.")

if __name__ == "__main__":
    main()
