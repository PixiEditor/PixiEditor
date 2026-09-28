# Write a script that checks if the first line of {}rootdir}/src/PixiEditor/Data/changelog.md
# is the same as AssemblyInfo.cs of src\PixiEditor\Properties\AssemblyInfo.cs

import os
import re
def get_version_from_changelog(changelog_path):
    with open(changelog_path, 'r') as f:
        first_line = f.readline().strip()
        match = re.match(r'^#\s+(\d+\.\d+\.\d+\.\d+)', first_line)
        if match:
            return match.group(1)
    return None

def get_version_from_assembly_info(assembly_info_path):
    with open(assembly_info_path, 'r') as f:
        for line in f:
            match = re.match(r'^\[assembly:\s*AssemblyVersion\("(\d+\.\d+\.\d+\.\d+)"\)\]', line.strip())
            if match:
                return match.group(1)
    return None

if __name__ == "__main__":
    rootdir = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..', '..'))
    changelog_path = os.path.join(rootdir, 'src', 'PixiEditor', 'Data', 'changelog.md')
    assembly_info_path = os.path.join(rootdir, 'src', 'PixiEditor', 'Properties', 'AssemblyInfo.cs')

    if not os.path.exists(changelog_path):
        print(f"Changelog file not found at {changelog_path}")
        exit(1)

    if not os.path.exists(assembly_info_path):
        print(f"AssemblyInfo.cs file not found at {assembly_info_path}")
        exit(1)


    changelog_version = get_version_from_changelog(changelog_path)
    assembly_info_version = get_version_from_assembly_info(assembly_info_path)

    if changelog_version is None:
        print("Could not find version in changelog.md")
        exit(1)

    if assembly_info_version is None:
        print("Could not find version in AssemblyInfo.cs")
        exit(1)

    if changelog_version == assembly_info_version:
        print("Versions match: " + changelog_version)
        exit(0)
    else:
        print(f"Versions do not match: {changelog_version} (changelog) vs {assembly_info_version} (AssemblyInfo.cs)")
        exit(1)