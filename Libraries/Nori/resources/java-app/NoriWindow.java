package org.crayonlang.libraries.nori;

import java.awt.event.WindowEvent;
import java.awt.event.WindowListener;
import javax.swing.JComponent;
import javax.swing.JFrame;
import javax.swing.JPanel;
import org.crayonlang.interpreter.Interpreter;

public class NoriWindow implements WindowListener {
	
	private JFrame frame;
	private JPanel contentHost;
	private int executionContextIdOnClose;
	
	public NoriWindow(String title, int width, int height) {
		this.frame = new JFrame("Nori Window");
		this.frame.setSize(width, height);
		this.contentHost = (JPanel) this.frame.getContentPane();
		this.contentHost.setLayout(null);
		this.contentHost.setBounds(0, 0, width, height);
		this.frame.addWindowListener(this);
	}
	
	public void show() {
		this.frame.show();
	}
	
	public void setContent(JComponent content) {
		this.contentHost.add(content, null);
	}
	
	public void setExecId(int execId) {
		this.executionContextIdOnClose = execId;
	}

	// TODO: figure out why windowClosed isn't getting fired. This ideally should run
	// after the window is completely closed, rather than when it's about to.
	public void windowClosing(WindowEvent e) {
		Interpreter.v_runInterpreter(this.executionContextIdOnClose);
	}

	public void windowClosed(WindowEvent e) { }	
	public void windowOpened(WindowEvent e) { }
	public void windowIconified(WindowEvent e) { }
	public void windowDeiconified(WindowEvent e) { }
	public void windowActivated(WindowEvent e) { }
	public void windowDeactivated(WindowEvent e) { }
	public void windowGainedFocus(WindowEvent e) { }
	public void windowLostFocus(WindowEvent e) { }
	public void windowStateChanged(WindowEvent e) { }
}
