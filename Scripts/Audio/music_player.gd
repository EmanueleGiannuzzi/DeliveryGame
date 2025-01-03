extends AudioStreamPlayer

@export var music_tracks: Array[AudioStream]
var rng = RandomNumberGenerator.new()

var last_track_index: int = 0

func next_track_index() -> void:
	last_track_index += 1
	if last_track_index >= music_tracks.size():
		last_track_index = 0

func _ready() -> void:
	rng.randomize()
	last_track_index = rng.randi_range(0, music_tracks.size() - 1)
	finished.connect(on_track_finished)
	play_next()

func play_next() -> void:
	stream = music_tracks[last_track_index]
	play()
	next_track_index()

func on_track_finished() -> void:
	play_next()
