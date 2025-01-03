using Godot;
using System;


public partial class MainMenu : Control {
	[Export] private CanvasLayer HUD;
	[Export] private QuestSystem questSystem;
	[Export] private Label pausedLabel;
	
	[ExportCategory("UI")]
	[Export] private Button playButton;
	[Export] private Slider volumeSlider;
	[Export] private Label volumeLabel;
	[Export] private float startVolume = 50f;
	[Export] private Label difficultyLabel;
	[Export] private CanvasItem difficultyGroup;
	

	private int masterBusIndex;
	private int currentDifficulty = 0;
	

	private float currentVolume => (float)volumeSlider.Value;

	public override void _Ready() {
		pausedLabel.Visible = false;
		
		masterBusIndex = AudioServer.GetBusIndex("Master");
		
		playButton.GrabFocus();
		setVolume(currentVolume);
		setDifficulty(currentDifficulty);
		
		showMenu();
	}

	private void onNextDiffButtonPressed() {
		currentDifficulty++;
		if (currentDifficulty >= Enum.GetNames(typeof(QuestSystem.Difficulty)).Length) {
			currentDifficulty = 0;
		}
		setDifficulty(currentDifficulty);
	}

	private void onPrevDiffButtonPressed() {
		currentDifficulty--;
		if (currentDifficulty <= 0) {
			currentDifficulty = Enum.GetNames(typeof(QuestSystem.Difficulty)).Length - 1;
		}
		setDifficulty(currentDifficulty);
	}

	private void setDifficulty(int difficulty) {
		difficultyLabel.Text = Enum.GetName(typeof(QuestSystem.Difficulty), difficulty)?.Replace("_", " ");
	}

	private void onPlayButtonPressed() {
		showGame();
		if (!questSystem.IsGameStarted) {
			questSystem.SetDifficulty((QuestSystem.Difficulty)currentDifficulty);
			questSystem.NewGame();
			hideStartMenuItems();
			pausedLabel.Visible = true;
		}
	}

	private void hideStartMenuItems() {
		playButton.Text = "Resume";
		difficultyGroup.Visible = false;
	}
	
	private void onQuitButtonPressed() {
		GetTree().Quit();
	}

	private void onVolumeChanged(float volume) {
		setVolume(volume);
	}

	private void setVolume(float volume) {
		volumeLabel.Text = $"Volume: {(int)volume}";
		
		const float minDb = -60.0f;
		const float maxDb = 0.0f;
		float volumeDb = Mathf.Lerp(minDb, maxDb, volume / 100.0f);
		AudioServer.SetBusVolumeDb(masterBusIndex, volumeDb);
	}

	private void showGame() {
		HUD.Visible = true;
		this.Visible =  false;
		if (questSystem.IsGameStarted) {
			questSystem.SetPaused(false);
		}
	}

	private void showMenu() {
		HUD.Visible = false;
		this.Visible = true;
		if (questSystem.IsGameStarted) {
			questSystem.SetPaused(true);
		}
	}

	public override void _Process(double delta) {
		if (questSystem.IsGameStarted && Input.IsActionJustPressed("MainMenu")) {
			if (this.Visible) {
				showGame();
			}
			else {
				showMenu();
			}
		}
	}
}
