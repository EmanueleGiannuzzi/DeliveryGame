using Godot;
//Adaptation of: https://github.com/SebLague/Geographical-Adventures/blob/82fcda20bebb033c749b2339e9ce3a6e58007699/Assets/Scripts/Generation/Terrain/OutlineMeshCreator.cs

[Tool]
public partial class CubeSphereGenerator : Node {
	[Export] private bool update = false;
	
	[Export] private int resolution = 10;
	[Export] private int numSubdivisions = 2;
	[Export] private float radius = 100f;
	[Export] private bool generateColliders = false;

	private SurfaceTool meshGen = new ();
	
	public override void _Ready() {}
	
	public override void _Process(double delta) {
		if (update) {
			updateMesh();
			update = false;
		}
	}

	private void clearMeshes() {
		foreach (var child in GetChildren()) {
			child.QueueFree();
		}
	}

	private void addMesh(Mesh mesh) {
		MeshInstance3D meshInstance = new();
		meshInstance.Mesh = mesh;
		if (generateColliders) {
			meshInstance.CreateTrimeshCollision();
		}
		AddChild(meshInstance);
		meshInstance.Owner = GetTree().EditedSceneRoot;
	}
	
	private void updateMesh() {
		clearMeshes();
		
		var material = new StandardMaterial3D();
		var randomColor = new Color(GD.Randf(), GD.Randf(), GD.Randf());
		material.AlbedoColor = randomColor;
		Mesh[] meshes = GenerateSphereMeshes();
		
		foreach (var mesh in meshes) {
			mesh.SurfaceSetMaterial(0, material);
			addMesh(mesh);
		}
		GD.Print("Mesh Updated");
	}

	public Mesh[] GenerateSphereMeshes() {
		Vector3[] faceNormals = { Vector3.Up, Vector3.Down, Vector3.Left, Vector3.Right, Vector3.Forward, Vector3.Back };
		Mesh[] meshes = new Mesh[faceNormals.Length * numSubdivisions * numSubdivisions];
		float faceCoveragePerSubFace = 1f / numSubdivisions;
		int meshIndex = 0;

		foreach (Vector3 faceNormal in faceNormals)
		{
			for (int y = 0; y < numSubdivisions; y++)
			{
				for (int x = 0; x < numSubdivisions; x++)
				{
					Vector2 startT = new Vector2(x, y) * faceCoveragePerSubFace;
					Vector2 endT = startT + Vector2.One * faceCoveragePerSubFace;
					
					meshGen.Clear();
					meshGen.Begin(Mesh.PrimitiveType.Triangles);
					addFaceToMeshGen(faceNormal, startT, endT);
					meshes[meshIndex] = meshGen.Commit();
					meshIndex++;
				}
			}
		}

		return meshes;
	}

	private void addFaceToMeshGen(Vector3 normal, Vector2 startT, Vector2 endT) {
		Vector3 axisA = new Vector3(normal.Y, normal.Z, normal.X);
		Vector3 axisB = normal.Cross(axisA).Normalized();

		float ty = startT.Y;
		float dx = (endT.X - startT.X) / (resolution - 1);
		float dy = (endT.Y - startT.Y) / (resolution - 1);

		for (int y = 0; y < resolution; y++) {
			float tx = startT.X;

			for (int x = 0; x < resolution; x++) {
				int i = x + y * resolution;
				Vector3 pointOnUnitCube = normal + (tx - 0.5f) * 2 * axisA + (ty - 0.5f) * 2 * axisB;
				Vector3 pointOnUnitSphere = cubePointToSpherePoint(pointOnUnitCube.Normalized());
				Vector2 uv = new Vector2(x / (resolution - 1f), y / (resolution - 1f));
				meshGen.SetNormal(pointOnUnitSphere);
				meshGen.SetUV(uv);
				meshGen.AddVertex(pointOnUnitSphere * radius);

				if (x != resolution - 1 && y != resolution - 1) {
					meshGen.AddIndex(i);                     // First triangle with reversed order
					meshGen.AddIndex(i + resolution);
					meshGen.AddIndex(i + resolution + 1);

					meshGen.AddIndex(i);                     // Second triangle with reversed order
					meshGen.AddIndex(i + resolution + 1);
					meshGen.AddIndex(i + 1);
				}
				tx += dx;
			}
			ty += dy;
		}
	}
	
	private static Vector3 cubePointToSpherePoint(Vector3 p) {
		float x2 = p.X * p.X;
		float y2 = p.Y * p.Y;
		float z2 = p.Z * p.Z;

		float x = p.X * Mathf.Sqrt(1.0f - (y2 / 2.0f) - (z2 / 2.0f) + ((y2 * z2) / 3.0f));
		float y = p.Y * Mathf.Sqrt(1.0f - (z2 / 2.0f) - (x2 / 2.0f) + ((z2 * x2) / 3.0f));
		float z = p.Z * Mathf.Sqrt(1.0f - (x2 / 2.0f) - (y2 / 2.0f) + ((x2 * y2) / 3.0f));

		return new Vector3(x, y, z).Normalized();
	}


	private static Vector3 spherePointToCubePoint(Vector3 p) {
		float absX = Mathf.Abs(p.X);
		float absY = Mathf.Abs(p.Y);
		float absZ = Mathf.Abs(p.Z);

		if (absY >= absX && absY >= absZ) {
			return cubifyFace(p);
		}

		if (absX >= absY && absX >= absZ) {
			p = cubifyFace(new Vector3(p.Y, p.X, p.Z));
			return new Vector3(p.Y, p.X, p.Z);
		}

		p = cubifyFace(new Vector3(p.X, p.Z, p.Y));
		return new Vector3(p.X, p.Z, p.Y);
	}
	
	private static Vector3 cubifyFace(Vector3 p)
	{
		const float inverseSqrt2 = 0.70710676908493042f;

		float a2 = p.X * p.X * 2.0f;
		float b2 = p.Z * p.Z * 2.0f;
		float inner = -a2 + b2 - 3;
		float innerSqrt = -Mathf.Sqrt((inner * inner) - 12.0f * a2);

		if (p.X != 0) {
			p.X = Mathf.Min(1, Mathf.Sqrt(innerSqrt + a2 - b2 + 3.0f) * inverseSqrt2) * Mathf.Sign(p.X);
		}

		if (p.Z != 0) {
			p.Z = Mathf.Min(1, Mathf.Sqrt(innerSqrt - a2 + b2 + 3.0f) * inverseSqrt2) * Mathf.Sign(p.Z);
		}

		// Top/bottom face
		p.Y = Mathf.Sign(p.Y);

		return p;
	}
}
