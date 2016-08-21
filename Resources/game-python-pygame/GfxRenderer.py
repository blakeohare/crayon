
_gfxRendererVars = {
  'tempImg': pygame.Surface((10, 10)),
  'events': [],
  'eventsLength': 0,
  'images': [],
  'textChars': [],
  'fontLookup': {},
  'fontCharLookupByStyle': {},
}

def gfxRendererSetData(events, eventsLength, images, textChars):
  _gfxRendererVars['events'] = events
  _gfxRendererVars['eventsLength'] = eventsLength
  _gfxRendererVars['images'] = images
  _gfxRendererVars['textChars'] = textChars

def gfxRender():
  screen = _global_vars['virtual_screen']
  tempImg = _gfxRendererVars['tempImg']
  events = _gfxRendererVars['events']
  eventsLength = _gfxRendererVars['eventsLength']
  images = _gfxRendererVars['images']
  textChars = _gfxRendererVars['textChars']
  tempImgWidth, tempImgHeight = tempImg.get_size()
  screenWidth, screenHeight = screen.get_size()

  if len(textChars) > 0:
    _gfxEnsureCharsLoaded(events, textChars)

  imageIndex = 0
  textIndex = 0
  i = 0
  while i < eventsLength:
    command = events[i]

    if command == 6:
      mask = events[i | 1]
      image = images[imageIndex][0][3]
      imageIndex += 1
      x = events[i | 8]
      y = events[i | 9]
      if mask == 0:
        # basic
        screen.blit(image, (x, y))
      elif mask == 1:
        # slice
        sx, sy, sw, sh = events[i | 2], events[i | 3], events[i | 4], events[i | 5]
        screen.blit(image, (x, y), (sx, sy, sw, sh))
      elif mask == 2:
        # stretch
        screen.blit(image, (x, y, events[i | 6], events[i | 7]))
      elif mask == 3:
        # slice and stretch
        sx, sy, sw, sh = events[i | 2], events[i | 3], events[i | 4], events[i | 5]
        screen.blit(image, (x, y, events[i | 6], events[i | 7]), (sx, sy, sw, sh))
      else:

        alpha = 255
        if (mask & 8) != 0:
          alpha = events[i | 11]

        if alpha > 0:

          w, h = image.get_size()
          if (mask & 1) != 0:
            sx, sy, sw, sh = events[i | 2], events[i | 3], events[i | 4], events[i | 5]
          else:
            sx, sy, sw, sh = 0, 0, w, h

          if (mask & 2) != 0:
            tw, th = events[i | 6], events[i | 7]
          else:
            tw, th = w, h

          if (mask & 4) != 0:
            angle = events[i | 10] / 1048576.0 * -180 / math.pi
          else:
            angle = None

          imgToBlit = None
          if angle != None:
            if mask == 4: # just rotation, not other transforms
              rotated = pygame.transform.rotate(image, angle)
              w, h = rotated.get_size()
              screen.blit(rotated, (x - w // 2, y - h // 2))
            else:
              rotatedTempImg = pygame.Surface((tw, th), pygame.SRCALPHA)
              rotatedTempImg.blit(image, (0, 0, tw, th), (sx, sy, sw, sh))
              rotated = pygame.transform.rotate(rotatedTempImg, angle)
              x -= rotated.get_width() // 2
              y -= rotated.get_height() // 2
              if alpha < 255:
                imgToBlit = rotated
              else:
                screen.blit(rotated, (x, y))
          else:
            imgToBlit = pygame.Surface((tw, th), pygame.SRCALPHA)
            imgToBlit.blit(image, (0, 0, tw, th), (sx, sy, sw, sh))

          if imgToBlit != None:
            w, h = imgToBlit.get_size()

            if w > tempImgWidth or h > tempImgHeight:
              tempImgWidth = max(tempImgWidth, w)
              tempImgHeight = max(tempImgHeight, h)
              tempImg = pygame.Surface((tempImgWidth, tempImgHeight)) # intentionally no alpha channel
            tempImg.blit(screen, (-x, -y, w, h))
            tempImg.blit(imgToBlit, (0, 0))
            tempImg.set_alpha(alpha)
            screen.blit(tempImg, (x, y, w, h), (0, 0, w, h))
    elif command < 3:
      alpha = events[i | 8]
      color = events[i | 5 : i | 8]
      area = events[i | 1: i | 5]
      if alpha == 255:
        if command == 1:
          if area[3] == 1:
            # PyGame has a bug where rectangles of height 1 do not get rendered.
            pygame.draw.line(screen, color, (area[0], area[1]), (area[0] + area[2], area[1]))
          else:
            pygame.draw.rect(screen, color, area)
        elif command == 2:
          pygame.draw.ellipse(screen, color, area)
      elif alpha > 0:
        wh = area[2:]
        t = pygame.Surface(wh) # intenionally no alpha channel
        t.blit(screen, (-area[0], -area[1]))
        if command == 1:
          t.fill(color)
        else:
          pygame.draw.ellipse(t, color, (0, 0, w[0], h[0]))
        t.set_alpha(alpha)
        screen.blit(t, area[:2])
    elif command == 4 or command == 5:

      # 4 -> triangle
      # 5 -> quad
      pts = [
        [events[i | 1], events[i | 2]],
        [events[i | 3], events[i | 4]],
        [events[i | 5], events[i | 6]]]

      if command == 4:
        rgb = events[i | 7 : i | 10]
        alpha = events[i | 10]
      else:
        pts.append([events[i | 7], events[i | 8]])
        rgb = events[i | 9 : i | 12]
        alpha = events[i | 12]

      if alpha == 255:
        pygame.draw.polygon(screen, rgb, pts)
      else:
        pt = pts[0]
        minX = pt[0]
        minY = pt[1]
        maxX = pt[0]
        maxY = pt[1]
        for pt in pts:
          if pt[0] < minX: minX = pt[0]
          if pt[1] < minY: minY = pt[1]
          if pt[0] > maxX: maxX = pt[0]
          if pt[1] > maxY: maxY = pt[1]
        width = maxX - minX
        height = maxY - minY
        if width > tempImgWidth or height > tempImgHeight:
          tempImgWidth = max(tempImgWidth, width)
          tempImgHeight = max(tempImgHeight, height)
          tempImg = pygame.Surface((tempImgWidth, tempImgHeight)) # intentionally no alpha channel
        for pt in pts:
          pt[0] -= minX
          pt[1] -= minY
        tempImg.blit(screen, (0, 0, width, height), (minX, minY, width, height))
        pygame.draw.polygon(tempImg, rgb, pts)
        tempImg.set_alpha(alpha)
        screen.blit(tempImg, (minX, minY, width, height), (0, 0, width, height))

    elif command == 3:
      # line
      ax = events[i | 1]
      ay = events[i | 2]
      bx = events[i | 3]
      by = events[i | 4]
      strokeWidth = events[i | 5]
      rgb = events[i | 6 : i | 9]
      alpha = events[i | 9]
      if alpha == 255:
        pygame.draw.line(screen, rgb, (ax, ay), (bx, by), strokeWidth)
      else:
        width = abs(ax - bx) + strokeWidth * 2
        height = abs(ay - by) + strokeWidth * 2
        minX = min(ax, bx) - strokeWidth
        minY = min(ay, by) - strokeWidth
        if width > tempImgWidth or height > tempImgHeight:
          tempImgWidth = max(tempImgWidth, width)
          tempImgHeight = max(tempImgHeight, height)
          tempImg = pygame.Surface((tempImgWidth, tempImgHeight)) # intentionally no alpha channel
        tempImg.blit(screen, (0, 0, width, height), (minX, minY, width, height))
        pt1 = (ax - minX, ay - minY)
        pt2 = (bx - minX, by - minY)
        pygame.draw.line(tempImg, rgb, pt1, pt2, strokeWidth)
        tempImg.set_alpha(alpha)
        screen.blit(tempImg, (minX, minY, width, height), (0, 0, width, height))
    elif command == 7:
      # text
      styleKey = _gfxGetStyleKey(events, i)
      charLookup = _gfxRendererVars['fontCharLookupByStyle'].get(styleKey)
      x = events[i | 1]
      y = events[i | 2]
      for j in range(events[i | 13]):
        c = textChars[j]
        textIndex += 1
        img = charLookup[c]
        screen.blit(img, (x, y))
        x += img.get_width()

    i += 16

  _gfxRendererVars['tempImg'] = tempImg

class GfxFont:
  def __init__(self, name, isSystem, font12):
    self.name = name
    self.isSystem = isSystem
    self.fonts = { 12 * 1024: font12 }

  def getFont(self, size):
    size = int(size + .9999999999)
    key = int(size * 1024 + .5)
    font = self.fonts.get(key)
    if font == None:
      if self.isSystem:
        font = pygame.font.SysFont(self.name, size)
      else:
        font = pygame.font.Font(self.name, size)
      self.fonts[key] = font
    return font

def _gfxLoadFont(isSystem, name, globalId):
  try:
    if isSystem:
      font12 = pygame.font.SysFont(name, 12)
    else:
      font12 = pygame.font.Font(name, 12)
  except:
    return False

  _gfxRendererVars['fontLookup'][globalId] = GfxFont(name, isSystem, font12)

  return True

def _gfxPushCodePoints(list, string):
  for char in string:
    list.append(ord(char))
  return len(string)

def _gfxEnsureCharsLoaded(events, textChars):
  textCharIndex = 0
  i = 0
  eventLength = len(events)
  while i < eventLength:
    if events[i] == 7:
      key = _gfxGetStyleKey(events, i)
      charLookup = _gfxRendererVars['fontCharLookupByStyle'].get(key)
      if charLookup == None:
        charLookup = {}
        _gfxRendererVars['fontCharLookupByStyle'][key] = charLookup
      textLen = events[i | 13]
      while textLen > 0:
        c = textChars[textCharIndex]
        if charLookup.get(c) == None:
          fontId = events[i | 3]
          charLookup[c] = _gfxRenderChar(
            _gfxRendererVars['fontLookup'][fontId],
            c,
            events[i | 4] / 1024.0, # size
            events[i | 5] == 1, # bold
            events[i | 6] == 1, # italic
            events[i | 7], events[i | 8], events[i | 9]) # RGB
        textCharIndex += 1
        textLen -= 1
    i += 16

def _gfxGetStyleKey(events, i):
  # i | (3 through 11) are font face ID, size, bold, italic, r, g, b, a respectively
  return ','.join(map(str, events[i + 3: i + 11]))

def _gfxRenderChar(fontFace, codePoint, size, isBold, isItalic, r, g, b):
  font = fontFace.getFont(size)
  text = chr(codePoint)
  img = font.render(text, True, (r, g, b))
  return img

def _gfxScale(img, width, height):
  return pygame.transform.scale(img, (width, height))
