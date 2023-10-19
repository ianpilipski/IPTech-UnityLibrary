#if !(UNITY_EDITOR || DEVELOPMENT_BUILD || QA_BUILD)
#define CONTAINERDEBUGSERVICE_DISABLED
#endif

#if !CONTAINERDEBUGSERVICE_DISABLED
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace IPTech.DebugConsoleService.TelnetConsole
{
	public delegate void ConnectionEventHandler(TelnetClient c);
	public delegate void ConnectionBlockedEventHandler(IPEndPoint endPoint);
	public delegate void MessageReceivedEventHandler(TelnetClient c, string message);

	public interface ITelnetServer : IDisposable {
		void start();
		void stop();
		bool isListening { get; }
		bool incomingConnectionsAllowed();
		void denyIncomingConnections();
		void allowIncomingConnections();
		void clearClientScreen(TelnetClient c);
		void sendMessageToClient(TelnetClient c, string message);
		void sendMessageToAll(string message);
		void kickClient(TelnetClient client);
		event ConnectionEventHandler ClientConnected;
		event ConnectionEventHandler ClientDisconnected;
		event ConnectionBlockedEventHandler ConnectionBlocked;
		event MessageReceivedEventHandler MessageReceived;
	}

	public interface ITelnetServerFactory {
		ITelnetServer Create(IPAddress ip, int port, int dataSize = 1024);
	}

	public class TelnetServer : ITelnetServer
	{
		public const string END_LINE = "\r\n";

		private Socket serverSocket;
		private IPAddress ip;
		private int port;
		private readonly int dataSize;
		private byte[] data;
		private bool acceptIncomingConnections;
		private Dictionary<Socket, TelnetClient> clients;

		public event ConnectionEventHandler ClientConnected;
		public event ConnectionEventHandler ClientDisconnected;
		public event ConnectionBlockedEventHandler ConnectionBlocked;
		public event MessageReceivedEventHandler MessageReceived;


		public TelnetServer(IPAddress ip, int port, int dataSize = 1024)
		{
			this.ip = ip;
			this.port = port;
			this.dataSize = dataSize;
			this.data = new byte[dataSize];
			this.clients = new Dictionary<Socket, TelnetClient>();
			this.acceptIncomingConnections = true;
			this.serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		}

		public void start()
		{
			if(isListening) return;
			this.serverSocket.Bind(new IPEndPoint(ip, port));
			serverSocket.Listen(0);
			serverSocket.BeginAccept(new AsyncCallback(handleIncomingConnection), serverSocket);
		}

		public void stop()
		{
			if(!isListening) return;
			serverSocket.Close();
		}

		public bool isListening { get { return serverSocket.IsBound; } }
		
		public bool incomingConnectionsAllowed()
		{
			return acceptIncomingConnections;
		}

		public void denyIncomingConnections()
		{
			this.acceptIncomingConnections = false;
		}

		public void allowIncomingConnections()
		{
			this.acceptIncomingConnections = true;
		}

		public void clearClientScreen(TelnetClient c)
		{
			sendMessageToClient(c, "\u001B[1J\u001B[H");
		}

		public void sendMessageToClient(TelnetClient c, string message)
		{
			Socket clientSocket = getSocketByClient(c);
			sendMessageToSocket(clientSocket, message);
		}

		private void sendMessageToSocket(Socket s, string message)
		{
			byte[] data = Encoding.ASCII.GetBytes(message);
			sendBytesToSocket(s, data);
		}

		private void sendBytesToSocket(Socket s, byte[] data)
		{
			s.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(sendData), s);
		}

		public void sendMessageToAll(string message)
		{
			foreach (Socket s in clients.Keys)
			{
				try { sendMessageToSocket(s, message); }
				catch { clients.Remove(s); }
			}
		}

		private TelnetClient getClientBySocket(Socket clientSocket)
		{
			TelnetClient c;
			try {
				clients.TryGetValue(clientSocket, out c);
			}
			catch {
				c = null;
			}
			return c;
		}

		private Socket getSocketByClient(TelnetClient client)
		{
			Socket s;
			s = clients.FirstOrDefault(x => x.Value.getClientID() == client.getClientID()).Key;
			return s;
		}

		public void kickClient(TelnetClient client)
		{
			closeSocket(getSocketByClient(client));
			ClientDisconnected(client);
		}

		private void closeSocket(Socket clientSocket)
		{
			clientSocket.Close();
			clients.Remove(clientSocket);
		}

		private void handleIncomingConnection(IAsyncResult result)
		{
			try
			{
				Socket oldSocket = (Socket)result.AsyncState;

				if (acceptIncomingConnections)
				{
					Socket newSocket = oldSocket.EndAccept(result);

					uint clientID = (uint)clients.Count + 1;
					TelnetClient client = new TelnetClient(clientID, (IPEndPoint)newSocket.RemoteEndPoint);
					clients.Add(newSocket, client);

					// Do Echo
					// Do Remote Flow Control
					// Will Echo
					// Will Suppress Go Ahead
					sendBytesToSocket(
						newSocket,
						new byte[] { 0xff, 0xfd, 0x01, 0xff, 0xfd, 0x21, 0xff, 0xfb, 0x01, 0xff, 0xfb, 0x03 }
					);

					client.resetReceivedData();

					ClientConnected(client);

					serverSocket.BeginAccept(new AsyncCallback(handleIncomingConnection), serverSocket);
				}

				else
				{
					ConnectionBlocked((IPEndPoint)oldSocket.RemoteEndPoint);
				}
			}

			catch { }
		}

		private void sendData(IAsyncResult result)
		{
			try {
				Socket clientSocket = (Socket)result.AsyncState;
				clientSocket.EndSend(result);
				clientSocket.BeginReceive(data, 0, dataSize, SocketFlags.None, new AsyncCallback(receiveData), clientSocket);
			}
			catch { }
		}

		private void receiveData(IAsyncResult result)
		{
			try
			{
				Socket clientSocket = (Socket)result.AsyncState;
				TelnetClient client = getClientBySocket(clientSocket);

				int bytesReceived = clientSocket.EndReceive(result);

				if(bytesReceived>0) {
					Console.WriteLine("bytesRecieved = " + bytesReceived + " : " + BitConverter.ToString(data));
				}

				if (bytesReceived == 0)
				{
					closeSocket(clientSocket);
					serverSocket.BeginAccept(new AsyncCallback(handleIncomingConnection), serverSocket);
				}
				else if (data[0] < 0xF0)
				{
					string receivedData = client.getReceivedData();

					// 0x2E = '.', 0x0D = carriage return, 0x0A = new line
					if ((data[0] == 0x2E && data[1] == 0x0D && receivedData.Length == 0) ||
						(data[0] == 0x0D && (data[1] == 0x0A || data[1] == 0x00)))
					{
						//sendMessageToSocket(clientSocket, "\u001B[1J\u001B[H");
						MessageReceived(client, client.getReceivedData());
						client.pushReceivedDataToHistory();
						client.resetReceivedDataHistoryIterator();
						client.resetReceivedData();
					}
					else
					{
						// 0x08 => backspace character
						if (data[0] == 0x08 || data[0] == 0x7F)
						{
							if (receivedData.Length > 0)
							{
								client.removeLastCharacterReceived();
								sendBytesToSocket(clientSocket, new byte[] { 0x08, 0x20, 0x08 });
							}

							else
								clientSocket.BeginReceive(data, 0, dataSize, SocketFlags.None, new AsyncCallback(receiveData), clientSocket);
						}
						// 0x7F => delete character (backspace on mac client)
						else if (data[0] == 0x7F) {
							clientSocket.BeginReceive(data, 0, dataSize, SocketFlags.None, new AsyncCallback(receiveData), clientSocket);
						}
						// 0x1B-0x5B-(0x41|0x42) Arrow up / down
						else if(data[0] == 0x1B && data[1] == 0x5B && bytesReceived==3 && (data[2] == 0x41 || data[2] == 0x42)) {
							string m = null;
							if(data[2] == 0x41) {
								m = client.getNextReceivedDataHistory();
							} else {
								m = client.getPreviousReceiveDataHistory();
							}
							if(m!=null) {
								client.setReceivedData(m);
								m = "\u001b[2K\u001b[1G > " + m;
								//m = "\u00FF\u00F8" + m; 
								sendMessageToSocket(clientSocket, m);
							}
						}
						else
						{
							client.appendReceivedData(Encoding.ASCII.GetString(data, 0, bytesReceived));

							// Echo back the received character
							// if client is not writing any password
							if (client.getCurrentStatus() != EClientStatus.Authenticating)
								sendBytesToSocket(clientSocket, new byte[] { data[0] });

							// Echo back asterisks if client is
							// writing a password
							else
								sendMessageToSocket(clientSocket, "*");

							clientSocket.BeginReceive(data, 0, dataSize, SocketFlags.None, new AsyncCallback(receiveData), clientSocket);
						}
					}
				}

				else
					clientSocket.BeginReceive(data, 0, dataSize, SocketFlags.None, new AsyncCallback(receiveData), clientSocket);
			}

			catch { }
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(true);
		}

		protected void Dispose(bool isDisposing) {
			if(isDisposing) {
				if(this.serverSocket != null) {
					this.serverSocket.Close();
					this.serverSocket = null;
				}
			}
		}

		public class Factory : ITelnetServerFactory {
			public ITelnetServer Create(IPAddress ip, int port, int dataSize = 1024) {
				return new TelnetServer(ip, port, dataSize);
			}
		}
	}
}
#endif