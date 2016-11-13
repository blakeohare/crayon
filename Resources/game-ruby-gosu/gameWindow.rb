
$libGame_window = nil

$libGame_keyCodeLookup = {
	Gosu::KbBackspace => 8,
	Gosu::KbTab => 9,
	Gosu::KbEnter => 13, # numpad
	Gosu::KbReturn => 13, # keyboard
	Gosu::KbLeftShift => 16,
	Gosu::KbRightShift => 16,
	Gosu::KbLeftControl => 17,
	Gosu::KbRightControl => 17,
	Gosu::KbLeftAlt => 18,
	Gosu::KbRightAlt => 18,
	Gosu::KbEscape => 27,
	Gosu::KbSpace => 32,
	Gosu::KbPageUp => 33,
	Gosu::KbPageDown => 34,
	Gosu::KbEnd => 35,
	Gosu::KbHome => 36,
	Gosu::KbLeft => 37,
	Gosu::KbUp => 38,
	Gosu::KbRight => 39,
	Gosu::KbDown => 40,
	Gosu::KbInsert => 45,
	Gosu::KbDelete => 46,
	Gosu::KbSemicolon => 186,
	Gosu::KbEqual => 187,
	Gosu::KbComma => 188,
	Gosu::KbMinus => 189,
	Gosu::KbPeriod => 190,
	Gosu::KbSlash => 191,
	Gosu::KbBacktick => 192,
	Gosu::KbBracketLeft => 219,
	Gosu::KbBackslash => 220,
	Gosu::KbBracketRight => 221,
	Gosu::KbApostrophe => 222,
}

for i in 0..26
	$libGame_keyCodeLookup[i + Gosu::KbA] = 65 + i
end

for i in 0..12
	$libGame_keyCodeLookup[i + Gosu::KbF1] = 112 + i
end

for i in 0..10
	offset = i == 0 ? 10 : 0
	$libGame_keyCodeLookup[i + Gosu::Kb0 + offset] = 48 + i
	$libGame_keyCodeLookup[i + Gosu::KbNumpad0 + offset] = 48 + i
end

def libGame_setFps(fps)
	# currently ignored
end

def libGame_openWindow(width, height, screenWidth, screenHeight, execId)
	$libGame_window = GameWindow.new(width, height, execId)
	$libGame_window.show
end

def libGame_setWindowTitle(title)
	$libGame_window.setCaption(title)
end

def libGame_getEventsRaw
	output = @userEvents
	@userEvents = []
	return output
end

def libGame_clockTick
	# pass
end

class GameWindow < Gosu::Window
	def initialize(width, height, execId)
		super width, height
		@renderEvents = []
		@renderEventsLength = 0
		@renderImages = []
		@renderTextChars = []
		@userEvents = []
		@execId = execId
		@stopExecution = false
		@mouse_int_x = -1
		@mouse_int_y = -1
	end

	def setCaption(title)
		self.caption = title
	end

	def needs_cursor?
		true
	end

	def update

		self.update_mouse_coords

		status = v_runInterpreter(@execId)
		if status != 2
			self.close	
		end
	end

	def draw
		libGraphics2D_runPipeline(@renderEvents, @renderEventsLength, @renderImages, @renderTextChars)
	end

	def setRenderQueue(events, eventsLength, images, textChars)
		@renderEvents = events
		@renderEventsLength = eventsLength
		@renderImages = images
		@renderTextChars = textChars
	end

	def button_down(id)
		self.key_toggle(id, true)
	end

	def button_up(id)
		self.key_toggle(id, false)
	end

	def update_mouse_coords
		# There are no mouse events triggered on movement, just polling of the current location
		new_mouse_x = self.mouse_x.to_i
		new_mouse_y = self.mouse_y.to_i
		if (new_mouse_x != @mouse_int_x || new_mouse_y != @mouse_int_y)
			@mouse_int_x = new_mouse_x
			@mouse_int_y = new_mouse_y
			@userEvents.push(v_buildRelayObj(32, new_mouse_x, new_mouse_y, 0, 0, nil))
		end
	end

	def key_toggle(id, isDown)
		case id
			when Gosu::MsLeft, Gosu::MsRight
				code = 32
				code += 1 if !isDown
				code += 2 if id == Gosu::MsRight
				self.update_mouse_coords
				@userEvents.push(v_buildRelayObj(code, @mouse_int_x, @mouse_int_y, 0, 0, nil))
			else
				code = $libGame_keyCodeLookup[id]

				if code != nil
					@userEvents.push(v_buildRelayObj(isDown ? 16 : 17, code, 0, 0, 0, nil))
				end
		end

	end

end
