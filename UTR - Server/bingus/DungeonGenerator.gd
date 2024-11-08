extends Node

var rng = RandomNumberGenerator.new()
var mapWidth = 10
var mapHeight = 6
var array = []
var totalRoomCount = 10 # does not include starting room
var currRoomCount = 0
const WALL_TILE_ID = Vector2(8, 7)
const ROOM_TILE_ID = Vector2(7, 3)

func _ready():
	rng.randomize()
func setCellNum(x,y,value):
	array[y][x]=value
func getCellValue(x,y): #returns the number in the cell
	if ((x<=mapWidth-1)&&(x>=0)&&(y<=mapHeight-1)&&(y>=0)):
		return array[y][x]
	else:
		return -1
func checkCardinalAdjacents(x,y): #does not include the selected cell
	var adjaCounter = 0
	if (getCellValue(x+1,y)>0):
		adjaCounter+=1
	if (getCellValue(x-1,y)>0):
		adjaCounter+=1
	if (getCellValue(x,y+1)>0):
		adjaCounter+=1
	if (getCellValue(x,y-1)>0):
		adjaCounter+=1
		
	if (adjaCounter>0):
		return true
	else:
		return false
	
func generate(_map):
	array=[]
	currRoomCount=0
	for i in range(mapHeight):
		array.append([])
		for j in range(mapWidth):
			array[i].append(0)

	#set a random cell to 1
	setCellNum(rng.randi_range(0,mapWidth-1),rng.randi_range(0,mapHeight-1),1)
	
	while (currRoomCount<=totalRoomCount):
		#select a random cell to check
		var selectedX = rng.randi_range(0,mapWidth-1)
		var selectedY = rng.randi_range(0,mapHeight-1)
		
		#check if any adjacents
		if (checkCardinalAdjacents(selectedX,selectedY)&&(getCellValue(selectedX,selectedY))==0):
			setCellNum(selectedX,selectedY,1)
			currRoomCount+=1
		#if so, set the selected cell to 1
	populate_tilemap()
	# Print the array
	for o in range(mapHeight):
		print(array[o])
	print("~~~")
	return array
	
	
func populate_tilemap():
	for y in range(mapHeight):
		for x in range(mapWidth):
			if getCellValue(x,y)==1:
				get_node("/root/Node2D/TileMapLayer").set_cell(Vector2i(x,y), 0, WALL_TILE_ID)
			if getCellValue(x,y)==0:
				get_node("/root/Node2D/TileMapLayer").set_cell(Vector2i(x,y), 0, ROOM_TILE_ID)
