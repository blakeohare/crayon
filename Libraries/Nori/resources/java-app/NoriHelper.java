package org.crayonlang.libraries.nori;

import java.awt.Color;
import java.awt.Component;
import java.awt.Dimension;
import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;
import java.util.ArrayList;
import javax.swing.JButton;
import javax.swing.JComponent;
import javax.swing.JLabel;
import javax.swing.JPanel;
import org.crayonlang.interpreter.structs.Value;
import org.crayonlang.interpreter.Interpreter;
import org.crayonlang.interpreter.FastList;

final class NoriHelper {

	private NoriHelper() {}

	public static void addChildToParent(Object childObj, Object parentObj) {
		JPanel jp = (JPanel) parentObj;
		JComponent jc = (JComponent)  childObj;
		jp.add(jc, null);
	}

	public static void closeWindow(Object window) {
		throw new RuntimeException();
	}

	public static void ensureParentLinkOrder(Object parent, Object[] children) {
		Component[] actualChildren = ((JPanel)parent).getComponents();
		if (actualChildren.length == children.length) {
			int validUntil = 0;
			for (int i = 0; i < children.length; ++i) {
				if (actualChildren[i] == children[i]) {
					validUntil = i + 1;
				} else {
					break;
				}
			}
			if (validUntil == children.length) return;
		}
		throw new RuntimeException();
	}

	public static Object instantiateElement(int type, ElementProperties properties) {
		JComponent jc = null;
		switch (type) {
			case 1:
				jc = new JPanel();
				jc.setBackground(new Color(properties.bg_red, properties.bg_green, properties.bg_blue, properties.bg_alpha));
				break;
			case 3:
				JPanel jp = new JPanel();
				jp.setLayout(null);
				jc = jp;
				break;
			case 4:
				JButton btn = new JButton(properties.misc_string_0);
				jc = btn;
				break;
			case 5:
				JLabel lbl = new JLabel(properties.misc_string_0);
				lbl.setVerticalAlignment(JLabel.TOP);
				jc = lbl;
				break;
			default:
				throw new RuntimeException("not implemented");
		}
		
		jc.setBounds(properties.render_left, properties.render_top, properties.render_width, properties.render_height);
		
		return jc;
	}

	public static Object instantiateWindow(WindowProperties properties) {
		String title = properties.title;
		int width = properties.width;
		int height = properties.height;
		return new NoriWindow(title, width, height);
	}

	public static void invalidateElementProperty(int type, Object element, int key, Object value) {
		switch (type) {
			// Button
			case 4:
				switch (key) {
					// misc_string_0
					case 21: ((JButton)element).setText(value.toString()); return;
				}
				break;
			// Label
			case 5:
				switch (key) {
					// misc_string_0
					case 21: ((JLabel)element).setText(value.toString()); return;
				}
				break;
		}
		throw new RuntimeException();
	}

	public static void invalidateWindowProperty(Object window, int key, Object value) {
		throw new RuntimeException();
	}

	public static void showWindow(Object wObj, Object[] ignored, Object rootElement, int execId) {
		NoriWindow window = (NoriWindow) wObj;
		window.setExecId(execId);
		window.setContent((JComponent) rootElement);
		window.show();
	}

	public static void updateLayout(Object obj, int typeId, int x, int y, int width, int height) {
		JComponent jc = (JComponent) obj;
		jc.setBounds(x, y, width, height);
	}
	
	private static Value eventHandlerCallbackValue;
	private static FastList eventHandlerArgs;
	private static Value[] argsWrapper = new Value[1];
	
	public static void registerHandlerCallback(Value callbackValue, FastList args) {
		eventHandlerCallbackValue = callbackValue;
		eventHandlerArgs = args;
	}
	
	public static void registerHandler(Object element, int typeId, String handlerType, final int handlerId) {
		switch (typeId + ":" + handlerType) {
			case "4:click":
				((JButton)element).addActionListener(new ActionListener() {
					public void actionPerformed(ActionEvent e) {
						invokeEventHandlerCallback(handlerId);
					}
				});
				break;
			default:
				throw new RuntimeException("Not implemented: " + typeId + ":" + handlerType);
		}
	}
	
	private static void invokeEventHandlerCallback(int id) {
		argsWrapper[0] = Interpreter.buildInteger(id);
		Interpreter.runInterpreterWithFunctionPointer(eventHandlerCallbackValue, argsWrapper);
	}
}
