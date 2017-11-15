package org.crayonlang.libraries.nori2;

import java.util.ArrayList;

import org.crayonlang.interpreter.structs.Value;
import org.crayonlang.interpreter.FastList;

final class NoriHelper {

	private NoriHelper() {}

	public static void addChildToParent(Object child, Object parent) {
		throw new RuntimeException();
	}

	public static void closeWindow(Object window) {
		throw new RuntimeException();
	}

	public static void ensureParentLinkOrder(Object parent, Object[] children) {
		throw new RuntimeException();
	}

	public static Object instantiateElement(int type, Object[] properties) {
		throw new RuntimeException();
	}

	public static Object instantiateWindow(Object[] properties) {
		String title = properties[0].toString();
		FastList size = (FastList) properties[1];
		int width = (Integer) size.items[0].intValue;
		int height = (Integer) size.items[1].intValue;
		return new NoriWindow(title, width, height);
	}

	public static void invalidateElementProperty(int type, Object element, int key, Object value) {
		throw new RuntimeException();
	}

	public static void invalidateWindowProperty(Object window, int key, Object value) {
		throw new RuntimeException();
	}

	public static void showWindow(Object window, Object[] ignored, Object rootElement) {
		throw new RuntimeException();
	}

	public static void updateLayout(Object element, int typeId, int x, int y, int width, int height) {
		throw new RuntimeException();
	}
}
