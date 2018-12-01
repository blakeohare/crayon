package org.crayonlang.libraries.imageencoder;

import java.awt.image.BufferedImage;
import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.util.ArrayList;
import javax.imageio.ImageIO;
import org.crayonlang.interpreter.structs.*;

public class ImageEncoderUtil {

	public static int encode(Object imageObj, int formatEnum, ArrayList<Value> output, Value[] bytesAsValues) {
		BufferedImage image = (BufferedImage) imageObj;
		String format;
		switch (formatEnum) {
			case 1: format = "png"; break;
			case 2:
				format = "jpg";
				
				// Re-encode the JPEG to remove any possible alpha channel, which gives corrupted colors.
				int width = image.getWidth();
				int height = image.getHeight();
				BufferedImage newImage = new BufferedImage(width, height, BufferedImage.TYPE_INT_RGB);
				int[] pixels = image.getRGB(0, 0, width, height, null, 0, width);
				newImage.setRGB(0, 0, width, height, pixels, 0, width);
				image = newImage;
				break;
			default: throw new RuntimeException();
		}
		
		byte[] bytes;
		try {
			ByteArrayOutputStream outputStream = new ByteArrayOutputStream();
			ImageIO.write(image, format, outputStream);
			bytes = outputStream.toByteArray();
		} catch (IOException ioe) {
			return 1;
		}
		
		int i = 0;
		int length = bytes.length;
		int uByte;
		while (i < length) {
			uByte = bytes[i++];
			if (uByte < 0) uByte += 256;
			output.add(bytesAsValues[uByte]);
		}
		
		return 0;
	}
}
