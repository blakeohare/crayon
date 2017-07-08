package org.crayonlang.libraries.graphics2dtext;

import java.awt.Color;
import java.awt.Font;
import java.awt.FontMetrics;
import java.awt.Graphics2D;
import java.awt.GraphicsEnvironment;
import java.awt.image.BufferedImage;
import java.util.ArrayList;
import org.crayonlang.interpreter.structs.*;
import org.crayonlang.interpreter.ResourceReader;

final class Graphics2DTextHelper {
	
	private Graphics2DTextHelper() { }
	
	public static Object createNativeFont(int fontType, int fontClass, String fontId, int size, boolean isBold, boolean isItalic) {
		int style = Font.PLAIN;
		if (isBold) style |= Font.BOLD;
		if (isItalic) style |= Font.ITALIC;
		
		if (fontType == 1) { // font embedded resource
			return ResourceReader.getFontResource(fontId, size, style);
		} else if (fontType == 3) { // system font
			return new Font(fontId, style, size);
		} else {
			throw new RuntimeException("Not implemented.");
		}
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
		int margin = height / 8;
		int width = fontMetrics.stringWidth(text) + margin * 2;
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
