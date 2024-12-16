extends CanvasLayer

var flipFlop=1
var flipInc=0

func _on_timer_timeout() -> void:
	%Title.position.y += flipFlop+(flipFlop*(0.25*flipInc))
	flipInc += 1
	if (flipInc==10):
		flipFlop = -1*flipFlop
		flipInc=0
		
	pass # Replace with function body.
