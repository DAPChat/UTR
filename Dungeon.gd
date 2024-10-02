extends TileMap

@export var mapWidth = 10
@export var mapHeight = 10

@export var minRoomSize = 4 #5
@export var maxRoomSize = 4 #5

func _ready():
	DungeonGenerator.generate(self, mapWidth, mapHeight, minRoomSize, maxRoomSize)


func _on_button_pressed():
	DungeonGenerator.generate(self, mapWidth, mapHeight, minRoomSize, maxRoomSize)
