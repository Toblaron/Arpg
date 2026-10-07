# smoke_test.gd – plays the main level without a window and checks the core loop works:
# scene loads, UI is wired, the attack kills enemies, loot drops, pickup fills the
# inventory, clicking a slot equips the item.
#
# Run from the project folder (build the C# first):
#   godot --headless --path . -s res://tests/smoke_test.gd
# Prints PASS/FAIL per check and exits with code 0 only if everything passed.
extends SceneTree

var _failures := 0


func _check(ok: bool, what: String) -> void:
	print(("PASS  " if ok else "FAIL  ") + what)
	if not ok:
		_failures += 1


func _wait_physics(frames: int) -> void:
	for i in frames:
		await physics_frame


func _press_attack() -> void:
	Input.action_press("attack")
	await _wait_physics(2)
	Input.action_release("attack")
	await create_timer(0.5).timeout  # longer than the player's attack cooldown


func _filled_slots(inventory: Node) -> Array:
	return inventory.get_children().filter(func(c): return c is Button and c.text != "")


func _init() -> void:
	_run.call_deferred()


func _run() -> void:
	var main_path: String = ProjectSettings.get_setting("application/run/main_scene", "")
	_check(main_path == "res://scenes/Level.tscn", "main scene is Level.tscn (got '%s')" % main_path)
	_check(InputMap.has_action("attack"), "input action 'attack' exists")

	var level: Node = load("res://scenes/Level.tscn").instantiate()
	root.add_child(level)
	current_scene = level
	await _wait_physics(3)

	var player: Node2D = level.get_node("Player")
	var enemies := get_nodes_in_group("Enemy")
	var inventory: Node = level.get_node("UI/Inventory")
	var bar: ProgressBar = level.get_node("UI/HealthUI/HealthBar")
	_check(player.is_in_group("Player"), "Player.cs is attached (player is in the 'Player' group)")
	_check(enemies.size() == 4, "4 enemies in the level (got %d)" % enemies.size())
	_check(is_equal_approx(bar.value, 100.0), "health bar starts full (got %.1f)" % bar.value)
	_check(inventory.get_children().filter(func(c): return c is Button).size() == 20, "inventory built 20 slots")

	# Pull every enemy next to the player and swing until they're dead.
	var offsets := [Vector2(30, 0), Vector2(-30, 0), Vector2(0, 30), Vector2(0, -30)]
	for i in enemies.size():
		enemies[i].global_position = player.global_position + offsets[i]
	for swing in 4:
		await _press_attack()
	await _wait_physics(3)
	var alive := get_nodes_in_group("Enemy").filter(func(e): return is_instance_valid(e) and not e.is_queued_for_deletion())
	_check(alive.is_empty(), "attack killed all enemies (%d left)" % alive.size())
	_check(bar.value < 100.0, "enemies damaged the player and the bar dropped (%.1f)" % bar.value)

	var items := level.get_children().filter(func(n): return n is Area2D and n.get_script() != null \
		and n.get_script().resource_path.ends_with("Item.cs"))
	_check(not items.is_empty(), "loot dropped (%d items on the floor)" % items.size())

	# Loot can land under the player and be picked up during the fight, so compare counts.
	var before := _filled_slots(inventory).size()
	var on_floor := items.filter(func(n): return is_instance_valid(n) and not n.is_queued_for_deletion())
	if not on_floor.is_empty():
		var first: Node2D = on_floor[0]
		player.global_position = first.global_position + Vector2(200, 0)
		await _wait_physics(3)
		player.global_position = first.global_position  # walk onto it
		await _wait_physics(5)
	var filled := _filled_slots(inventory)
	_check(filled.size() > before or (on_floor.is_empty() and before > 0),
		"walking over items puts them in the inventory (%d -> %d slots filled)" % [before, filled.size()])

	if not filled.is_empty():
		var slot: Button = filled[0]
		print("      equipping: ", slot.text)
		slot.pressed.emit()  # same as clicking the slot
		await _wait_physics(2)
		_check(slot.text == "", "clicking the slot equipped the item (slot is now empty)")

	print("SMOKE TEST %s (%d failed)" % ["PASSED" if _failures == 0 else "FAILED", _failures])
	quit(0 if _failures == 0 else 1)
