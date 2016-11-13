
$libGame_window = nil

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
				code = -1

				case id
					when Gosu::KbEscape then code = 27
				end

				if code > -1
					@userEvents.push(v_buildRelayObj(isDown ? 16 : 17, code, 0, 0, 0, nil))
				end
		end

	end

end

