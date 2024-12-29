using System.Collections.Generic;
using Godot;

public static class CubeSphere {
	public static ArrayMesh GenerateMesh(int subdivisions, float size = 1f) {
		var arrayMesh = new ArrayMesh();

		// Directions for the cube's six faces
		Vector3[] directions = {
			Vector3.Forward, Vector3.Back, Vector3.Left,
			Vector3.Right, Vector3.Up, Vector3.Down
		};

		foreach (Vector3 normal in directions)
		{
			var arrays = CreateFace(normal, subdivisions, size);
			arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
		}

		return arrayMesh;
	}
	
	private static Godot.Collections.Array CreateFace(Vector3 normal, int subdivisions, float size = 1f)
	{
		var vertices = new List<Vector3>();
		var normals = new List<Vector3>();
		var uvs = new List<Vector2>();
		var indices = new List<int>();

		Vector3 tangent = normal.Cross(Vector3.Up).Normalized();
		if (tangent.Length() < 0.01f)
		{
			tangent = Vector3.Right;
		}
		Vector3 bitangent = tangent.Cross(normal).Normalized();

		float step = 1.0f / subdivisions;

		for (int y = 0; y <= subdivisions; y++)
		{
			for (int x = 0; x <= subdivisions; x++)
			{
				// Calculate vertex position
				Vector3 offset = tangent * (x * step - 0.5f) + bitangent * (y * step - 0.5f);
				Vector3 vertex = normal * 0.5f + offset;
				vertex = vertex.Normalized() * size;
				vertices.Add(vertex);

				// Add the correct normal for each face
				normals.Add(normal);

				// Add correct UV mapping for each face
				uvs.Add(new Vector2((float)x / subdivisions, (float)y / subdivisions));
			}
		}

		// Generate indices for each face (subdivisions * subdivisions quads)
		for (int y = 0; y < subdivisions; y++)
		{
			for (int x = 0; x < subdivisions; x++)
			{
				int bl = y * (subdivisions + 1) + x;
				int br = bl + 1;
				int tl = bl + (subdivisions + 1);
				int tr = tl + 1;

				indices.Add(bl);
				indices.Add(br);
				indices.Add(tr);

				indices.Add(bl);
				indices.Add(tr);
				indices.Add(tl);
			}
		}

		// Construct the arrays to be used by the mesh
		var arrays = new Godot.Collections.Array();
		arrays.Resize((int)Mesh.ArrayType.Max);
		arrays[(int)Mesh.ArrayType.Vertex] = vertices.ToArray();
		arrays[(int)Mesh.ArrayType.Normal] = normals.ToArray();
		arrays[(int)Mesh.ArrayType.TexUV] = uvs.ToArray();
		arrays[(int)Mesh.ArrayType.Index] = indices.ToArray();

		return arrays;
	}
}
