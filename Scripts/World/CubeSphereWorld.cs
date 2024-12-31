using Godot;

[Tool]
public partial class CubeSphereWorld : Node3D {
	[Export] private bool update = false;
	[Export] private int resolution = 2;
	[Export] private float radius = 1f;

	[Export] private Material material;
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
	

	private void updateMesh() {
		MeshInstance3D meshInstance = GetNode<MeshInstance3D>("WorldMesh");
		meshInstance.SetMesh(CubeSphere.GenerateMesh(resolution, radius));
		foreach (var child in meshInstance.GetChildren()) {
			if (child is MeshInstance3D childMesh) {
				childMesh.SetMaterialOverride(material);
			}
		}
	}
}
