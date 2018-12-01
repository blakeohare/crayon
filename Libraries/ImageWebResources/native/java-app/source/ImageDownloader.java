package org.crayonlang.libraries.imagewebresources;

import java.awt.image.BufferedImage;
import java.io.ByteArrayInputStream;
import javax.imageio.ImageIO;
import org.crayonlang.interpreter.structs.*;

public class ImageDownloader {

	public static boolean bytesToImage(Object byteArrayObj, Object[] output) {
		byte[] bytes = (byte[]) byteArrayObj;
		ByteArrayInputStream inputStream = new ByteArrayInputStream(bytes);
		BufferedImage image = null;
		try {
			image = ImageIO.read(inputStream);
		} catch (java.io.IOException e) {
			return false;
		}
		
		output[0] = image;
		output[1] = image.getWidth();
		output[2] = image.getHeight();
		
		return true;
	}
}
