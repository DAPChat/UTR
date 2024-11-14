extends Node

var rng = RandomNumberGenerator.new()
var mapWidth = 10
var mapHeight = 10
var array = []
var totalRoomCount = 10 # does not include starting room
var currRoomCount = 0
var bossRoomSpawned = 0
const WALL_TILE_ID = Vector2(8, 7)
const ROOM_TILE_ID = Vector2(7, 3)
const START_TILE_ID = Vector2(5, 5)
const BOSS_TILE_ID = Vector2(3, 8)
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
	
func checkOctolinearAdjacents(x,y): #does not include the selected cell
	var adjaCounter = 0
	if (getCellValue(x+1,y)>0):
		adjaCounter+=1
	if (getCellValue(x-1,y)>0):
		adjaCounter+=1
	if (getCellValue(x,y+1)>0):
		adjaCounter+=1
	if (getCellValue(x,y-1)>0):
		adjaCounter+=1
	if (getCellValue(x+1,y+1)>0):
		adjaCounter+=1
	if (getCellValue(x-1,y+1)>0):
		adjaCounter+=1
	if (getCellValue(x+1,y-1)>0):
		adjaCounter+=1
	if (getCellValue(x-1,y-1)>0):
		adjaCounter+=1
	if (adjaCounter>0):
		return true
	else:
		return false

func checkCardinalAdjacentsTwoUnitsAway(x,y): #does not include the selected cell
	var adjaCounter = 0
	if (getCellValue(x+2,y)>0):
		adjaCounter+=1
	if (getCellValue(x-2,y)>0):
		adjaCounter+=1
	if (getCellValue(x,y+2)>0):
		adjaCounter+=1
	if (getCellValue(x,y-2)>0):
		adjaCounter+=1
	if (adjaCounter>0):
		return true
	else:
		return false

func createSquareWithSideLengthSize(x,y,sideLength,fillValue): #DOES NOT CHECK IF THE CELLS IT SETS TO ARE OUT OF BOUNDS
	if (sideLength==3):
		setCellNum(x,y,fillValue)
		setCellNum(x-1,y,fillValue)
		setCellNum(x+1,y,fillValue)
		setCellNum(x,y-1,fillValue)
		setCellNum(x-1,y-1,fillValue)
		setCellNum(x+1,y-1,fillValue)
		setCellNum(x,y+1,fillValue)
		setCellNum(x-1,y+1,fillValue)
		setCellNum(x+1,y+1,fillValue)
	pass
	
func generate():
	array=[]
	currRoomCount=0
	bossRoomSpawned=0
	for i in range(mapHeight):
		array.append([])
		for j in range(mapWidth):
			array[i].append(0)

	#set a random cell to 2 (starting cell) and make sure it cant spawn on the edges
	setCellNum(rng.randi_range(0+1,mapWidth-1-1),rng.randi_range(0+1,mapHeight-1-1),2)
	
	while (currRoomCount<=totalRoomCount):
		#select a random cell to check
		var selectedX = rng.randi_range(0,mapWidth-1)
		var selectedY = rng.randi_range(0,mapHeight-1)
		
		#check if any adjacents
		if (checkCardinalAdjacents(selectedX,selectedY)&&(getCellValue(selectedX,selectedY))==0):
			setCellNum(selectedX,selectedY,1)
			currRoomCount+=1
		#if so, set the selected cell to 1
		
		
	while (bossRoomSpawned==0):
		var bossRoomCentralX = rng.randi_range(0+1,mapWidth-1-1)
		var bossRoomCentralY = rng.randi_range(0+1,mapHeight-1-1)
		if (!checkOctolinearAdjacents(bossRoomCentralX,bossRoomCentralY)&&checkCardinalAdjacentsTwoUnitsAway(bossRoomCentralX,bossRoomCentralY)&&(getCellValue(bossRoomCentralX,bossRoomCentralY))==0):
			createSquareWithSideLengthSize(bossRoomCentralX,bossRoomCentralY,3,3)
			bossRoomSpawned+=1
			
	populate_tilemap()
	
	return array
	
	
func populate_tilemap():
	return
	for y in range(mapHeight):
		for x in range(mapWidth):
			if getCellValue(x,y)==3:
				get_node("/root/Node2D/TileMapLayer").set_cell(Vector2i(x,y), 0, BOSS_TILE_ID)
			if getCellValue(x,y)==2:
				get_node("/root/Node2D/TileMapLayer").set_cell(Vector2i(x,y), 0, START_TILE_ID)
			if getCellValue(x,y)==1:
				get_node("/root/Node2D/TileMapLayer").set_cell(Vector2i(x,y), 0, WALL_TILE_ID)
			if getCellValue(x,y)==0:
				get_node("/root/Node2D/TileMapLayer").set_cell(Vector2i(x,y), 0, ROOM_TILE_ID)
			
			
