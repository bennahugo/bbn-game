/**
 * Interface for a sub menu in our game
 * @author Benjamin
 */
public interface SubMenu {
	/**
	 * Prints out the menu according to the active state of the component
	 */
	public void printMenu();
	/**
	 * Reads user input from the driver per iteration
	 * @param input user input as an integer
	 * @return true iff user's option was correct
	 */
	public boolean readInput(int input);
	/**
	 * Checks if the menu has reached its final state. If this method returns true the program will terminate
	 * @return true if final state was reached
	 */
	public boolean isTerminated();
}
