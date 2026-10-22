using System.Collections.Generic;
using UnityEngine;
using HousePlanner.Core;

namespace HousePlanner.HouseGeneration
{
    /// <summary>
    /// Static utility class for creating procedural meshes used in 3D house generation.
    /// All meshes are generated with proper vertices, triangles, normals, UVs, and
    /// recalculated bounds. Wall meshes support openings (doors/windows) via quad
    /// subdivision rather than complex boolean operations.
    /// </summary>
    public static class ProceduralMeshBuilder
    {
        // -----------------------------------------------------------------
        //  Simple primitives
        // -----------------------------------------------------------------

        /// <summary>
        /// Creates a flat horizontal floor quad on the XZ plane at Y = 0.
        /// </summary>
        /// <param name="width">Floor extent along the X axis (meters).</param>
        /// <param name="length">Floor extent along the Z axis (meters).</param>
        /// <returns>A new <see cref="Mesh"/> representing the floor.</returns>
        public static Mesh CreateFloor(float width, float length)
        {
            Mesh mesh = new Mesh { name = "ProceduralFloor" };

            Vector3[] vertices =
            {
                new Vector3(0f, 0f, 0f),
                new Vector3(width, 0f, 0f),
                new Vector3(width, 0f, length),
                new Vector3(0f, 0f, length)
            };

            int[] triangles = { 0, 2, 1, 0, 3, 2 };

            Vector3[] normals =
            {
                Vector3.up, Vector3.up, Vector3.up, Vector3.up
            };

            Vector2[] uvs =
            {
                new Vector2(0f, 0f),
                new Vector2(width, 0f),
                new Vector2(width, length),
                new Vector2(0f, length)
            };

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.normals = normals;
            mesh.uv = uvs;
            mesh.RecalculateBounds();

            return mesh;
        }

        /// <summary>
        /// Creates a flat horizontal ceiling quad on the XZ plane at a given height.
        /// The ceiling faces downward (normal = Vector3.down).
        /// </summary>
        /// <param name="width">Ceiling extent along the X axis (meters).</param>
        /// <param name="length">Ceiling extent along the Z axis (meters).</param>
        /// <returns>A new <see cref="Mesh"/> representing the ceiling.</returns>
        public static Mesh CreateCeiling(float width, float length)
        {
            Mesh mesh = new Mesh { name = "ProceduralCeiling" };

            Vector3[] vertices =
            {
                new Vector3(0f, 0f, 0f),
                new Vector3(width, 0f, 0f),
                new Vector3(width, 0f, length),
                new Vector3(0f, 0f, length)
            };

            // Reverse winding so the face points downward
            int[] triangles = { 0, 1, 2, 0, 2, 3 };

            Vector3[] normals =
            {
                Vector3.down, Vector3.down, Vector3.down, Vector3.down
            };

            Vector2[] uvs =
            {
                new Vector2(0f, 0f),
                new Vector2(width, 0f),
                new Vector2(width, length),
                new Vector2(0f, length)
            };

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.normals = normals;
            mesh.uv = uvs;
            mesh.RecalculateBounds();

            return mesh;
        }

