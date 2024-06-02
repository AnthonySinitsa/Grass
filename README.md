# Grass

This is a repo I am using to learn different ways to render grass. This first variant uses billboarding, next will be using an actual 3d model. 

![fog](images/fog.gif)

## Steps taken

### 211,600 grass rendered(2,539,200 vertices)

![grass1](images/grass1.png)

### Increased density

![grass2](images/grass2.png)

### Randomized the grass position

![grass3](images/grass3.png)

### Grass with position and height variance

![grass4](images/grass4.png)

### Animated artificial wind

![wind2](images/wind2.gif)
![wind](images/wind.gif)
 
- 2 triangles that form a quad of grass, technique known as billboarding

- Use 3 intersecting quads

- Create grass texture

- Use GPU Instancing
  - Create massive buffer of grass positions
  - Then ask GPU to use buffer to find propery grass position each frame

- Calculate grass positions using the compute shader so that the positions of the grass will go directly into the GPU buffer

- The positions we are creating will fill a square space of whatever size we like

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

- Wind
  - We hash the instance id of the grass object to get random unique to individual object
  - Then 
  ```
  if id_hash > threashold:
    fast_cos
  else:
    slow_cos
  ```
  - Otherwise we compute different cos value with parameterized frequency
  ```
  cos(t * _Frequency)
  ```
  - This frequency is reduced on grass that is taller since takes longer to swing back and forth
  ```
  cos(t * (_Frequency - GrassHeight))
  ```
  - Next we square the cosine wave, reduce its amplitude, then subtract from it based on id hash to add local variance to the cosine value, and then reduce amplitude even further
  ```
  (cos * cos * 0.65) - id_hash * 0.5
  ```
  - Finally we add this trigvalue to the local position of our top side vertices, scaled by hash id for further localized variants and also height variance of the grass, since taller grass sways further than shorter grass
  ```
  pos.xz += uv.y * trigValue * grassHeight * id_hash * 0.6
  ```

## Model Grass

- Chunking
  - Build an individual blade of grass using vertex IDs
  - Use vertex shader to create positions from that
  - Then instance it a number of times that generates a unique position of each blade of grass

- Place blades of grass using jittered grid of points
  - Start with evenly spaced points and apply small random offset to each one
  - Use voronoi noise to give ways of clumping the grass

- Add angle variation by rotating each blade, do that in shader via per blade hash value

- Cubic Bezier curve to model the shape of the grass(AKA droopiness)
  - or
  - Could just use a simple rotation of the vertex on the x-axis based on the height and random per blade curve value

- Slightly round the normal instead of having flat grass normals
  - In the shader use two rotated normals and blend between them
    ```
    rotatedNormal1 = rotateY(PI * 0.3) * grassVertexNormal
    rotatedNormal2 = rotateY(PI * -0.3) * grassVertexNormal
    
    normalMixFactor = widthPercent;

    normal = mix(rotatedNormal1, rotatedNormal2, normalMixFactor);
    normal = normalize(normal);
    ```

- Shift the verts in the viewspace before the final transform is written out
  - First calculate dot product of view direction and grass normal
  - `viewDotNormal = saturate(dot(grassFaceNormal.xz, viewDir.xz))`
  - Calculate view space thicken factor. easOut function is used to shape how quickly we start adjusting the width
  - `viewSpaceThickenFactor = easeOut(1.0 - viewDotNormal, 4.0)`
  - Then ramp it down when we go completely orthogonal
  - `viewSpaceThickenFactor *= smoothstep(0.0, 0.2, viewDotNormal)`
  - Now apply viewspace adjustment
  - `mvPosition = modelViewMatrix * position`
  - `mvPosition.x += viewSpaceThickenFactor * xDirection * grassWidth`

- Add a gradient from base to tip

- For Specular, blend the normal with the Vector based distance

- Ambient Occlusion
  - Density is in the range [0, 1]
  - 0 being no grass and 1 being full grass
  - `aoForDensity = mix(1.0, 0.25, density)`
  - `ao = mix(aoForDensity, 1.0, easeIn(heightPercent, 2.0))`
  - `ambientLighting *= ao`
  - So more dense areas get more shading

- Wind
  - Take sample of noise based on grass position and scroll it with time
  - Then add on top of the curve of grass blade