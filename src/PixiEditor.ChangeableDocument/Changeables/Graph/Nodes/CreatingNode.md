# How to create a new node?

This is a guide that will help with writing new nodes and will help
with solving common problems that may occur during the process.


# Creating shader nodes



# Important notes

1. Do not create new Textures directly unless they are disposed in the same execution context.
Creating new textures without managing them properly will lead to memory leaks, performance issues, visual glitches and unexpected crashes.

Use `RequestTexture` method, it handles node texture caching and management.

2. For the love of god, do not enumerate over pixels in a loop unless it's absolutely necessary. Create a proper shader builder instead.