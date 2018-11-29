package org.crayonlang.libraries.game;

import java.awt.Canvas;
import java.awt.Dimension;
import java.awt.Graphics2D;
import java.awt.RenderingHints;
import java.awt.event.KeyAdapter;
import java.awt.event.KeyEvent;
import java.awt.event.MouseEvent;
import java.awt.event.MouseListener;
import java.awt.event.MouseMotionListener;
import java.awt.event.WindowAdapter;
import java.awt.event.WindowEvent;
import java.awt.image.BufferStrategy;
import java.awt.image.BufferedImage;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.Timer;
import java.util.TimerTask;

import javax.swing.JFrame;
import javax.swing.JPanel;

import org.crayonlang.interpreter.*;
import org.crayonlang.interpreter.structs.*;

public final class GameWindow {

    public static GameWindow INSTANCE = null;
    public static double FPS = 60;

    private JFrame frame;
    private BufferStrategy strategy = null;
    private int gameWidth;
    private int gameHeight;
	private int executionContextId;

    private int screenWidth;
    private int screenHeight;
    private BufferedImage virtualScreen;
    private JPanel canvasHost;
    private KeyAdapter keyAdapter;
    private GameWindowMouseListener mouseListener;

    private GameWindow(double fps, int gameWidth, int gameHeight, int pixelWidth, int pixelHeight, int executionContextId) {

        INSTANCE = this;
		this.executionContextId = executionContextId;

		int dpi = java.awt.Toolkit.getDefaultToolkit().getScreenResolution();
		pixelWidth = pixelWidth * dpi / 96;
		pixelHeight = pixelHeight * dpi / 96;

        this.gameWidth = gameWidth;
        this.gameHeight = gameHeight;
        this.screenWidth = -1;
        this.screenHeight = -1;

        virtualScreen = new BufferedImage(gameWidth, gameHeight, BufferedImage.TYPE_INT_ARGB);

        frame = new JFrame("Crayon Window");
        frame.setSize(pixelWidth, pixelHeight);

        canvasHost = (JPanel) frame.getContentPane();
        canvasHost.setPreferredSize(new Dimension(pixelWidth, pixelHeight));
        canvasHost.setLayout(null);

		// TODO: even though this returns null, it's because it catches an exception.
		// Would prefer to check in a more robust way to shave a few CPU cycles from startup time.
		java.awt.Image icon = ResourceReader.loadImageFromLocalFile("icon.png");

        frame.setVisible(true);
		frame.setIconImage(icon);

        frame.addWindowListener(new WindowAdapter() { 
            @Override 
            public void windowClosing(WindowEvent e) {
                System.exit(0);
            }
        });

        keyAdapter = new KeyAdapter() {
            @Override
            public void keyPressed(KeyEvent e) {
                handleKeyPress(e.getKeyCode(), true);
            }

            @Override
            public void keyReleased(KeyEvent e) { 
                handleKeyPress(e.getKeyCode(), false);
            }
        };

        mouseListener = new GameWindowMouseListener(this);

        canvasHost.addKeyListener(keyAdapter);
        canvasHost.setFocusTraversalKeysEnabled(false);
        canvasHost.requestFocus();
    }

    public void maybeUpdateCanvas() {
        int actualWidth = canvasHost.getWidth();
        int actualHeight = canvasHost.getHeight();
        if (actualWidth == screenWidth && actualHeight == screenHeight) {
            return;
        }

        canvasHost.removeAll();
        Canvas canvas = new Canvas();
        canvas.setBounds(0, 0, actualWidth, actualHeight);
        canvas.setSize(actualWidth, actualHeight);
        canvasHost.add(canvas);
        canvas.setIgnoreRepaint(true);
        canvas.addKeyListener(keyAdapter);
        canvas.addMouseListener(mouseListener);
        canvas.addMouseMotionListener(mouseListener);

        canvas.createBufferStrategy(2);
        strategy = canvas.getBufferStrategy();

        screenWidth = actualWidth;
        screenHeight = actualHeight;
    }

    public static void initializeScreen(int gameWidth, int gameHeight, int screenWidth, int screenHeight, int executionContextId) {
        GameWindow window = new GameWindow(FPS, gameWidth, gameHeight, screenWidth, screenHeight, executionContextId);
        window.show();
    }

    public void setTitle(String title) {
        frame.setTitle(title);
    }

