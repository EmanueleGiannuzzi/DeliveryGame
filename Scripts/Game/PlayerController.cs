using Godot;

public partial class PlayerController : Node3D {
	[Export] private GpuParticles3D rocketParticles;
	[Export] private AudioStreamPlayer rocketSFX;
	[Export] private float rocketAttenuation = -2f;
	
	[Export] private float speed = 0.25f;
	[Export] private float boostSpeed = 1f;
	[Export] private float steerSpeed = 2f;

	private bool _paused = false;
	public bool Paused {
		get => _paused;
		set {
			_paused = value;
			rocketParticles.Emitting = !_paused;
			rocketSFX.VolumeDb = _paused ? 0f : rocketAttenuation;
		}
	}

	public delegate void OnPackageDropped(float latitude, float longitude);
	public event OnPackageDropped OnPackageDroppedEvent;
	public override void _Ready() {
		rocketSFX.VolumeDb = rocketAttenuation;
	}

	public override void _PhysicsProcess(double delta) {
		if(Paused)
			return;
		
		bool boost = Input.IsActionPressed("Boost");
		if (Input.IsActionJustPressed("Boost")) {
			rocketSFX.VolumeDb -= rocketAttenuation;
		}
		else if (Input.IsActionJustReleased("Boost")) {
			rocketSFX.VolumeDb += rocketAttenuation;
		}
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

		if (Input.IsActionJustPressed("Drop")) {
			handleDrop();
		}
	}

	private static (float longitude, float latitude) getGeoCoordsFromPointOnSphere(Vector3 point) {
		Vector3 normalized = point.Normalized();
		float latitude = Mathf.Asin(normalized.Y) * (180f / Mathf.Pi);
		float longitude = Mathf.Atan2(normalized.Z, normalized.X) * (180f / Mathf.Pi);

		return (longitude, latitude);
	}

	private void handleDrop() {
		var geoCoords = getGeoCoordsFromPointOnSphere(Transform.Basis.Z);
		OnPackageDroppedEvent?.Invoke(geoCoords.latitude, geoCoords.longitude);
	}

	private void updatePosition(float movementSpeed) {
		Transform = Transform.RotatedLocal(Vector3.Down, movementSpeed);
	}

	private void updateRotation(float rotationSpeed) {
		Transform = Transform.RotatedLocal(Vector3.Forward, rotationSpeed);
	}
}
