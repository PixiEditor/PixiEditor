# How to create a new node?

This is a guide that will help with writing new nodes and will help
with solving common problems that may occur during the process.

# Creating shader nodes

1. Each node needs to inherit from `Node` class
2. Each node class needs `NodeInfoAttribute` with Unique Name and Display name, display name should be localized, unique name should be unique across all nodes.
3. Node inputs are serialized and therefore any input should have a proper SerializationFactory class, otherwise saving fails.

# Important notes

1. Do not create new Textures directly unless they are disposed in the same execution context.
Creating new textures without managing them properly will lead to memory leaks, performance issues, visual glitches and unexpected crashes.

Use `RequestTexture` method, it handles node texture caching and management.