    private static HashMap<Integer, Integer> keyCodeLookup = null;
    private HashMap<Integer, Integer> getKeyCodeLookup() {
        if (keyCodeLookup == null) {
            keyCodeLookup = new HashMap<Integer, Integer>();

            keyCodeLookup.put(KeyEvent.VK_LEFT, 37);
            keyCodeLookup.put(KeyEvent.VK_UP, 38);
            keyCodeLookup.put(KeyEvent.VK_RIGHT, 39);
            keyCodeLookup.put(KeyEvent.VK_DOWN, 40);

            keyCodeLookup.put(KeyEvent.VK_SPACE, 32);
            keyCodeLookup.put(KeyEvent.VK_ENTER, 13);
            keyCodeLookup.put(KeyEvent.VK_TAB, 9);
            keyCodeLookup.put(KeyEvent.VK_ESCAPE, 27);
            keyCodeLookup.put(KeyEvent.VK_BACK_SPACE, 8);
            keyCodeLookup.put(KeyEvent.VK_PRINTSCREEN, 44);
            keyCodeLookup.put(KeyEvent.VK_HOME, 36);
            keyCodeLookup.put(KeyEvent.VK_END, 35);
            keyCodeLookup.put(KeyEvent.VK_PAGE_UP, 33);
            keyCodeLookup.put(KeyEvent.VK_PAGE_DOWN, 34);
            keyCodeLookup.put(KeyEvent.VK_INSERT, 45);
            keyCodeLookup.put(KeyEvent.VK_DELETE, 46);
            keyCodeLookup.put(KeyEvent.VK_PAUSE, 19);
            
            keyCodeLookup.put(KeyEvent.VK_NUM_LOCK, 144);
            keyCodeLookup.put(KeyEvent.VK_CAPS_LOCK, 20);
            keyCodeLookup.put(KeyEvent.VK_SCROLL_LOCK, 145);

            keyCodeLookup.put(KeyEvent.VK_SEMICOLON, 186);
            keyCodeLookup.put(KeyEvent.VK_QUOTE, 222);
            keyCodeLookup.put(KeyEvent.VK_PERIOD, 190);
            keyCodeLookup.put(KeyEvent.VK_COMMA, 188);
            keyCodeLookup.put(KeyEvent.VK_SLASH, 191);
            keyCodeLookup.put(KeyEvent.VK_OPEN_BRACKET, 219);
            keyCodeLookup.put(KeyEvent.VK_CLOSE_BRACKET, 221);
            keyCodeLookup.put(KeyEvent.VK_BACK_SLASH, 229);
            keyCodeLookup.put(KeyEvent.VK_MINUS, 189);
            keyCodeLookup.put(KeyEvent.VK_EQUALS, 187);

            keyCodeLookup.put(KeyEvent.VK_CONTROL, 17);
            keyCodeLookup.put(KeyEvent.VK_SHIFT, 16);
            keyCodeLookup.put(KeyEvent.VK_ALT, 18);

            for (int i = 0; i < 26; ++i) {
                keyCodeLookup.put(KeyEvent.VK_A + i, 65 + i);
            }

            for (int i = 0; i < 10; ++i) {
                keyCodeLookup.put(KeyEvent.VK_0 + i, 48 + i);
            }

            for (int i = 0; i < 12; ++i) {
                keyCodeLookup.put(KeyEvent.VK_F1 + i, 112 + i);
            }
        }
        return keyCodeLookup;
    }

    private void timerTick() {

        InterpreterResult vmResult = TranslationHelper.runInterpreter(this.executionContextId, false);
        if (vmResult.status == 1 || // finished
			vmResult.status == 3) { // uncaught error
            frame.setVisible(false);
            System.exit(0);
        }

		maybeUpdateCanvas();
		Graphics2D virtualG = (Graphics2D) virtualScreen.createGraphics();
		RenderEngine.render(virtualG, gameWidth, gameHeight);

		Graphics2D realG = (Graphics2D) strategy.getDrawGraphics();
		realG.setRenderingHint(
			RenderingHints.KEY_INTERPOLATION, 
			RenderingHints.VALUE_INTERPOLATION_NEAREST_NEIGHBOR);
		realG.drawImage(
			virtualScreen, 0, 0, screenWidth, screenHeight,
			0, 0, virtualScreen.getWidth(), virtualScreen.getHeight(),
			null);

		realG.dispose();
		strategy.show();
    }

