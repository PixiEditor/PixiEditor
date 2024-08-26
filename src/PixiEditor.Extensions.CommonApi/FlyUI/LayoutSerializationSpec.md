# LayoutBuilder interface

FlyUI is an abstract API used to build layouts inside PixiEditor.
Layout data is passed as byte span, which is then deserialized into a layout object.

This spec describes how to serialize and deserialize layout data.

## Layout data

Layout byte span is a recursive structure containing elements and properties data.

Byte sequence:
```
    4 bytes - unique id of the control,
    4 bytes - length of control type string,
    n bytes - control type string,
    4 bytes - length of properties data,
    n bytes - properties data,
        - 1 byte - property type,
        - (if property type is string) 4 bytes - length of string)
        - x bytes - property value, where x is determined by property type,
    4 bytes - number of children,
    n bytes - children data, where children get serialized recursively.
```