        /// <summary>
        /// Creates a simple six-faced box mesh centred at half-extents.
        /// Origin is at (0, 0, 0) and extends to (width, height, depth).
        /// </summary>
        /// <param name="width">Extent along the X axis.</param>
        /// <param name="height">Extent along the Y axis.</param>
        /// <param name="depth">Extent along the Z axis.</param>
        /// <returns>A new <see cref="Mesh"/> representing the box.</returns>
        public static Mesh CreateBox(float width, float height, float depth)
        {
            Mesh mesh = new Mesh { name = "ProceduralBox" };

            float w = width;
            float h = height;
            float d = depth;

            // 24 vertices (4 per face, 6 faces)
            Vector3[] vertices =
            {
                // Front face (Z = d, normal +Z)
                new Vector3(0, 0, d), new Vector3(w, 0, d), new Vector3(w, h, d), new Vector3(0, h, d),
                // Back face  (Z = 0, normal -Z)
                new Vector3(w, 0, 0), new Vector3(0, 0, 0), new Vector3(0, h, 0), new Vector3(w, h, 0),
                // Top face   (Y = h, normal +Y)
                new Vector3(0, h, 0), new Vector3(0, h, d), new Vector3(w, h, d), new Vector3(w, h, 0),
                // Bottom face(Y = 0, normal -Y)
                new Vector3(0, 0, d), new Vector3(0, 0, 0), new Vector3(w, 0, 0), new Vector3(w, 0, d),
                // Right face (X = w, normal +X)
                new Vector3(w, 0, d), new Vector3(w, 0, 0), new Vector3(w, h, 0), new Vector3(w, h, d),
                // Left face  (X = 0, normal -X)
                new Vector3(0, 0, 0), new Vector3(0, 0, d), new Vector3(0, h, d), new Vector3(0, h, 0)
            };

            int[] triangles = new int[36];
            for (int face = 0; face < 6; face++)
            {
                int vi = face * 4;
                int ti = face * 6;
                triangles[ti + 0] = vi + 0;
                triangles[ti + 1] = vi + 2;
                triangles[ti + 2] = vi + 1;
                triangles[ti + 3] = vi + 0;
                triangles[ti + 4] = vi + 3;
                triangles[ti + 5] = vi + 2;
            }

            Vector2[] uvs = new Vector2[24];
            for (int face = 0; face < 6; face++)
            {
                int vi = face * 4;
                uvs[vi + 0] = new Vector2(0, 0);
                uvs[vi + 1] = new Vector2(1, 0);
                uvs[vi + 2] = new Vector2(1, 1);
                uvs[vi + 3] = new Vector2(0, 1);
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }

        // -----------------------------------------------------------------
        //  Wall with openings
        // -----------------------------------------------------------------

        /// <summary>
        /// Creates a flat wall mesh on the XY plane (at Z = 0) with rectangular
        /// openings cut out for doors and windows.
        /// <para>
        /// The algorithm subdivides the wall face into up to four quads per opening:
        /// left strip, right strip, bottom strip (below opening), and top strip
        /// (above opening). This avoids complex CSG boolean operations.
        /// </para>
        /// </summary>
        /// <param name="width">Total wall width along the X axis (meters).</param>
        /// <param name="height">Total wall height along the Y axis (meters).</param>
        /// <param name="thickness">Wall thickness along the Z axis (meters).</param>
        /// <param name="openings">
        /// List of <see cref="WallOpeningData"/> describing doors/windows.
        /// May be <c>null</c> or empty for a solid wall.
        /// </param>
        /// <returns>A new <see cref="Mesh"/> representing the wall.</returns>
        public static Mesh CreateWall(float width, float height, float thickness,
            List<WallOpeningData> openings)
        {
            Mesh mesh = new Mesh { name = "ProceduralWall" };

            if (openings == null || openings.Count == 0)
            {
                // Solid wall — simple box
                return CreateSolidWall(width, height, thickness);
            }

            // Sort openings left-to-right by their centre position
            List<WallOpeningData> sorted = new List<WallOpeningData>(openings);
            sorted.Sort((a, b) => a.position.CompareTo(b.position));

            // Clamp openings to wall bounds
            List<OpeningRect> rects = new List<OpeningRect>();
            foreach (WallOpeningData o in sorted)
            {
                float centreX = o.position * width;
                float halfW = o.width * 0.5f;
                float left = Mathf.Max(0f, centreX - halfW);
                float right = Mathf.Min(width, centreX + halfW);
                float bottom = Mathf.Max(0f, o.elevation);
                float top = Mathf.Min(height, o.elevation + o.height);

                if (right - left < 0.01f || top - bottom < 0.01f)
                {
                    Debug.LogWarning(
                        $"[ProceduralMeshBuilder] Opening '{o.id}' has degenerate size, skipping.");
                    continue;
                }

                rects.Add(new OpeningRect(left, right, bottom, top));
            }

            if (rects.Count == 0)
            {
                return CreateSolidWall(width, height, thickness);
            }

            // Build the front face quads, then extrude for thickness
            List<Vector3> verts = new List<Vector3>();
            List<int> tris = new List<int>();
            List<Vector2> uvs = new List<Vector2>();

            // ---- Front face (+Z = thickness) ----
            BuildWallFaceWithOpenings(verts, tris, uvs,
                width, height, thickness, rects, Vector3.forward);

            // ---- Back face (Z = 0) ----
            BuildWallFaceWithOpenings(verts, tris, uvs,
                width, height, 0f, rects, Vector3.back);

            // ---- Top edge of wall ----
            AddEdgeQuad(verts, tris, uvs,
                new Vector3(0, height, 0), new Vector3(width, height, 0),
                new Vector3(0, height, thickness), new Vector3(width, height, thickness),
                Vector3.up);

            // ---- Bottom edge of wall ----
            AddEdgeQuad(verts, tris, uvs,
                new Vector3(0, 0, thickness), new Vector3(width, 0, thickness),
                new Vector3(0, 0, 0), new Vector3(width, 0, 0),
                Vector3.down);

            // ---- Left edge of wall ----
            AddEdgeQuad(verts, tris, uvs,
                new Vector3(0, 0, 0), new Vector3(0, 0, thickness),
                new Vector3(0, height, 0), new Vector3(0, height, thickness),
                Vector3.left);

            // ---- Right edge of wall ----
            AddEdgeQuad(verts, tris, uvs,
                new Vector3(width, 0, thickness), new Vector3(width, 0, 0),
                new Vector3(width, height, thickness), new Vector3(width, height, 0),
                Vector3.right);

            // ---- Opening inner faces (the "reveal" inside each opening) ----
            foreach (OpeningRect rect in rects)
            {
                // Left inner face
                AddEdgeQuad(verts, tris, uvs,
                    new Vector3(rect.left, rect.bottom, thickness),
                    new Vector3(rect.left, rect.bottom, 0),
                    new Vector3(rect.left, rect.top, thickness),
                    new Vector3(rect.left, rect.top, 0),
                    Vector3.right);

                // Right inner face
                AddEdgeQuad(verts, tris, uvs,
                    new Vector3(rect.right, rect.bottom, 0),
                    new Vector3(rect.right, rect.bottom, thickness),
                    new Vector3(rect.right, rect.top, 0),
                    new Vector3(rect.right, rect.top, thickness),
                    Vector3.left);

                // Top inner face (soffit)
                AddEdgeQuad(verts, tris, uvs,
                    new Vector3(rect.left, rect.top, 0),
                    new Vector3(rect.right, rect.top, 0),
                    new Vector3(rect.left, rect.top, thickness),
                    new Vector3(rect.right, rect.top, thickness),
                    Vector3.down);

                // Bottom inner face (sill) — only for windows with elevation > 0
                if (rect.bottom > 0.01f)
                {
                    AddEdgeQuad(verts, tris, uvs,
                        new Vector3(rect.left, rect.bottom, thickness),
                        new Vector3(rect.right, rect.bottom, thickness),
                        new Vector3(rect.left, rect.bottom, 0),
                        new Vector3(rect.right, rect.bottom, 0),
                        Vector3.up);
                }
            }

            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }

        // -----------------------------------------------------------------
        //  Door / window frames
        // -----------------------------------------------------------------

        /// <summary>
        /// Creates a simple door frame mesh as a U-shaped set of three boxes
        /// (two side jambs and one header).
        /// </summary>
        /// <param name="width">Outer width of the frame opening.</param>
        /// <param name="height">Outer height of the frame.</param>
        /// <param name="thickness">Depth of the frame into the wall.</param>
        /// <returns>A combined <see cref="Mesh"/>.</returns>
        public static Mesh CreateDoorFrame(float width, float height, float thickness)
        {
            const float frameWidth = 0.05f; // 5 cm frame members

            Mesh mesh = new Mesh { name = "ProceduralDoorFrame" };

            List<Vector3> verts = new List<Vector3>();
            List<int> tris = new List<int>();
            List<Vector2> uvs = new List<Vector2>();

            // Left jamb
            AddBoxToLists(verts, tris, uvs,
                new Vector3(0, 0, 0), frameWidth, height, thickness);

            // Right jamb
            AddBoxToLists(verts, tris, uvs,
                new Vector3(width - frameWidth, 0, 0), frameWidth, height, thickness);

            // Top header
            AddBoxToLists(verts, tris, uvs,
                new Vector3(0, height - frameWidth, 0), width, frameWidth, thickness);

            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }

        /// <summary>
        /// Creates a simple window frame mesh as a rectangular set of four boxes
        /// (two side jambs, one header, and one sill).
        /// </summary>
        /// <param name="width">Outer width of the frame opening.</param>
        /// <param name="height">Outer height of the frame.</param>
        /// <param name="thickness">Depth of the frame into the wall.</param>
        /// <returns>A combined <see cref="Mesh"/>.</returns>
        public static Mesh CreateWindowFrame(float width, float height, float thickness)
        {
            const float frameWidth = 0.04f; // 4 cm frame members

            Mesh mesh = new Mesh { name = "ProceduralWindowFrame" };

            List<Vector3> verts = new List<Vector3>();
            List<int> tris = new List<int>();
            List<Vector2> uvs = new List<Vector2>();

            // Left jamb
            AddBoxToLists(verts, tris, uvs,
                new Vector3(0, 0, 0), frameWidth, height, thickness);

            // Right jamb
            AddBoxToLists(verts, tris, uvs,
                new Vector3(width - frameWidth, 0, 0), frameWidth, height, thickness);

            // Top header
            AddBoxToLists(verts, tris, uvs,
                new Vector3(0, height - frameWidth, 0), width, frameWidth, thickness);

            // Bottom sill
            AddBoxToLists(verts, tris, uvs,
                new Vector3(0, 0, 0), width, frameWidth, thickness);

            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }

        // =================================================================
        //  Private helpers
        // =================================================================

        /// <summary>
        /// Small value type for a clamped rectangular opening.
        /// </summary>
        private readonly struct OpeningRect
        {
            public readonly float left;
            public readonly float right;
            public readonly float bottom;
            public readonly float top;

            public OpeningRect(float left, float right, float bottom, float top)
            {
                this.left = left;
                this.right = right;
                this.bottom = bottom;
                this.top = top;
            }
        }

        /// <summary>
        /// Creates a solid wall as a simple box (no openings).
        /// </summary>
        private static Mesh CreateSolidWall(float width, float height, float thickness)
        {
            Mesh box = CreateBox(width, height, thickness);
            box.name = "ProceduralWall_Solid";
            return box;
        }

        /// <summary>
        /// Builds one face of the wall (front or back) as a set of quads that
        /// leave rectangular holes for each opening.
        /// <para>
        /// Strategy: sweep left-to-right across the wall, emitting full-height
        /// vertical strips between openings and partial-height strips above and
        /// below each opening.
        /// </para>
        /// </summary>
        private static void BuildWallFaceWithOpenings(
            List<Vector3> verts, List<int> tris, List<Vector2> uvs,
            float wallWidth, float wallHeight, float zPos,
            List<OpeningRect> openings, Vector3 faceNormal)
        {
            bool isFront = faceNormal == Vector3.forward;
            float cursorX = 0f;

            for (int i = 0; i < openings.Count; i++)
            {
                OpeningRect o = openings[i];

                // 1) Full-height strip from cursorX to opening left edge
                if (o.left - cursorX > 0.001f)
                {
                    AddFaceQuad(verts, tris, uvs,
                        cursorX, 0f, o.left, wallHeight,
                        zPos, wallWidth, wallHeight, isFront);
                }

                // 2) Strip below the opening (from opening left to opening right)
                if (o.bottom > 0.001f)
                {
                    AddFaceQuad(verts, tris, uvs,
                        o.left, 0f, o.right, o.bottom,
                        zPos, wallWidth, wallHeight, isFront);
                }

                // 3) Strip above the opening
                if (wallHeight - o.top > 0.001f)
                {
                    AddFaceQuad(verts, tris, uvs,
                        o.left, o.top, o.right, wallHeight,
                        zPos, wallWidth, wallHeight, isFront);
                }

                cursorX = o.right;
            }

            // 4) Remaining full-height strip after the last opening
            if (wallWidth - cursorX > 0.001f)
            {
                AddFaceQuad(verts, tris, uvs,
                    cursorX, 0f, wallWidth, wallHeight,
                    zPos, wallWidth, wallHeight, isFront);
            }
        }

        /// <summary>
        /// Adds a single face-aligned quad defined by its XY extents and a fixed Z.
        /// </summary>
        private static void AddFaceQuad(
            List<Vector3> verts, List<int> tris, List<Vector2> uvs,
            float xMin, float yMin, float xMax, float yMax,
            float z, float wallWidth, float wallHeight, bool isFront)
        {
            int baseIdx = verts.Count;

            if (isFront)
            {
                // Winding for normal = +Z
                verts.Add(new Vector3(xMin, yMin, z));
                verts.Add(new Vector3(xMax, yMin, z));
                verts.Add(new Vector3(xMax, yMax, z));
                verts.Add(new Vector3(xMin, yMax, z));

                tris.Add(baseIdx + 0);
                tris.Add(baseIdx + 2);
                tris.Add(baseIdx + 1);
                tris.Add(baseIdx + 0);
                tris.Add(baseIdx + 3);
                tris.Add(baseIdx + 2);
            }
            else
            {
                // Winding for normal = -Z
                verts.Add(new Vector3(xMin, yMin, z));
                verts.Add(new Vector3(xMax, yMin, z));
                verts.Add(new Vector3(xMax, yMax, z));
                verts.Add(new Vector3(xMin, yMax, z));

                tris.Add(baseIdx + 0);
                tris.Add(baseIdx + 1);
                tris.Add(baseIdx + 2);
                tris.Add(baseIdx + 0);
                tris.Add(baseIdx + 2);
                tris.Add(baseIdx + 3);
            }

            // UVs normalised to wall dimensions
            uvs.Add(new Vector2(xMin / wallWidth, yMin / wallHeight));
            uvs.Add(new Vector2(xMax / wallWidth, yMin / wallHeight));
            uvs.Add(new Vector2(xMax / wallWidth, yMax / wallHeight));
            uvs.Add(new Vector2(xMin / wallWidth, yMax / wallHeight));
        }

        /// <summary>
        /// Adds an arbitrary quad defined by four corner positions with a given normal
        /// (used for wall edges and opening reveal faces).
        /// </summary>
        private static void AddEdgeQuad(
            List<Vector3> verts, List<int> tris, List<Vector2> uvs,
            Vector3 bl, Vector3 br, Vector3 tl, Vector3 tr,
            Vector3 normal)
        {
            int baseIdx = verts.Count;

            verts.Add(bl);
            verts.Add(br);
            verts.Add(tr);
            verts.Add(tl);

            // Standard winding: 0-2-1, 0-3-2
            tris.Add(baseIdx + 0);
            tris.Add(baseIdx + 2);
            tris.Add(baseIdx + 1);
            tris.Add(baseIdx + 0);
            tris.Add(baseIdx + 3);
            tris.Add(baseIdx + 2);

            // Simple planar UVs based on edge distances
            float uLen = Vector3.Distance(bl, br);
            float vLen = Vector3.Distance(bl, tl);
            uvs.Add(new Vector2(0f, 0f));
            uvs.Add(new Vector2(uLen, 0f));
            uvs.Add(new Vector2(uLen, vLen));
            uvs.Add(new Vector2(0f, vLen));
        }

        /// <summary>
        /// Appends a six-faced box (24 vertices) to shared mesh lists,
        /// offset by a given origin position.
        /// </summary>
        private static void AddBoxToLists(
            List<Vector3> verts, List<int> tris, List<Vector2> uvs,
            Vector3 origin, float w, float h, float d)
        {
            int baseIdx = verts.Count;
            float ox = origin.x, oy = origin.y, oz = origin.z;

            // 24 vertices — 4 per face
            Vector3[] boxVerts =
            {
                // Front (+Z)
                new Vector3(ox, oy, oz + d), new Vector3(ox + w, oy, oz + d),
                new Vector3(ox + w, oy + h, oz + d), new Vector3(ox, oy + h, oz + d),
                // Back (-Z)
                new Vector3(ox + w, oy, oz), new Vector3(ox, oy, oz),
                new Vector3(ox, oy + h, oz), new Vector3(ox + w, oy + h, oz),
                // Top (+Y)
                new Vector3(ox, oy + h, oz), new Vector3(ox, oy + h, oz + d),
                new Vector3(ox + w, oy + h, oz + d), new Vector3(ox + w, oy + h, oz),
                // Bottom (-Y)
                new Vector3(ox, oy, oz + d), new Vector3(ox, oy, oz),
                new Vector3(ox + w, oy, oz), new Vector3(ox + w, oy, oz + d),
                // Right (+X)
                new Vector3(ox + w, oy, oz + d), new Vector3(ox + w, oy, oz),
                new Vector3(ox + w, oy + h, oz), new Vector3(ox + w, oy + h, oz + d),
                // Left (-X)
                new Vector3(ox, oy, oz), new Vector3(ox, oy, oz + d),
                new Vector3(ox, oy + h, oz + d), new Vector3(ox, oy + h, oz)
            };

            verts.AddRange(boxVerts);

            for (int face = 0; face < 6; face++)
            {
                int vi = baseIdx + face * 4;
                tris.Add(vi + 0);
                tris.Add(vi + 2);
                tris.Add(vi + 1);
                tris.Add(vi + 0);
                tris.Add(vi + 3);
                tris.Add(vi + 2);

                uvs.Add(new Vector2(0, 0));
                uvs.Add(new Vector2(1, 0));
                uvs.Add(new Vector2(1, 1));
                uvs.Add(new Vector2(0, 1));
            }
        }
    }
}
