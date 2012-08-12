import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.InetAddress;
import java.util.ArrayList;


public class UDPSocket {
	class Tripple <T1,T2,T3>{
		T1 data1;
		T2 data2;
		T3 data3;
		public Tripple(T1 d1,T2 d2, T3 d3){ data1 = d1; data2 = d2; data3 = d3;}
	}
	static final int MAX_PACKET_SIZE = 1024; 
	SocketListener ear;
	DatagramSocket socket;
	byte[] receiveData = new byte[MAX_PACKET_SIZE];
    byte[] sendData = new byte[MAX_PACKET_SIZE];
    ArrayList<Tripple<InetAddress,Integer,String>> sendBuffer = new ArrayList<Tripple<InetAddress,Integer,String>>();
    /**
     * Constructor for the UDPSocket
     * @param ear A socket event listener
     * @param port Port number to start socket on
     * @throws Exception if socket cannot be started
     */
	public UDPSocket(SocketListener ear, int port) throws Exception
	{
		this.ear = ear;
		socket = new DatagramSocket(port);	
	}
	/**
	 * Sends the string of data to a destination 
	 * @param ip ipv4 address of the destination
	 * @param port integer indicating the port number of the destination
	 * @param data data to send as a string
	 * @throws Exception if the data exceeds 1024 bytes
	 */
	public void sendData(InetAddress ip, Integer port, String data) throws Exception
	{
		synchronized(sendBuffer){
			if (sendData.length > MAX_PACKET_SIZE)
				throw new Exception("Data packet too large to send");
			sendBuffer.add(new Tripple<InetAddress, Integer, String>(ip, port, data));
		}
	}
	public void run()
	{
		while(!Thread.currentThread().isInterrupted())
		{
			try
			{
				DatagramPacket receivePacket = new DatagramPacket(receiveData, receiveData.length);
				socket.receive(receivePacket);
				ear.onIncommingData(receivePacket.getAddress(), receivePacket.getPort(), new String(receivePacket.getData()));
				synchronized(sendBuffer){
					for ( ; sendBuffer.size() > 0; ) 
					{
						//receive data
						Tripple<InetAddress, Integer, String> packet = sendBuffer.get(0);
						sendData = packet.data3.getBytes();
						//send data
						DatagramPacket sendPacket = new DatagramPacket(sendData,sendData.length,packet.data1,packet.data2);
						socket.send(sendPacket);
						sendBuffer.remove(0);
					}
				}
			}
			catch (Exception e) { e.printStackTrace(); }
			try {Thread.sleep(50,0);} catch (InterruptedException e) { break; }
		}
		socket.close();
	}
}
