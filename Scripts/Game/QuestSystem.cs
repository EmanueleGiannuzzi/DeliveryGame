using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using Cities = Godot.Collections.Dictionary<string, Godot.Collections.Array>;

public partial class QuestSystem : Node {
	[Export] private PlayerController playerController;
	[Export] private Node3D player;
	
	private const float EARTH_RADIUS = 6371f;
	private const float PLANET_RADIUS = 100f;

	[Export] private CitiesDataResource citiesResource;
	private Godot.Collections.Dictionary<string, Cities> citiesData => citiesResource.citiesData;
	
	[ExportCategory("Current Quest")]
	[Export] private string currentQuestName;
	[Export] private float[] currentQuestCoords = new float[2];
	
	[Export] private Difficulty difficulty = Difficulty.NORMAL;
	
	[ExportCategory("HUD Members")]
	[Export] private Label questText;
	[Export] private Label timerText;
	[Export] private TextureRect flagSprite;
	[Export(PropertyHint.Dir)] private string flagTexturesDirectory;
	[Export] private Godot.Collections.Dictionary<string, Texture2D> flagTextures;
	[Export(PropertyHint.File)] private string presentsListFile;
	[Export] private Array<string> presentsList;
	
	[ExportCategory("Timer")]
	[Export] private int maxTimeSec = 180;
	[Export] private Timer timer;
	[Export] private Label timerLabel;
	[Export] private Label packagedDroppedLabel;
	
	[ExportCategory("Points")]
	[Export] private Node3D giftLandedPoint;
	[Export] private Node3D lastCorrectDestinationPoint;
	
	public bool IsGameStarted => !timer.IsStopped();
	
	
	public enum Difficulty {
		NORMAL = 0,
		HARD = 1,
		VERY_HARD = 2
	}

	private readonly string[][] difficultyCategories = {
		new[] { "Admin-0 capital" }, //NORMAL
		new[] { "Admin-0 capital", "Admin-1 capital" }, // HARD
		new[] { "Admin-0 capital", "Admin-1 capital", "Populated place" } //VERY_HARD
	};

	public override void _Ready() {
		GD.Randomize();
		playerController.OnPackageDroppedEvent += onPackageDroppedEvent;
		loadFlagTextures();
		loadPresents();
	}

	public void NewGame() {
		generateNewQuest();
		startTimer();
	}

	public void SetPaused(bool paused) {
		timer.SetPaused(paused);
		playerController.Paused = paused;
	}

	private void startTimer() {
		timer.Start(maxTimeSec);
		timer.Timeout += onTimerTimeout;
	}

	public void SetDifficulty(Difficulty _difficulty) {
		this.difficulty = _difficulty;
	}

	private void onTimerTimeout() {
		
		//TODO: End game
	}

	private void loadPresents() {
		using var file = FileAccess.Open(presentsListFile, FileAccess.ModeFlags.Read);
		string content = file.GetAsText();
		presentsList = new Array<string>(content.Split('\n').ToList());
	}

	private void loadFlagTextures() {
		using var dir = DirAccess.Open(flagTexturesDirectory);
		if (dir != null) {
			dir.ListDirBegin();
			string fileName = dir.GetNext();
			while (fileName != "") {
				if (!dir.CurrentIsDir()) {
					string fileNameTrimmed = fileName.TrimSuffix(".png");
					if (fileNameTrimmed.Length != fileName.Length) {
						Texture2D texture = (Texture2D)GD.Load(flagTexturesDirectory + "/" + fileName);
						flagTextures.TryAdd(fileNameTrimmed, texture);
					}
				}
				fileName = dir.GetNext();
			}
		}
		else {
			GD.Print("An error occurred when trying to access the path.");
		}
	}

	private string getRandomCategory() {
		string[] categories = difficultyCategories[(int)difficulty];
		int randomCategoryId = (int)(GD.Randi() % categories.Length);
		return categories[randomCategoryId];
	}

