import java.util.ArrayList;

/**
 * Class contains a list of connected clients and provides Round Robin access of this data
 * @author Benjamin
 */
public class ConnectedClients {
	public final static int MAX_LATENCY_IN_SECONDS = 1000; //one second
	public final static int TIME_INTERAL_FOR_CLIENTS_TO_SEND_LATENCY = 5000;
	public final static int MAX_CONNECTIONS = 8;
	
	ArrayList<ClientInfo> listOfConnectedClients;
	int currentRoundRobinClient = 0;
	/**
	 * Default constructor
	 */
	public ConnectedClients()
	{
		listOfConnectedClients = new ArrayList<ClientInfo>();
	}
	public boolean establishNewConnection(byte[] ipV4Address, int port, String playerNickname) throws Exception
	{
		if (listOfConnectedClients.contains(playerNickname))
			return false;
		if (listOfConnectedClients.size() > MAX_CONNECTIONS)
			return false;
		listOfConnectedClients.add(new ClientInfo(ipV4Address, port, playerNickname));
		return true;
	}
	public void lostConnection(int index)
	{
		listOfConnectedClients.remove(index);
		if (currentRoundRobinClient == index)
			moveToNextClient();
	}
	public ClientInfo getCurrentClient()
	{
		return listOfConnectedClients.get(currentRoundRobinClient);
	}
	public int getCurrentClientIndex()
	{
		return currentRoundRobinClient;
	}
	public boolean isThereConnectedClients()
	{
		return listOfConnectedClients.size() > 0;
	}
	public void playerIndicatedReadyness(String playerNickname){
		listOfConnectedClients.get(listOfConnectedClients.indexOf(playerNickname)).ready = true;
	}
	public ClientInfo getClientForNextRound()
	{
		moveToNextClient();
		return listOfConnectedClients.get(currentRoundRobinClient);
	}
	public int getNumberOfConnectedClients()
	{
		return listOfConnectedClients.size();
	}
	public ClientInfo getClientAtIndex(int index)
	{
		return listOfConnectedClients.get(index);
	}
	public void setLatencyForClient(String playerNickname, int latency)
	{
		if (listOfConnectedClients.contains(playerNickname))
		{
			ClientInfo ci = listOfConnectedClients.get(listOfConnectedClients.indexOf(playerNickname));
			ci.latency = latency;
			ci.tickTimeOfLastReceival = System.currentTimeMillis();
		}
	}
	public ArrayList<ClientInfo> removeAllPlayersWithHighLatency()
	{
		ArrayList<ClientInfo> results = new ArrayList<ClientInfo>();
		for (ClientInfo ci : listOfConnectedClients)
			if (ci.latency > MAX_LATENCY_IN_SECONDS || 
					System.currentTimeMillis() - ci.tickTimeOfLastReceival > MAX_LATENCY_IN_SECONDS + TIME_INTERAL_FOR_CLIENTS_TO_SEND_LATENCY)
			{
				results.add(ci);
				lostConnection(listOfConnectedClients.indexOf(ci));
			}
		return results;
	}
	private void moveToNextClient()
	{
		currentRoundRobinClient = currentRoundRobinClient >= listOfConnectedClients.size() ? 0 : currentRoundRobinClient + 1;
	}
}
