package org.crayonlang.libraries.graphics2dtext;

import java.awt.Color;
import java.awt.Font;
import java.awt.FontMetrics;
import java.awt.font.TextAttribute;
import java.awt.Graphics2D;
import java.awt.GraphicsEnvironment;
import java.awt.image.BufferedImage;
import java.util.ArrayList;
import java.util.HashMap;
import org.crayonlang.interpreter.structs.*;
import org.crayonlang.interpreter.ResourceReader;

final class Graphics2DTextHelper {
	
	private Graphics2DTextHelper() { }
	
	public static Object createNativeFont(int fontType, int fontClass, String fontId, int size, boolean isBold, boolean isItalic) {
		int style = Font.PLAIN;
		if (isBold) style |= Font.BOLD;
		if (isItalic) style |= Font.ITALIC;
		
		int adjustedSize = size * 3 / 2;
		Font font;
		if (fontType == 1) { // font embedded resource
			font = ResourceReader.getFontResource(fontId, adjustedSize, style);
		} else if (fontType == 3) { // system font
			font = new Font(fontId, style, adjustedSize);
		} else {
			throw new RuntimeException("Not implemented.");
		}
		
		// adjust kerning
		HashMap<TextAttribute, Object> attributes = new HashMap<TextAttribute, Object>();
		attributes.put(TextAttribute.TRACKING, -0.05);
		font = font.deriveFont(attributes);
		
		return font;
	}
	
	public static void addAll(ArrayList<Value> values, Value... toAdd) {
		for (Value value : toAdd) {
			values.add(value);
		}
	}
	
	public static boolean isSystemFontAvailable(String name) {
		String[] fontNames = GraphicsEnvironment.getLocalGraphicsEnvironment().getAvailableFontFamilyNames();
		for (String fontName : fontNames) {
			if (fontName.equals(name)) {
				return true;
			}
		}
		return false;
	}
	
	private static final BufferedImage DUMMY_IMAGE = new BufferedImage(1, 1, BufferedImage.TYPE_INT_ARGB);
	
	public static Object renderText(
		int[] sizeOut,
		Object fontObj,
		int red,
		int green,
		int blue,
		String text) {
		
		Font font = (Font) fontObj;
		FontMetrics fontMetrics = DUMMY_IMAGE.createGraphics().getFontMetrics(font);
		int baselineHeight = fontMetrics.getHeight();
		int height = baselineHeight * 3 / 2;
		int margin = height / 32;
		int width = fontMetrics.stringWidth(text) + margin * 2 + 4;
		BufferedImage image = new BufferedImage(width, height, BufferedImage.TYPE_INT_ARGB);
		Graphics2D g = image.createGraphics();
		Color color = new Color(red, green, blue);
		g.setColor(color);
		g.setFont(font);
		g.drawString(text, margin, baselineHeight);
		sizeOut[0] = width;
		sizeOut[1] = height;
		return image;
	}
}