	private void generateNewQuest() {
		string category = getRandomCategory();
		Cities cities = citiesData[category];
		
		int randomCityId = (int)(GD.Randi() % cities.Count);
		randomCityId = 201; //TODO: Remove
		var randomCity = cities.ElementAt(randomCityId);
		currentQuestName = randomCity.Key;
		currentQuestCoords[0] = randomCity.Value[0].AsSingle();
		currentQuestCoords[1] = randomCity.Value[1].AsSingle();

		string countryId = randomCity.Value[2].AsString();

		int randomPresentId = (int)(GD.Randi() % presentsList.Count);
		string randomPresent = presentsList[randomPresentId];
		
		giftLandedPoint.Position = geoCoordsToSpherePos(currentQuestCoords[0], currentQuestCoords[1], PLANET_RADIUS);
		
		onNewQuestGenerated(currentQuestName, currentQuestCoords[0], currentQuestCoords[1], countryId, randomPresent);
	}

	private void onNewQuestGenerated(string cityName, float latitude, float longitude, string countryId, string present) {
		GD.Print($"New Quest: {cityName} [{latitude}, {longitude}]");
		
		questText.Text = $"Deliver {present} to {cityName}";
		flagSprite.Texture = flagTextures[countryId];
		
		
		
		lastCorrectDestinationPoint.Position = geoCoordsToSpherePos(longitude, latitude, PLANET_RADIUS);
	}
	
	public static (float latitude, float longitude) UnitSphereToLatLon(Vector3 point) {
		point = point.Normalized();

		float latitude = Mathf.Asin(point.Y) * 180f / Mathf.Pi; 
		float longitude = Mathf.Atan2(point.Z, point.X) * 180f / Mathf.Pi;

		return (latitude, longitude);
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
	
	private static Vector3 geoCoordsToSpherePos(float latitude, float longitude, float radius) {
		float latRad = latitude * Mathf.Pi / 180f;
		float lonRad = longitude * Mathf.Pi / 180f;

		float x = Mathf.Cos(latRad) * Mathf.Cos(lonRad);
		float y = Mathf.Cos(latRad) * Mathf.Sin(lonRad);
		float z = Mathf.Sin(latRad);

		return new Vector3(x, y, z) * radius;
	}

	private void onPackageDroppedEvent(float latitude, float longitude) {
		GD.Print($"latitude: {latitude}, longitude: {longitude}");
		
		float distanceFromTarget = haversineDistance(latitude, longitude, currentQuestCoords[0], currentQuestCoords[1], EARTH_RADIUS);
		GD.Print($"distanceFromTarget: {distanceFromTarget}");
		
		//TODO: Drop package
		
		generateNewQuest();
		packagedDroppedLabel.Text = $"The gift landed {Mathf.RoundToInt(distanceFromTarget)}km away from its destination";
		removeTextFromPackageLabel(3);
	}

	private async void removeTextFromPackageLabel(float delaySeconds) {
		try {
			Timer _timer = new Timer();
			AddChild(_timer);
			_timer.WaitTime = delaySeconds;
			_timer.OneShot = true;
			_timer.Start();

			await ToSignal(_timer, "timeout");

			packagedDroppedLabel.Text = "";
			_timer.QueueFree();
		}
		catch (Exception e) {
			GD.PushError(e.Message);
		}
	}

	private static string secondsToTimeString(int totalSeconds) {
		int minutes = totalSeconds / 60;
		int seconds = totalSeconds % 60;
		return $"{minutes:D2}:{seconds:D2}";
	}

	public override void _Process(double delta) {
		timerLabel.Text = secondsToTimeString((int)Math.Round(timer.TimeLeft));
		
		var (latitude, longitude) = UnitSphereToLatLon(player.GlobalPosition);
		longitude += 90;
		latitude = -latitude;
		packagedDroppedLabel.Text = $"latitude[{(int)latitude}] longitude[{(int)longitude}] {(Vector3I)player.GlobalPosition}";
	}
	
}
