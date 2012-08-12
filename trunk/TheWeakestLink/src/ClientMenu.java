import java.net.Inet4Address;
import java.net.InetAddress;

public class ClientMenu extends Thread implements SubMenu,SocketListener {
	static final int TIMEOUT_CONNECT_MILLIS = 5000;
	enum ClientState {SS_NOT_CONNECTED,SS_DIRECT_CONNECTION_IP_REQUEST_1,SS_DIRECT_CONNECTION_IP_REQUEST_2,
		SS_DIRECT_CONNECTION_IP_REQUEST_3,SS_DIRECT_CONNECTION_IP_REQUEST_4,TRYING_TO_CONNECT,SS_ESTABLISHING_CONNECTION,
		SS_LOBBY,SS_GAME_ACTIVE,SS_VOTING,SS_SUDDENDEATH,SS_GAME_COMPLETE};
	ClientState myState = ClientState.SS_NOT_CONNECTED;
	byte[] serverip = new byte[4];
	UDPSocket socket;
	
	public ClientMenu() throws Exception
	{
		socket = new UDPSocket(this, ProtocolInformation.CLIENT_PORT_NUMBER);
		this.start();
	}
	
	@Override
	public void printMenu() {
		synchronized(myState)
		{
			System.out.println("\nTHE WEAKEST LINK");
			switch (myState){
			case SS_NOT_CONNECTED:
				System.out.println("Establish a connection");
				System.out.println("1. Establish a direct connection");
				System.out.println("2. Exit game");
				break;
			case SS_DIRECT_CONNECTION_IP_REQUEST_1:
				System.out.println("Direct connection");
				System.out.println("Enter first IP address block (0-255)");
				break;
			case SS_DIRECT_CONNECTION_IP_REQUEST_2:
				System.out.println("Direct connection");
				System.out.println("Enter second IP address block (0-255)");
				break;
			case SS_DIRECT_CONNECTION_IP_REQUEST_3:
				System.out.println("Direct connection");
				System.out.println("Enter third IP address block (0-255)");
				break;
			case SS_DIRECT_CONNECTION_IP_REQUEST_4:
				System.out.println("Direct connection");
				System.out.println("Enter fourth IP address block (0-255)");
				break;
			case SS_ESTABLISHING_CONNECTION:
				System.out.println("Waiting for the connection to be established...");
				System.out.println("1. Abort");
				System.out.println("2. Exit game");
				break;
			case SS_LOBBY:
				System.out.println("Players' lobby");
				System.out.println("1. Get a list of players");
				System.out.println("2. Exit game");
				break;
			}
		}
	}

	@Override
	public boolean readInput(int input) {
		synchronized (myState)
		{
			switch (myState){
			case SS_NOT_CONNECTED:
				switch (input){
				case 1:
					myState = ClientState.SS_DIRECT_CONNECTION_IP_REQUEST_1;
					return true;
				case 2:
					myState = ClientState.SS_GAME_COMPLETE;
					return true;
				}
			case SS_DIRECT_CONNECTION_IP_REQUEST_1:
				if (input < 0 || input > 255)
					return false;
				serverip[0] = (byte)input;
				myState = ClientState.SS_DIRECT_CONNECTION_IP_REQUEST_2;
				return true;
			case SS_DIRECT_CONNECTION_IP_REQUEST_2:
				if (input < 0 || input > 255)
					return false;
				serverip[1] = (byte)input;
				myState = ClientState.SS_DIRECT_CONNECTION_IP_REQUEST_3;
				return true;
			case SS_DIRECT_CONNECTION_IP_REQUEST_3:
				if (input < 0 || input > 255)
					return false;
				serverip[2] = (byte)input;
				myState = ClientState.SS_DIRECT_CONNECTION_IP_REQUEST_4;
				return true;
			case SS_DIRECT_CONNECTION_IP_REQUEST_4:
				if (input < 0 || input > 255)
					return false;
				serverip[3] = (byte)input;
				try
				{
					socket.sendData(Inet4Address.getByAddress(serverip), ProtocolInformation.SERVER_PORT_NUMBER, 
						ProtocolInformation.CONNECT_REQUEST + " " + Driver.nickname);
					myState = ClientState.SS_ESTABLISHING_CONNECTION;
					return true;
				}
				catch (Exception e)
				{
					System.out.println("Problem with connection details.");
					return false;
				}
			case SS_ESTABLISHING_CONNECTION:
				switch (input){
				case 1:
					myState = ClientState.SS_NOT_CONNECTED;
					Driver.subMenuInterrupted();
					return true;
				case 2:
					myState = ClientState.SS_GAME_COMPLETE;
					return true;
				}
			case SS_LOBBY:
				switch (input){
				case 1:
					return true;
				case 2:
					myState = ClientState.SS_GAME_COMPLETE;
					return true;
				}
			}
			return false;
		}
	}

	@Override
	public boolean isTerminated() {
		return myState == ClientState.SS_GAME_COMPLETE;
	}
	@Override
	public void run()
	{
		long connectTrySendTime = -1;
		while (!Thread.currentThread().isInterrupted())
		{
			synchronized(myState)
			{
				switch (myState){
				case SS_NOT_CONNECTED:
					connectTrySendTime = -1;
					break;
				case SS_ESTABLISHING_CONNECTION:
					if (connectTrySendTime < 0)
						connectTrySendTime = System.currentTimeMillis();
					else if (System.currentTimeMillis() - connectTrySendTime > TIMEOUT_CONNECT_MILLIS)
					{
						System.out.println("\nThe server is not responding. Connection failed. Try again.");
						myState = ClientState.SS_NOT_CONNECTED;
						connectTrySendTime = -1; //reset
						Driver.subMenuInterrupted();
					}
				}
			}
			try {Thread.sleep(50,0);} catch (InterruptedException e) { break; }
		}
		synchronized(myState)
		{
			myState = ClientState.SS_GAME_COMPLETE;
		}
	}

	@Override
	public void onIncommingData(InetAddress clientAddress, int port, String data) {
		String[] tokens = data.split(" ");
		try
		{
			synchronized(myState)
			{
				switch (myState){
				case SS_LOBBY:
					switch (Integer.parseInt(tokens[0]))
					{
					case ProtocolInformation.CONNECT_ACCEPT:
						myState = ClientState.SS_LOBBY;
						System.out.println(new String("\nYou are now connected."));
						Driver.subMenuInterrupted();
						break;
					case ProtocolInformation.CONNECT_REJECT:
						myState = ClientState.SS_NOT_CONNECTED;
						System.out.println("\nCould not connect: "+data.substring(data.indexOf(" ")));
						Driver.subMenuInterrupted();
						break;
					case ProtocolInformation.READY_FOR_MATCH_RECEIVED:
						System.out.println("\nCould not connect: "+data.substring(data.indexOf(" ")));
						Driver.subMenuInterrupted();
						break;
					}
				}
			}
		} catch (Exception e) { /*BOGUS MESSAGE*/ }
	}
}
