using Godot;

[Tool]
public partial class WorldGenerator : Node3D {
	[Export] private bool update = false;
	[Export] private int resolution = 2;
	[Export] private float radius = 1f;
	public override void _Process(double delta) {
		if (update) {
			updateMesh();
			update = false;
		}
	}

	private void updateMesh() {
		MeshInstance3D meshInstance = GetNode<MeshInstance3D>("WorldMesh");
		meshInstance.SetMesh(Icosphere.GenerateMesh(resolution, radius));
		// meshInstance.SetMesh(CubeSphere.GenerateMesh(resolution, radius));
	}
}
