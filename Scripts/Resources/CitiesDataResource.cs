using Godot;
using Godot.Collections;
using Array = Godot.Collections.Array;
using Cities = Godot.Collections.Dictionary<string, Godot.Collections.Array>;

[GlobalClass, Tool]
public partial class CitiesDataResource : Resource {
	[Export(PropertyHint.File, "*.txt,")] public string JsonFileLocation { get; set; } = "";
	private bool _loadJsonButton;

	[Export]
	private bool loadJsonButton {
		get => _loadJsonButton;
		set {
			if (value) {
				onLoadJsonButton();
				_loadJsonButton = false;
			}
		}
	}

	[ExportCategory("Data")]
	[Export] public Dictionary<string, Cities> citiesData { get; private set; } = new ();

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
			if (feature.TryGetValue("properties", out var propertiesObj)) {
				var properties = propertiesObj.AsGodotDictionary<string, Variant>();
				if (properties.TryGetValue("featurecla", out var featureCLAObj) &&
					properties.TryGetValue("latitude", out var latitudeObj) &&
					properties.TryGetValue("longitude", out var longitudeObj) &&
					properties.TryGetValue("name", out var nameObj) &&
					properties.TryGetValue("sov_a3", out var countryIdObj)
					) {
					Array coordinates = new Array();
					coordinates.Add(latitudeObj.AsSingle());
					coordinates.Add(longitudeObj.AsSingle());
					coordinates.Add(countryIdObj.AsString().ToLower());
					string name = nameObj.AsString();
					
					string featureCLA = featureCLAObj.AsString();
					if(!citiesData.ContainsKey(featureCLA))
						citiesData.Add(featureCLA, new Cities());
					
					if(!citiesData[featureCLA].ContainsKey(name))
						this.citiesData[featureCLA].Add(name, coordinates);
				}
			}
		}
	}
}
