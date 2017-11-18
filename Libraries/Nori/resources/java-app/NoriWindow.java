package org.crayonlang.libraries.nori;

import javax.swing.JComponent;
import javax.swing.JFrame;
import javax.swing.JPanel;

public class NoriWindow {
	
	private JFrame frame;
	private JPanel contentHost;
	
	public NoriWindow(String title, int width, int height) {
		this.frame = new JFrame("Nori Window");
		this.frame.setSize(width, height);
		this.contentHost = (JPanel) this.frame.getContentPane();
		this.contentHost.setLayout(null);
		this.contentHost.setBounds(0, 0, width, height);
	}
	
	public void show() {
		this.frame.show();
	}
	
	public void setContent(JComponent content) {
		this.contentHost.add(content, null);
	}
}
