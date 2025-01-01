using Godot;
using Godot.Collections;
using Array = Godot.Collections.Array;

[GlobalClass, Tool]
public partial class CitiesDataResource : Resource {
	[Export(PropertyHint.File, "*.txt,")] public string JsonFileLocation { get; set; } = "";
	private bool _loadJsonButton;

	[Export]
	public bool LoadJsonButton {
		get => _loadJsonButton;
		set {
			if (value) {
				onLoadJsonButton();
				_loadJsonButton = false;
			}
		}
	}

	[ExportCategory("Data")]
	[Export] public Dictionary<string, Array<float>> citiesData { get; private set; } = new ();

	private void onLoadJsonButton() {
		loadJson(JsonFileLocation);
	}

	private void loadJson(string filePath) {
		try {
			var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
			if (file == null) {
				GD.PushError($"Failed to open file: {filePath}");
				return;
			}

			var jsonData = file.GetAsText();
			var parseResult = Json.ParseString(jsonData);

			var jsonObj = parseResult.AsGodotDictionary<string, Array>();
			if (jsonObj.TryGetValue("features", out var jsonCitiesData)) {
				extractCityData(jsonCitiesData);
				GD.Print("JSON loaded successfully!");
			} else {
				GD.PushError("Invalid JSON structure: 'features' key missing");
			}
		} catch (System.Exception e) {
			GD.PushError($"Error loading JSON: {e.Message}");
		}
	}

	private void extractCityData(Array jsonCitiesData) {
		this.citiesData.Clear();

		foreach (var featureObj in jsonCitiesData) {
			var feature = featureObj.AsGodotDictionary<string, Variant>();
			if (feature.ContainsKey("properties") &&
				feature.TryGetValue("geometry", out var value)) {
				var properties = feature["properties"].AsGodotDictionary<string, Variant>();
				var geometry = value.AsGodotDictionary<string, Variant>();

				var cityName = properties.TryGetValue("name", out var nameObj) ? nameObj.AsString() : "Unknown";
				var coordinates = geometry.ContainsKey("coordinates") ? geometry["coordinates"].AsGodotArray<float>() : null;

				if (coordinates != null) {
					GD.Print($"City name: {cityName}");
					if(!citiesData.ContainsKey(cityName))
						this.citiesData.Add(cityName, coordinates);
				}
			}
		}
	}
}
