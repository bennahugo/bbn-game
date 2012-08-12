/**
 * ADT to store client information
 * @author Benjamin
 */
public class ClientInfo {
	byte[] ipv4Address;
	int port;
	String playerId;
	int latency;
	long tickTimeOfLastReceival;
	boolean ready;
	/**
	 * ClientInfo constructor
	 * @param ipv4Address player's IP v4 address
	 * @param playerId nickname of the player
	 * @throws Exception when arguements are null or invalid
	 */
	public ClientInfo(byte[] ipv4Address, int port, String playerId) throws Exception
	{
		if (ipv4Address == null | ipv4Address.length != 4 | playerId == null | playerId.trim().equals(""))
			throw new Exception("Enter valid client details");
		this.playerId = playerId;
		this.ipv4Address = ipv4Address;
		latency = 0;
	}
	@Override
	public boolean equals(Object o)
	{
		return ((ClientInfo)o).playerId.equals(playerId); 
	}
	@Override
	public String toString()
	{
		return "Nickname: " + playerId + "\t IP: " + ipv4Address[0] + "." + 
				ipv4Address[1] + "." + ipv4Address[2] + "." + ipv4Address[3] + "\t port: " + port + 
				"\t Latency (milliseconds): " + latency + "\t Time since last latency report (milliseconds): " + 
				(System.currentTimeMillis() - tickTimeOfLastReceival) + "\t ready: "+ready; 
	}
}
