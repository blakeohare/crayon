
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
	return []
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
	end

	def setCaption(title)
		self.caption = title
	end

	def update
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

end

