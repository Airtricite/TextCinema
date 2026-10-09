extends Button


# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame


func add():
	var m=get_node("marker").duplicate()
	m.modulate=modulate
	get_parent().get_node("Delete/Markers").add_child(m)
	m.position=Vector2(randf_range(50,get_parent().size.x),randf_range(50,get_parent().size.y))
	m.visible=true;
	pass # Replace with function body.
