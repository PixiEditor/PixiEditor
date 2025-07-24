import json
import os
import logging
from collections import OrderedDict
import time
import ahocorasick

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger()

# PATHS
REFERENCE_LANGUAGE = "src/PixiEditor/Data/Localization/Languages/en.json"
SEARCH_DIRECTORIES = ["src/PixiEditor/Views", "src/PixiEditor/ViewModels", "src/PixiEditor", "src/"]
IGNORE_DIRECTORIES = ["src/PixiEditor/Data/Localization"]

def load_json(file_path):
    """Load language JSON"""
    try:
        with open(file_path, "r", encoding="utf-8-sig") as f:
            return json.load(f, object_pairs_hook=OrderedDict)
    except FileNotFoundError:
        print(f"::error::File not found: {file_path}")
        return OrderedDict()
    except json.JSONDecodeError as e:
        print(f"::error::Failed to parse JSON in {file_path}: {e}")
        return OrderedDict()

def build_automaton(keys: list[str]) -> ahocorasick.Automaton:
    A = ahocorasick.Automaton()
    for i, k in enumerate(keys):
        A.add_word(k, (i, k))
    A.make_automaton()
    return A

def find_missing_keys(keys):
    automaton = build_automaton(keys)
    present = set()

    ignore_prefixes = tuple(os.path.abspath(p) for p in IGNORE_DIRECTORIES)
    for base_dir in SEARCH_DIRECTORIES:
        for root, dirs, files in os.walk(base_dir, topdown=True):
            dirs[:] = [d for d in dirs if not os.path.abspath(os.path.join(root, d)).startswith(ignore_prefixes)]
            for file in files:
                with open(os.path.join(root, file), "r", encoding="utf‑8", errors="ignore") as f:
                    for _, (_, k) in automaton.iter(f.read()):
                        present.add(k)
                        if len(present) == len(keys):
                            return []
    return sorted(set(keys) - present)

def main():
    keys = load_json(REFERENCE_LANGUAGE)

    print("Searching trough keys...")
    start = time.time()
    missing_keys = find_missing_keys(keys)
    end = time.time()
    print(f"Done, searching took {end - start}s")

    if len(missing_keys) > 0:
        print("Unreferenced keys have been found")
        for key in missing_keys:
            print(f"::error file={REFERENCE_LANGUAGE},title=Unreferenced key::No reference to '{key}' found")
        return 1
    else:
        print("All keys have been referenced")
        return 0
    
if __name__ == "__main__":
    exit(main())
