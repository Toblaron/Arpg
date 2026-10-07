# smoke_test.gd – plays the main level without a window and checks the core loop works:
# scene loads, UI is wired, every enemy type spawns with its look, the belt-scroller stage
# (camera, street bounds, fight zones, waves), the Pimp's Bitch-Slap,
# ranged and charging enemies, melee kills, loot drops, pickup fills the inventory, equip.
#
# Run from the project folder (build the C# first):
#   godot --headless --path . -s res://tests/smoke_test.gd
# Prints PASS/FAIL per check and exits with code 0 only if everything passed.
extends SceneTree

const ENEMY_TYPES := ["street_hustler", "skank", "thug", "playa", "drug_dealer", "baller", "g", "dope_fiend"]
const FAR := Vector2(4000, 4000)

var _failures := 0


func _check(ok: bool, what: String) -> void:
	print(("PASS  " if ok else "FAIL  ") + what)
	if not ok:
		_failures += 1


func _wait_physics(frames: int) -> void:
	for i in frames:
		await physics_frame


func _press(action: String) -> void:
	Input.action_press(action)
	await _wait_physics(2)
	Input.action_release(action)


# Sends a real input event (for nodes that react in _input/_unhandled_input, like the inventory).
func _tap_event(action: String) -> void:
	for pressed in [true, false]:
		var ev := InputEventAction.new()
		ev.action = action
		ev.pressed = pressed
		Input.parse_input_event(ev)
		await process_frame


func _alive(group: String) -> Array:
	return get_nodes_in_group(group).filter(func(e): return is_instance_valid(e) and not e.is_queued_for_deletion())


func _filled_slots(inventory: Node) -> Array:
	return inventory.get_children().filter(func(c): return c is Button and c.text != "")


func _hp(node: Node) -> float:
	return node.get_node("Health").get("Current")


func _init() -> void:
	_run.call_deferred()
	# Watchdog: a script error stops _run without quitting; don't hang forever.
	create_timer(240.0).timeout.connect(func():
		print("SMOKE TEST FAILED (timed out: a script error or a stuck wait)")
		quit(1))


