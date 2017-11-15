package org.crayonlang.libraries.nori2;

import javax.swing.JFrame;
import javax.swing.JPanel;

public class NoriWindow {
	
	private JFrame frame;
	private JPanel contentHost;
	
	public NoriWindow(String title, int width, int height) {
		this.frame = new JFrame("Nori Window");
		this.frame.setSize(width, height);
		this.contentHost = (JPanel) this.frame.getContentPane();
	}
}
