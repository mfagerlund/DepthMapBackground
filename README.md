# DepthMapBackground
Create Meshes from Depth Maps in Unity. Note there are severe limitations - this can't really be used to create walkable areas, more like shallow backdrops.

1. Select / AI Generate an image
2. Generate a depthmap for said image for instance using https://huggingface.co/spaces/pytorch/MiDaS / https://huggingface.co/Intel/dpt-large
3. Use DepthMapBackground to create a mesh for the depthmap
4. Create an Unlit Material and add the image as the texture
5. Apply the texture to the bitmap.

See related YouTube video: https://youtu.be/hL9YR2lTPww