func _run() -> void:
	var main_path: String = ProjectSettings.get_setting("application/run/main_scene", "")
	_check(main_path == "res://scenes/Level.tscn", "main scene is Level.tscn (got '%s')" % main_path)
	_check(InputMap.has_action("attack") and InputMap.has_action("ability"), "input actions 'attack' and 'ability' exist")

	var level: Node = load("res://scenes/Level.tscn").instantiate()
	var spawner: Node = level.get_node("WaveSpawner")
	root.add_child(level)
	current_scene = level
	await _wait_physics(3)
	_check(_alive("Enemy").is_empty(), "no enemies before the first wave")
	for i in ENEMY_TYPES.size():
		spawner.call("Spawn", ENEMY_TYPES[i], Vector2(100 + 120 * i, 120))
	await _wait_physics(3)

	var player: Node2D = level.get_node("Player")
	var inventory: Node = level.get_node("UI/Inventory")
	var bar: ProgressBar = level.get_node("UI/HealthUI/HealthBar")
	_check(player.is_in_group("Player"), "Player.cs is attached (player is in the 'Player' group)")
	_check(is_equal_approx(bar.value, 100.0), "health bar starts full (got %.1f)" % bar.value)
	_check(inventory.get_children().filter(func(c): return c is Button).size() == 20, "inventory built 20 slots")

	# Enemy roster: one of each type, each with its own look and name tag.
	var by_type := {}
	for e in get_nodes_in_group("Enemy"):
		by_type[e.get("TypeId")] = e
	_check(by_type.size() == ENEMY_TYPES.size(), "spawner made one of each enemy type (%d types)" % by_type.size())
	for t in ENEMY_TYPES:
		var e: Node = by_type.get(t)
		_check(e != null and e.get_node_or_null("Look") != null and e.get_node_or_null("Body") == null,
			"enemy '%s' spawned with its look" % t)
	_check(is_equal_approx(_hp(by_type["thug"]), 60.0), "Thug has his own health (%.0f)" % _hp(by_type["thug"]))

	# From here the player is made unkillable so a death reload can't end the test early.
	player.get_node("Health").call("Reset", 100000.0)
	# Park everyone far away, spaced apart (stacked bodies shove each other around).
	var park_all := func():
		var alive := _alive("Enemy")
		for i in alive.size():
			alive[i].global_position = player.global_position + FAR + Vector2(i * 120.0, 0)

	# Class: the Pimp's look replaces the placeholder box, and the UI names his ability.
	_check(player.get_node_or_null("Look") != null and player.get_node_or_null("Body") == null,
		"Pimp look replaced the placeholder body")
	var ability_label: Label = level.get_node("UI/AbilityUI")
	_check(ability_label.text.begins_with("Bitch-Slap"), "ability UI shows Bitch-Slap (got '%s')" % ability_label.text)

	# Cane Sweep (J): the Pimp's default attack hits in front, not behind, and makes them flinch back.
	park_all.call()
	await _wait_physics(2)
	var s_front: Node2D = by_type["thug"]
	var s_back: Node2D = by_type["baller"]
	s_front.get_node("Health").call("Reset", 1000.0)
	s_back.get_node("Health").call("Reset", 1000.0)
	s_front.global_position = player.global_position + Vector2(44, 6)
	s_back.global_position = player.global_position + Vector2(-44, 6)
	await _wait_physics(1)
	var sweep_x := s_front.global_position.x
	_check(player.get("AttackName") == "Cane Sweep", "default attack is Cane Sweep (got '%s')" % player.get("AttackName"))
	await _press("attack")
	await _wait_physics(4)
	_check(_hp(s_front) < 1000.0, "Cane Sweep hit the enemy in front (%.0f/1000)" % _hp(s_front))
	_check(s_front.global_position.x > sweep_x + 1.0, "Cane Sweep pushed the enemy back (%.1f px)" % (s_front.global_position.x - sweep_x))
	_check(is_equal_approx(_hp(s_back), 1000.0), "Cane Sweep missed the enemy behind")
	s_front.get_node("Health").call("Reset", 60.0)
	s_back.get_node("Health").call("Reset", 45.0)
	await create_timer(0.5).timeout

	# Bitch-Slap: hits the enemy in front (damage, knockback, stun), not the one behind.
	park_all.call()
	await _wait_physics(2)
	var front: Node2D = by_type["playa"]
	var behind: Node2D = by_type["skank"]
	front.get_node("Health").call("Reset", 1000.0)  # survive the slap so we can see the knockback
	front.global_position = player.global_position + Vector2(40, 0)
	behind.global_position = player.global_position + Vector2(-40, 0)
	await _wait_physics(1)
	var front_x := front.global_position.x
	var behind_max: float = behind.get_node("Health").get("MaxHealth")
	await _press("ability")
	await _wait_physics(6)
	_check(_hp(front) <= 1000.0 - 30.0, "Bitch-Slap dealt triple damage (front enemy %.0f/1000)" % _hp(front))
	_check(front.get("IsStunned"), "Bitch-Slap stunned the enemy")
	_check(front.global_position.x > front_x + 15.0, "Bitch-Slap knocked the enemy back (%.0f px)" % (front.global_position.x - front_x))
	_check(is_equal_approx(_hp(behind), behind_max), "enemy behind the Pimp was not slapped (%.0f/%.0f)" % [_hp(behind), behind_max])
	_check(ability_label.text.ends_with("s"), "ability is on cooldown after use (got '%s')" % ability_label.text)
	front.get_node("Health").call("Reset", 34.0)

	# Drug Dealer: keeps his distance and throws bottles that hurt.
	park_all.call()
	await _wait_physics(2)
	var dealer: Node2D = by_type["drug_dealer"]
	dealer.global_position = player.global_position + Vector2(200, 0)
	var hp_before := _hp(player)
	var saw_bottle := false
	for i in 150:  # 2.5 s
		await physics_frame
		saw_bottle = saw_bottle or not _alive("EnemyProjectile").is_empty()
	_check(saw_bottle, "Drug Dealer threw a bottle")
	_check(_hp(player) < hp_before, "a bottle hit the player (%.0f damage)" % (hp_before - _hp(player)))
	_check(dealer.global_position.distance_to(player.global_position) > 90.0, "Drug Dealer kept his distance")

	# G: winds up, then charges.
	park_all.call()
	await _wait_physics(2)
	var g: Node2D = by_type["g"]
	g.global_position = player.global_position + Vector2(160, 0)
	var charged := false
	for i in 120:  # 2 s
		await physics_frame
		charged = charged or g.get("IsCharging")
	_check(charged, "G charged at the player")

	# Everyone else: pull them in and swing until they're all dead.
	await create_timer(1.3).timeout  # let stuns wear off
	var hp_melee := _hp(player)
	for swing in 30:
		var alive := _alive("Enemy")
		if alive.is_empty():
			break
		for i in alive.size():
			alive[i].global_position = player.global_position + Vector2(30.0, -16.0 + 32.0 * i / max(1, alive.size() - 1))
		await _press("attack")
		await create_timer(0.45).timeout  # longer than the attack cooldown
	await _wait_physics(3)
	_check(_alive("Enemy").is_empty(), "melee killed every enemy (%d left)" % _alive("Enemy").size())
	_check(_hp(player) < hp_melee, "enemies fought back in melee")

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

	# Kroner: every kill dropped money, and walking near it picked it up.
	await create_timer(0.6).timeout
	var kroner: int = player.get("Kroner")
	_check(kroner > 0, "enemies dropped kroner and the player picked them up (%d kr)" % kroner)
	_check((level.get_node("UI/KronerUI") as Label).text.ends_with(" kr"), "kroner counter shows '%s'" % (level.get_node("UI/KronerUI") as Label).text)

	# Stage: Final Fight-style camera, street bounds and fight zones.
	var director: Node = level.get_node("StageDirector")
	var cam: Camera2D = level.get_node("Camera2D")
	var wave_label: Label = level.get_node("UI/WaveUI").get_child(0)
	var floor_top: float = director.get("FloorTop")
	player.global_position = Vector2(player.global_position.x, 40.0)  # try to stand in the buildings
	await _wait_physics(2)
	_check(player.global_position.y >= floor_top - 0.5, "player can't leave the street (y %.0f, top %.0f)" % [player.global_position.y, floor_top])

	director.call("WarpTo", 800.0)
	await _wait_physics(2)
	var cam_x := cam.global_position.x
	_check(cam_x > 500.0, "camera scrolled with the player (x %.0f)" % cam_x)
	Input.action_press("ui_left")
	await create_timer(0.8).timeout
	Input.action_release("ui_left")
	await _wait_physics(2)
	_check(is_equal_approx(cam.global_position.x, cam_x), "camera never scrolls back")
	_check(player.global_position.x >= cam_x - 288.0 + 20.0, "player can't walk off the left edge (x %.0f)" % player.global_position.x)

	# Fight zone 1: the screen locks and wave 1 (3 enemies) comes in from the sides.
	var zones: Array = director.get("EncounterAt")
	director.call("WarpTo", zones[0] + 5.0)
	await _wait_physics(2)
	_check(director.get("Locked"), "screen locked at the first fight zone")
	_check(spawner.get("CurrentWave") == 1 and spawner.get("Remaining") == 3, "wave 1 started with 3 enemies")
	var locked_x := cam.global_position.x
	await create_timer(2.5).timeout  # they enter one by one
	_check(_alive("Enemy").size() == 3, "wave 1 enemies entered (%d)" % _alive("Enemy").size())
	_check(wave_label.text.contains("WAVE 1"), "wave counter shows '%s'" % wave_label.text)
	var view_left := locked_x - 288.0
	_check(_alive("Enemy").all(func(e): return e.global_position.y >= floor_top - 0.5), "enemies stay on the street")
	Input.action_press("ui_right")
	await create_timer(0.6).timeout
	Input.action_release("ui_right")
	_check(is_equal_approx(cam.global_position.x, locked_x), "camera stays locked during the fight")
	_check(player.global_position.x <= locked_x + 288.0 - 20.0, "player can't leave the locked screen")
	for swing in 20:
		var alive := _alive("Enemy")
		if alive.is_empty():
			break
		for i in alive.size():
			alive[i].global_position = player.global_position + Vector2(30.0, -16.0 + 32.0 * i / max(1, alive.size() - 1))
		await _press("attack")
		await create_timer(0.45).timeout
	await _wait_physics(3)
	_check(not director.get("Locked"), "clearing the fight unlocks the screen")
	_check(director.get("ShowGo"), "GO -> is shown")

	# Fight zone 2 sends the next wave (4 enemies).
	director.call("WarpTo", zones[1] + 5.0)
	await _wait_physics(2)
	_check(director.get("Locked") and spawner.get("CurrentWave") == 2 and spawner.get("Remaining") == 4,
		"second fight zone sends wave 2 with 4 enemies (wave %d, %d left)" % [spawner.get("CurrentWave"), spawner.get("Remaining")])

	# Play out the rest of the street: every fight zone, then the stage is cleared at the end.
	for z in range(1, zones.size()):
		director.call("WarpTo", zones[z] + 5.0)
		for f in 3:
			await physics_frame
		var guard := 0
		while director.get("Locked") and guard < 120:
			guard += 1
			await create_timer(0.3).timeout
			for e in _alive("Enemy"):
				e.call("TakeDamage", 99999.0)
	_check(spawner.get("CurrentWave") == 8, "all fight zones played: 8 waves (got %d)" % spawner.get("CurrentWave"))
	director.call("WarpTo", float(director.get("StageLength")) - 100.0)
	for f in 3:
		await physics_frame
	_check(not director.get("Cleared"), "the stage isn't cleared before the boss is beaten")

	# Inventory is hidden during play and toggles with I.
	_check(not inventory.visible, "inventory starts hidden")
	await _tap_event("inventory")
	_check(inventory.visible, "I opens the inventory")
	await _tap_event("inventory")
	_check(not inventory.visible, "I closes the inventory")

	# Boss: Pantelåneren waits at the end of the street.
	_check(director.get("BossFight") and director.get("Locked"), "boss fight started, screen locked")
	var boss: Node2D = director.get("Boss")
	await create_timer(1.0).timeout
	_check(level.get_node("UI/BossUI").visible, "boss health bar is shown")
	var boss_max: float = boss.get_node("Health").get("MaxHealth")
	_check(boss_max >= 400.0, "boss is tough (%.0f HP)" % boss_max)
	# Stand next to him: he winds up a slam.
	var slammed := false
	for i in 400:
		player.global_position = boss.global_position + Vector2(-40, 0)
		await physics_frame
		if boss.get("IsSlamming"):
			slammed = true
			break
	_check(slammed, "boss winds up his sledgehammer slam")
	var enemies_before := _alive("Enemy").size()
	boss.call("TakeDamage", boss_max * 0.45)
	await _wait_physics(3)
	_check(boss.get("BackupCalls") == 1 and _alive("Enemy").size() > enemies_before, "hurt boss calls for backup (%d enemies)" % _alive("Enemy").size())
	var kroner_before_boss: int = player.get("Kroner")
	boss.call("TakeDamage", 999999.0)
	await _wait_physics(3)
	_check(director.get("Cleared"), "beating the boss clears the stage")
	_check(_alive("Enemy").is_empty(), "the boss's backup leaves with him")
	await create_timer(2.0).timeout
	_check(int(player.get("Kroner")) >= kroner_before_boss + 300, "boss paid out (%d -> %d kr)" % [kroner_before_boss, player.get("Kroner")])

	# Matkroken opens after the stage-clear banner and pauses the game.
	var shop: Control = level.get_node("UI/Shop")
	await create_timer(1.5).timeout
	_check(shop.visible and paused, "Matkroken shop opened and the game is paused")
	player.call("AddKroner", 2000)
	var wallet: int = player.get("Kroner")
	var max_hp: float = player.get_node("Health").get("MaxHealth")
	_check(shop.call("Buy", "brunost"), "bought Brunost")
	_check(int(player.get("Kroner")) == wallet - 150, "Brunost cost 150 kr (%d -> %d)" % [wallet, player.get("Kroner")])
	_check(is_equal_approx(float(player.get_node("Health").get("MaxHealth")), max_hp + 20.0), "Brunost gave +20 max health")
	shop.call("Buy", "fiskeboller")
	_check(player.call("UpgradeLevel", "fiskeboller") == 1, "bought Fiskeboller (damage upgrade)")
	_check(shop.call("Buy", "brunost") and int(player.get("Kroner")) == wallet - 150 - 200 - 240, "second Brunost costs more (240 kr)")
	player.get_node("Health").call("ApplyDamage", 50.0)
	shop.call("Buy", "polse")
	_check(is_equal_approx(float(player.get_node("Health").get("Current")), float(player.get_node("Health").get("MaxHealth"))), "Pølse i lompe healed to full")
	player.call("TrySpend", int(player.get("Kroner")))
	_check(not shop.call("Buy", "skrapelodd"), "can't buy without kroner")

	# On to stage 2: back to the start of the street, gear and upgrades kept.
	shop.call("Continue")
	await _wait_physics(3)
	_check(not shop.visible and not paused, "shop closed and the game resumed")
	_check(director.get("Stage") == 2 and not director.get("Cleared"), "stage 2 started")
	_check(player.global_position.x < 200.0, "player is back at the start of the street (x %.0f)" % player.global_position.x)
	_check(player.call("UpgradeLevel", "brunost") == 2, "upgrades carried over to stage 2")
	director.call("WarpTo", zones[0] + 5.0)
	for f in 3:
		await physics_frame
	_check(director.get("Locked") and spawner.get("CurrentWave") == 9, "stage 2 fights carry on the wave count (wave %d)" % spawner.get("CurrentWave"))

	print("SMOKE TEST %s (%d failed)" % ["PASSED" if _failures == 0 else "FAILED", _failures])
	quit(0 if _failures == 0 else 1)
