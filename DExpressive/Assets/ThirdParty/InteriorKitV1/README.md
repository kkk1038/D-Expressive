# Interior Kit v1 — Lifelike Home & Office Asset Pack

20 game-ready, realistic interior pieces: modular walls, floors, ceilings, a door, a window and furniture for a living room, bedroom, kitchen and office.

## Contents
- **wall_2x3** / **wall_door_2x3** / **wall_window_2x3** — modular 2 x 3 m plaster walls (plain, doorway, window opening)
- **floor_2x2** / **ceiling_2x2** — 2 x 2 m laminate floor and ceiling tiles
- **door_interior** — oak door with frame, fits wall_door_2x3
- **window_1200x1400** — double window with sill, fits wall_window_2x3
- **sofa_3seat**, **armchair**, **coffee_table**, **bookshelf**, **floor_lamp**, **rug_2x3** — living room
- **dining_table**, **dining_chair** — dining
- **bed_double** — bedroom
- **kitchen_counter_sink**, **fridge** — kitchen
- **office_desk**, **office_chair** — office

| asset | size m (x*y*z) | tris LOD0/1/2 | glb |
|---|---|---|---|
| armchair | 0.85 x 0.85 x 0.9 | 1004 / 400 / 120 | 1.6 MB |
| bed_double | 1.58 x 2.05 x 1.05 | 1204 / 480 / 144 | 2.4 MB |
| bookshelf | 0.9 x 0.32 x 1.92 | 1420 / 568 / 170 | 0.7 MB |
| ceiling_2x2 | 2.0 x 2.0 x 0.05 | 12 / 4 / 2 | 0.8 MB |
| coffee_table | 1.1 x 0.6 x 0.42 | 328 / 130 / 38 | 0.5 MB |
| dining_chair | 0.45 x 0.47 x 0.93 | 536 / 214 / 64 | 1.4 MB |
| dining_table | 1.6 x 0.9 x 0.76 | 296 / 118 / 34 | 0.6 MB |
| door_interior | 1.04 x 0.17 x 2.17 | 696 / 278 / 82 | 1.5 MB |
| floor_2x2 | 2.0 x 2.0 x 0.05 | 12 / 4 / 2 | 0.5 MB |
| floor_lamp | 0.44 x 0.44 x 1.65 | 292 / 116 / 34 | 1.4 MB |
| fridge | 0.7 x 0.73 x 1.85 | 472 / 188 / 56 | 0.9 MB |
| kitchen_counter_sink | 2.4 x 0.65 x 1.4 | 1000 / 400 / 120 | 1.7 MB |
| office_chair | 0.62 x 0.61 x 1.12 | 1084 / 432 / 130 | 2.0 MB |
| office_desk | 1.4 x 0.7 x 0.75 | 604 / 240 / 72 | 1.5 MB |
| rug_2x3 | 3.05 x 2.0 x 0.01 | 68 / 26 / 8 | 1.4 MB |
| sofa_3seat | 2.1 x 0.9 x 0.98 | 1944 / 776 / 232 | 1.3 MB |
| wall_2x3 | 2.0 x 0.18 x 3.0 | 100 / 40 / 12 | 0.8 MB |
| wall_door_2x3 | 2.0 x 0.18 x 3.0 | 212 / 84 / 24 | 0.8 MB |
| wall_window_2x3 | 2.0 x 0.18 x 3.0 | 312 / 124 / 36 | 0.8 MB |
| window_1200x1400 | 1.28 x 0.2 x 1.43 | 332 / 132 / 38 | 1.2 MB |

## Formats
- `models/*.glb` — glTF 2.0 binary, ready for **Godot 4** (textures embedded; also works in Unity, Unreal, Blender)
- `lods/*_LOD1.gltf` (~40% tris) and `*_LOD2.gltf` (~12% tris) + `.bin` — share the images in `../textures/` (no duplicates)
- `fbx/*.fbx` — FBX, textures referenced relatively from `../textures/`
- `textures/*.jpg` — 1024 px PBR sets: colour, normal, ORM (occlusion / roughness / metallic)

## Collision
Every model includes a `<name>-colonly` node; Godot 4 auto-converts it to a static collision body on import. Walls with openings, the bed and the kitchen counter use several boxes so doorways and windows stay open.

## Specs
Units: metres. Pivot: bottom-centre. Up axis: Y (glTF). Front faces -Y in Blender / front in engine.
Modular grid: walls 2 x 3 x 0.15 m, floor/ceiling tiles 2 x 2 m. The door fits the 0.9 x 2.1 m opening; the window sits at 0.9 m.

## Credits
Scan textures from **Poly Haven** (https://polyhaven.com), licensed **CC0**: laminate_floor_02, painted_plaster_wall, ceiling_interior, oak_veneer_01, black_walnut_veneer_01, velour_velvet, brown_leather, quatrefoil_jacquard_fabric, marble_01, metal_plate. Thank you, Poly Haven.

## License
You may use these assets in personal and commercial projects. You may not resell or redistribute the assets themselves as an asset pack.
