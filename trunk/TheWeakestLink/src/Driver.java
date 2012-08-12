import java.util.Iterator;
import java.util.Scanner;
import java.io.InputStream;
import java.nio.*;
import java.nio.channels.*;
import java.nio.charset.*;
/**
 * Driver for The Weakest Link game
 * @author Benjamin
 */
public class Driver {
	enum program_type {NO_SELECTION,SERVER_MODE,CLIENT_MODE,REGISTRY_SERVER_MDOE};
	static program_type myMode = program_type.NO_SELECTION;
	
	static Scanner inputReader = new Scanner(System.in);
	static NonBlockingReader nonBlockingReader = new NonBlockingReader(inputReader);
	static Charset charset = Charset.forName("ISO-8859-15");
    static CharsetDecoder decoder = charset.newDecoder();
    static String nickname = "Billy Bob";
	static SubMenu activeMenu;
	static boolean submenuInterruptTriggered = false;
	/**
	 * Call this method if there the program's state changes and the menu needs to be redrawn
	 */
	public static void subMenuInterrupted()
	{
		submenuInterruptTriggered = true;
		nonBlockingReader.clearBuffer();
	}
	
	public static void main(String [] args) throws Exception
	{
		System.out.println("*******************************************************************");
		System.out.println("THE WEAKEST LINK");
		System.out.println("");
		System.out.println("An multiplayer network game immitating the popular TV gameshow");
		System.out.println("By Benjamin Hugo, Brandon Talbot and Nathan Floor");
		System.out.println("*******************************************************************");
		
		//Ask the user what mode the program should start in:
		while (myMode == program_type.NO_SELECTION)
		{
			System.out.println("MAIN MENU\n Welcome, "+nickname+"\n1. Enter Game Host Mode");
			System.out.println("2. Enter Player Mode");
			System.out.println("3. Enter Master Server Mode");
			System.out.println("4. Change your nickname");
			System.out.print("\nEnter your choice:\n>");
			String option = inputReader.nextLine();
			if (option.trim().length() == 0) continue;
			if (option.matches("[0-9]+"))
			{
				int iOption = Integer.parseInt(option);
				switch (iOption)
				{
				case 1:
					myMode = program_type.SERVER_MODE;
					break;
				case 2:
					myMode = program_type.CLIENT_MODE;
					break;
				case 3:
					myMode = program_type.REGISTRY_SERVER_MDOE;
					break;
				case 4:					
					break;
				default:
					System.out.println("\nINVALID OPTION NUMBER. PICK A NUMBER 1 - 3.\n");
					break;
				}
			}
			else System.out.println("\nINVALID INPUT. PICK A NUMBER 1 - 3.\n");
		}
		
		//Start the component the user selected:
		switch (myMode)
		{
		case SERVER_MODE:
			activeMenu = new ServerMenu();
			break;
		case CLIENT_MODE:
			activeMenu = new ClientMenu();
			break;
		case REGISTRY_SERVER_MDOE:
			throw new Exception("Functionality not implemented yet");
		}
		
		nonBlockingReader.start();
		//Now display the active component's menu till it finishes:
		while (!activeMenu.isTerminated())
		{
			try {Thread.sleep(50,0);} catch (InterruptedException e) { break; }
			if (submenuInterruptTriggered)
			{
				submenuInterruptTriggered = false;
				continue;
			}
			
			activeMenu.printMenu();
			System.out.print("\nEnter your choice:\n>");
			
			String option = null;
			while (!submenuInterruptTriggered && (option == null || option.trim().length() == 0))
			{
				option = nonBlockingReader.getNextLine();
				if (option != null)
					if (option.trim().length() == 0)
						System.out.print(">");
				try {Thread.sleep(50,0);} catch (InterruptedException e) { break; }
			}
			if (option == null) continue;
			if (option.matches("[0-9]+"))
			{
				int iOption = Integer.parseInt(option);
				if (!activeMenu.readInput(iOption))
					System.out.println("\nINVALID OPTION NUMBER. PICK A NUMBER FROM THE LIST.\n");
			}
			else System.out.println("\nINVALID INPUT. PICK A NUMBER FROM THE LIST.\n");			
		}
		
		//Finally we are done
		System.out.println("GOOD BYE!");
		System.exit(0);
	}
	
	
}
