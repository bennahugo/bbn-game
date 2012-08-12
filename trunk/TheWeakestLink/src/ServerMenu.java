import java.net.InetAddress;

/**
 * Server for the Weakest Link Game
 * @author Benjamin
 */
public class ServerMenu implements SubMenu, SocketListener {
	enum ServerState {SS_LOBBY,SS_GAME_ACTIVE,SS_VOTING,SS_SUDDENDEATH,SS_GAME_COMPLETE};
	static final String QUESTIONS_DATABASE = "QuestionsDB.xml";	
	ServerState myState = ServerState.SS_LOBBY;
	
	ConnectedClients myClients;
	QuestionGenerator myQuestionDB;
	UDPSocket socket;
	public ServerMenu() throws Exception
	{
		myClients = new ConnectedClients();
		myQuestionDB = new QuestionGenerator(QUESTIONS_DATABASE);
		socket = new UDPSocket(this,ProtocolInformation.SERVER_PORT_NUMBER);
	}
	
	@Override
	public void printMenu() {
		System.out.println("\nGAME SERVER");
		switch (myState){
		case SS_LOBBY:
			System.out.println("The server is waiting for more players to join before the round can begin.");
			System.out.println("1. Get the number of players that have joined already");
			System.out.println("2. Get the network statistics of players that has joined already");
			System.out.println("3. Exit server and terminate all connections");
		}
	}

	@Override
	public boolean readInput(int input) {
		switch (myState){
		case SS_LOBBY:
			switch (input){
			case 1:
				System.out.println("Number of players joined: " + myClients.getNumberOfConnectedClients());
				return true;
			case 2:
				for (int i = 0; i < myClients.getNumberOfConnectedClients(); ++i)
					System.out.println(myClients.getClientAtIndex(i).toString());
				return true;
			case 3:
				myState = ServerState.SS_GAME_COMPLETE;
				return true;
			}
		}
		return false;
	}

	@Override
	public boolean isTerminated() {
		return myState == ServerState.SS_GAME_COMPLETE;
	}

	@Override
	public void onIncommingData(InetAddress clientAddress, int port, String data) {
		String[] tokens = data.split(" ");
		try
		{
		switch (myState){
		case SS_LOBBY:
			switch (Integer.parseInt(tokens[0]))
			{
			case ProtocolInformation.CONNECT_REQUEST:
				if (myClients.establishNewConnection(clientAddress.getAddress(), port, tokens[1]))
					socket.sendData(clientAddress, new Integer(port), String.valueOf(ProtocolInformation.CONNECT_ACCEPT));
				else
					socket.sendData(clientAddress, new Integer(port), String.valueOf(ProtocolInformation.CONNECT_REJECT) + " Already connected.");
				break;
			case ProtocolInformation.READY_FOR_MATCH:
				myClients.playerIndicatedReadyness(tokens[1]);
				socket.sendData(clientAddress, new Integer(port), String.valueOf(ProtocolInformation.READY_FOR_MATCH_RECEIVED) + " " + tokens[1] + " is ready to start the match");
			}
		}
		} catch (Exception e) { /*BOGUS MESSAGE*/ }
	}
}
