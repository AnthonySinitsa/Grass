## 3D Model Grass

### Grass using voronoi noise
![model1](../images/model1.png)

### Rotation and tilt added
![model2](../images/model2.png)

### Bezier Curve
![modelCurve](../images/modelcurve.png)

- Chunking
  - Build an individual blade of grass using vertex IDs
  - Use vertex shader to create positions from that
  - Then instance it a number of times that generates a unique position of each blade of grass

- Place blades of grass using jittered grid of points
  - Start with evenly spaced points and apply small random offset to each one
  - Use voronoi noise to give ways of clumping the grass

- For any given position in a 2d space, look at nearest nine points on a grid, each is jittered according to hash to give variation
  - Assign this 2d sample point to nearest points clump
  - With this shared clump and our distance to it we can influence various grass parameters

- Add angle variation by rotating each blade, do that in shader via per blade hash value

- Generating Grass Blade
  - Position, and Facing
  - Wind Strength at position which drives animation
  - Per-blade position based hash that drives various things on the blade including animation
  - Clump facing
  - Height for terrian, Width, Tilt, Bend, Side Curve

- Curve
  - Could just use a simple rotation of the vertex on the x-axis based on the height and random per blade curve value
    - or
    - For Cubic Bezier curve
      - Position easy to calculate
      - Derivative easy to calculate
        - Use derivative to find normal
      - Moving control points changes blade shape
        - Usefule for animation
        - Useful for varying apearance of grass
    - Decide position of the tip relative to the base
    - Controlled by tilt parameter as well as the facing
    - Next define a midpoint which is controlled by the bend parameter
      - If bend is zero then midpoint lies along line between base and tip
      - If midpoint larger than zero then push blade up and away from that center line
    - Now that we know where are control points are, it's straightforward to determine our world space position for our vertex
      - Take our zero to one value that determines where we are along the blade and feed that into Bezier curve function
      - Next take our facing direction, flip the x and y and negate to find a normal orthogonal to our facing
      - Step our vertex in the normals direction the distance depending on the width of the blade calculated in the compute and scaled by where we are along the blade
      - Taper down as we reach the tip
      - Define vertex normal
        - Find the derivative of bezier curve at our position and cross it with the normal we just found

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
  - Shift the verts in viewspace when the blades normal is orthogonal to the view vector

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