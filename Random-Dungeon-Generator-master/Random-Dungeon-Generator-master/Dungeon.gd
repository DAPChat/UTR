extends TileMap

func _ready():
	DungeonGenerator.generate(self)


func _on_button_pressed():
	DungeonGenerator.generate(self)
