extends CanvasLayer

var flipFlop=2

func _on_timer_timeout() -> void:
	%Title.position.y += flipFlop
	flipFlop = -1*flipFlop
	
	pass # Replace with function body.
