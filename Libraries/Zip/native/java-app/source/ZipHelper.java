package org.crayonlang.libraries.zip;

import org.crayonlang.interpreter.structs.*;

import java.io.DataOutputStream;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.io.IOException;
import java.io.Reader;
import java.util.ArrayList;
import java.util.List;
import java.util.Map;
import java.util.zip.ZipInputStream;
import java.util.zip.ZipEntry;

final class ZipHelper {
    private ZipHelper() { }

    public static Object createZipReader(int[] intArray) {
        int length = intArray.length;
        byte[] byteArray = new byte[length];
        for (int i = 0; i < length; ++i) {
            byteArray[i] = (byte) intArray[i];
        }
        InputStream stream = new java.io.ByteArrayInputStream(byteArray);
        try {
            ZipInputStream zis = new ZipInputStream(stream);
            return zis;
        } catch (Exception e) {
            return null;
        }
    }

    private static final byte[] BUFFER = new byte[400];

    public static void readNextZipEntry(Object zipArchiveObj, int index, boolean[] boolsOut, String[] nameOut, ArrayList<Integer> bytesOut) {
        boolsOut[0] = true;
        ZipInputStream zis = (ZipInputStream) zipArchiveObj;
        ZipEntry ze;
        try {
            ze = zis.getNextEntry();
        } catch (IOException ioe) {
            boolsOut[0] = false;
            return;
        }
        boolean isDone = ze == null;
        boolsOut[1] = !isDone;
        if (isDone) return;
        boolsOut[2] = false; // TODO: how to check this?
        nameOut[0] = ze.getName();
        
        try {
            int bytesRead = 0;
            int i, b;
            do {
                bytesRead = zis.read(BUFFER);
                for (i = 0; i < bytesRead; ++i) {
                    b = BUFFER[i];
                    bytesOut.add(b & 255);
                }
            } while (bytesRead > 0);
        } catch (IOException ioe) {
            boolsOut[0] = false;
        }
    }
}
