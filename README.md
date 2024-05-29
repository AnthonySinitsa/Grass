# Grass

### 211,600 grass rendered(2,539,200 vertices)

![grass1](images/grass1.png)

### 423,200 grass rendered(5,078,400 vertices)

![grass2](images/grass2.png)

### Randomized the grass position

![grass3](images/grass3.png)

### Grass with position and height variance

![grass4](images/grass4.png)

## Steps taken
 
- 300x300 meter plane
  - Have vertices displaced by height map

- 2 triangles that form a quad of grass, technique known as billboarding

- Use 3 intersecting quads

- Create grass texture

- Use GPU Instancing
  - Create massive buffer of grass positions
  - Then ask GPU to use buffer to find propery grass position each frame

- Calculate grass positions using the compute shader so that the positions of the grass will go directly into the GPU buffer

- The positions we are creating will fill a square space of whatever size we like(300x300)

- We take thread id of our compute shader thread, 
  - `position = id.xy`
  - Which will in range of `0 <= id.xy <= 300`
  - Then we subtract 150 `position = id.xy - 150` so that it centers it over the origin

- There should now be one grass object every meter

- Lets double amount of grass objects
  - Multiply `position *= (1 / Density)` (Density being 2)

- Added some variants to the grass positions
  - `pos.xz += noise()`

- Added some variants to the height of the grass with simplex noise
  - So that height variants are clumped together
  - `pos.y *= noise()`

```
fixed4 frag (v2f i) : SV_Target
{
    fixed4 texColor = tex2D(_MainTex, i.uv);
    // Perform alpha clipping
    if (texColor.a < 0.5) discard; // Adjust the threshold as needed
    return texColor;
}
```
