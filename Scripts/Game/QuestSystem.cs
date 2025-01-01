using System.Linq;
using Godot;
using Godot.Collections;

public partial class QuestSystem : Node {
	private const float EARTH_RADIUS = 6371f;

	[Export] private CitiesDataResource citiesResource;
	private Dictionary<string, Array<float>> citiesData => citiesResource.citiesData;
	[Export] PlayerController player;
	
	[ExportCategory("Current Quest")]
	[Export] private string currentQuestName;
	[Export] private float[] currentQuestCoords = new float[2];

	public override void _Ready() {
		GD.Randomize();
		player.OnPackageDroppedEvent += onPackageDroppedEvent;
		
		generateNewQuest();
	}

	private void generateNewQuest() {
		int citiesSize = citiesData.Count;
		int randomCityId = (int)(GD.Randi() % citiesSize);
		var randomCity = citiesData.ElementAt(randomCityId);
		currentQuestName = randomCity.Key;
		currentQuestCoords[0] = randomCity.Value[0];
		currentQuestCoords[1] = randomCity.Value[1];

		onNewQuestGenerated();
	}

	private void onNewQuestGenerated() {
		GD.Print($"New Quest: {currentQuestName}");
		//TODO: Update UI
	}
	
	private static float haversineDistance(float lat1, float lon1, float lat2, float lon2, float radius) {
		lat1 = Mathf.DegToRad(lat1);
		lon1 = Mathf.DegToRad(lon1);
		lat2 = Mathf.DegToRad(lat2);
		lon2 = Mathf.DegToRad(lon2);

		float dLat = lat2 - lat1;
		float dLon = lon2 - lon1;
		
		float a = Mathf.Sin(dLat / 2) * Mathf.Sin(dLat / 2) +
						  Mathf.Cos(lat1) * Mathf.Cos(lat2) *
						  Mathf.Sin(dLon / 2) * Mathf.Sin(dLon / 2);
		float c = 2f * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));

		return radius * c;
	}

	private void onPackageDroppedEvent(float latitude, float longitude) {
		GD.Print($"latitude: {latitude}, longitude: {longitude}");
		
		float distanceFromTarget = haversineDistance(latitude, longitude, currentQuestCoords[0], currentQuestCoords[1], EARTH_RADIUS);
		GD.Print($"distanceFromTarget: {distanceFromTarget}");
		
		generateNewQuest();
	}
	
}