    public void show() {
        Timer timer = new Timer();
        timer.scheduleAtFixedRate(new TimerTask() {
            @Override
            public void run() {
                timerTick();
            }
        }, 0, (int)(1000 / FPS));

        frame.setVisible(true);
    }

    private ArrayList<PlatformRelayObject> eventQueue = new ArrayList<PlatformRelayObject>();

    public ArrayList<PlatformRelayObject> pumpEventQueue() {
        ArrayList<PlatformRelayObject> output = eventQueue;
        eventQueue = new ArrayList<PlatformRelayObject>();
        return output;
    }

    private Integer convertKeyCode(int keyCode) {
        Integer value = getKeyCodeLookup().get(keyCode);
        if (value == null) {
            switch (keyCode) {
                case 192: return 192; // backtick
                case 525: return 93; // menu
                default: return null;
            }
        }
        return value;
    }

    private final HashMap<Integer, Boolean> pressedKeys = new HashMap<>();

    private void handleKeyPress(int keyCode, boolean down) {
        Integer key = convertKeyCode(keyCode);
        if (key != null) {
            int keyValue = key;
            Boolean status = pressedKeys.get(key);
            if (down && status != null && status) {
                // Pressing and holding a key will result in a key repeat. Suppress this.
                return;
            }
            pressedKeys.put(key, down);
            eventQueue.add(new PlatformRelayObject(
                down ? 16 : 17,
                keyValue,
                0, 0, 0.0, null));
        }
    }

    private void handleMouseEvent(boolean isClick, boolean isDown, boolean isLeft, int x, int y) {
        int gameX = x * gameWidth / screenWidth;
        int gameY = y * gameHeight / screenHeight;
        if (isClick) {
            eventQueue.add(new PlatformRelayObject(33 + (isLeft ? 0 : 2) + (isDown ? 0 : 1), gameX, gameY, 0, 0.0, null));
        } else {
            eventQueue.add(new PlatformRelayObject(32, gameX, gameY, 0, 0.0, null));
        }
    }

    private static class GameWindowMouseListener implements MouseListener, MouseMotionListener {
        private GameWindow gw;
        GameWindowMouseListener(GameWindow gw) {
            this.gw = gw;
        }

        @Override
        public void mouseClicked(MouseEvent e) {}
        @Override
        public void mouseEntered(MouseEvent e) {}
        @Override
        public void mouseExited(MouseEvent e) {}
        @Override
        public void mouseDragged(MouseEvent e) {}

        @Override
        public void mousePressed(MouseEvent e) {
            int button = e.getButton();
            if (button == MouseEvent.BUTTON1 || button == MouseEvent.BUTTON3) {
                gw.handleMouseEvent(true, false, button == MouseEvent.BUTTON1, e.getX(), e.getY());
            }
        }

        @Override
        public void mouseReleased(MouseEvent e) {
            int button = e.getButton();
            if (button == MouseEvent.BUTTON1 || button == MouseEvent.BUTTON3) {
                gw.handleMouseEvent(true, true, button == MouseEvent.BUTTON1, e.getX(), e.getY());
            }
        }

        @Override
        public void mouseMoved(MouseEvent e) {
            gw.handleMouseEvent(false, false, false, e.getX(), e.getY());
        }
    }
    
    public static Object _setRenderQueueImpl(Object[] args) {
		org.crayonlang.libraries.game.RenderEngine.setRenderQueues(
			(int[]) args[0],
			(int) args[1],
			(Object[][]) args[2],
			(ArrayList<Integer>) args[3]);
		return null;
    }

    private static java.lang.reflect.Method getMethod(String name) {
		try {
			java.lang.reflect.Method[] methods = GameWindow.class.getMethods();
			for (int i = 0; i < methods.length; ++i) {
				if (name.equals(methods[i].getName())) {
					return methods[i];
				}
			}
		} catch (Exception e) {
			throw new RuntimeException(e);
		}
		throw new RuntimeException("Could not find method: " + name);
    }

    public static HashMap<String, java.lang.reflect.Method> getCallbackFunctions() {
		HashMap<String, java.lang.reflect.Method> lookup = new HashMap<>();
		lookup.put("set-render-data", getMethod("_setRenderQueueImpl"));
		return lookup;
    }
}
