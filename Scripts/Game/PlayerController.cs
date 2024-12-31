using Godot;
using System;

public partial class PlayerController : Node3D {
	[Export] private float speed = 0.25f;
	[Export] private float boostSpeed = 1f;
	[Export] private float steerSpeed = 2f;
	
	public override void _Ready() {
		
	}
	
	public override void _PhysicsProcess(double delta) {
		bool boost = Input.IsActionPressed("Boost");
		float actualSpeed = boost ? boostSpeed : speed;
		float movementSpeed = (float)(actualSpeed * delta);
		updatePosition(movementSpeed);

		int steerDir = 0;
		if (Input.IsActionPressed("SteerLeft")) 
			steerDir -= 1;
		if(Input.IsActionPressed("SteerRight")) 
			steerDir += 1;
		float steerAmount = (float)(steerDir * steerSpeed * delta);
		updateRotation(steerAmount);

		if (Input.IsActionPressed("Drop")) {
			handleDrop();
		}
	}

	private static (double longitude, double latitude) getGeoCoordsFromPointOnSphere(Vector3 point) {
		Vector3 normalized = point.Normalized();
		double latitude = Math.Asin(normalized.Y) * (180.0 / Math.PI);
		double longitude = Math.Atan2(normalized.Z, normalized.X) * (180.0 / Math.PI);

		return (longitude, latitude);
	}

	private void handleDrop() {
		var geoCoords = getGeoCoordsFromPointOnSphere(Transform.Basis.Z);
		GD.Print($"longitude: {geoCoords.longitude}, latitude: {geoCoords.latitude}");
	}

	private void updatePosition(float movementSpeed) {
		Transform = Transform.RotatedLocal(Vector3.Up, movementSpeed);
	}

	private void updateRotation(float rotationSpeed) {
		Transform = Transform.RotatedLocal(Vector3.Forward, rotationSpeed);
	}
}
