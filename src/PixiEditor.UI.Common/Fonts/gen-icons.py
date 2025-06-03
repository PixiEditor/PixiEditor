import xml.etree.ElementTree as ET
import re

# Input and output file paths
SVG_FILE = "defs.svg"
XAML_FILE = "PixiPerfectIcons.axaml"
CS_FILE = "PixiPerfectIcons.axaml.cs"

# Parse SVG and extract relevant glyphs
tree = ET.parse(SVG_FILE)
root = tree.getroot()[1][0]  # Navigate to the <defs> <font> element

print(root[3].tag)

glyphs = []

# Traverse all glyph nodes
for glyph in root:
    if glyph.tag != "{http://www.w3.org/2000/svg}glyph":
        continue
    unicode_char = glyph.attrib.get("unicode")
    glyph_name = glyph.attrib.get("glyph-name")
    if unicode_char and glyph_name:
        code_point = f"{ord(unicode_char):04X}"
        glyphs.append((glyph_name, code_point))

# Sort glyphs by name
glyphs.sort()

# Utility functions
def to_kebab_case(name: str) -> str:
    return "icon-" + re.sub(r"(?<!^)(?=[A-Z])", "-", name).lower()

def to_pascal_case(name: str) -> str:
    return "".join(part.capitalize() for part in re.split(r"[-_]", name))

# Generate XAML content
xaml_lines = [
    '<Styles xmlns="https://github.com/avaloniaui"',
    '        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"',
    '        xmlns:system="clr-namespace:System;assembly=System.Runtime">',
    '    <Styles.Resources>',
    '        <ResourceDictionary>',
    '            <FontFamily x:Key="PixiPerfectIcons">avares://PixiEditor.UI.Common/Fonts#pixiperfect</FontFamily>',
]

for name, code in glyphs:
    kebab = to_kebab_case(name)
    xaml_lines.append(f'            <system:String x:Key="{kebab}">&#x{code};</system:String>')

xaml_lines += [
    '        </ResourceDictionary>',
    '    </Styles.Resources>',
    '</Styles>'
]

# Generate C# content
cs_lines = [
    "public static partial class PixiPerfectIcons",
    "{"
]

for name, code in glyphs:
    pascal = to_pascal_case(name)
    cs_lines.append(f'    public const string {pascal} = "\\u{code}";')

cs_lines.append("}")

# Write output files
with open(XAML_FILE, "w", encoding="utf-8") as xaml_out:
    xaml_out.write("\n".join(xaml_lines) + "\n")

with open(CS_FILE, "w", encoding="utf-8") as cs_out:
    cs_out.write("\n".join(cs_lines) + "\n")

print(f"Generated {XAML_FILE} and {CS_FILE} with {len(glyphs)} icons.")
