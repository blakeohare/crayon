def libGraphics2D_runPipeline(events, eventsLength, images, textChars)
	i = 0
	while i < eventsLength do
		case events[i]
			when 1
				Gosu::draw_rect(
					events[i + 1],
					events[i + 2],
					events[i + 3],
					events[i + 4],
					Gosu::Color.argb(
						events[i + 8],
						events[i + 5],
						events[i + 6],
						events[i + 7]))
			else
				puts "draw something else: " + events[i].to_s
		end
		i += 16
	end
end


def libGraphics2D_render(events, eventsLength, images, textChars)
	$libGame_window.setRenderQueue(events, eventsLength, images, textChars)
end

def libGraphics2D_flip(img, flipHorizontal, flipVertical)
	# TODO: this
	return img
end

def libGraphics2D_scale(img, flipHorizontal, flipVertical)
	# TODO: this
	return img
end
