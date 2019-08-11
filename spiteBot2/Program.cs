using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;

namespace spiteBot2
{
	class Program
	{
		private static Dictionary<char, Func<string, string, int>> actions =
			new Dictionary<char, Func<string, string, int>>();

		private static Dictionary<string, viewer> users = new Dictionary<string, viewer>();

		private static viewer owner = new viewer("drcobaltjedi", true, true);

		private const char free = '*';
		private const char userChar = '!';
		private const char admin = '&';

		static void Main(string[] args)
		{
			actions.Add(free, freeBes);
			actions.Add(userChar, normalCommands);
			actions.Add(admin, adminCommands);

			users.Add(owner.Name, owner);
			Console.WriteLine("Hello World!");
			
			StartListening();
		}

		#region objects and functions

		#region Users

		//User objects
		private class viewer
		{
			private bool isAdmin = false;
			private bool isApprovedUser = false;
			private string name;

			public viewer(string name)
			{
				this.name = name;
			}

			public viewer(string name, bool isApprovedUser, bool isAdmin)
			{
				this.name = name;
				this.isApprovedUser = isApprovedUser;
				this.isAdmin = isAdmin;
			}

			public bool IsAdmin
			{
				get => isAdmin;
				set => isAdmin = value;
			}

			public bool IsApprovedUser
			{
				get => isApprovedUser;
				set => isApprovedUser = value;
			}

			public string Name
			{
				get => name;
				set => name = value;
			}
		}

		#endregion

		#region client data

		// State object for reading client data asynchronously  
		public class StateObject
		{
			// Client  socket.  
			public Socket workSocket = null;

			// Size of receive buffer.  
			public const int BufferSize = 1024;

			// Receive buffer.  
			public byte[] buffer = new byte[BufferSize];

			// Received data string.  
			public StringBuilder sb = new StringBuilder();
		}

		#endregion

		private static int freeBes(string acct, string msg)
		{
			return 1;
		}

		#region normal Users

		private static int normalCommands(string acct, string msg)
		{
			if (users.ContainsKey(acct) && users[acct].IsApprovedUser)
			{
				doNormalCommand(msg);
			}

			return 1;
		}

		private static void doNormalCommand(string msg)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region admin users

		private static int adminCommands(string acct, string msg)
		{
			if (users.ContainsKey(acct) && users[acct].IsAdmin)
			{
				doAdminCommand(msg);
			}

			return 1;
		}

		private static void doAdminCommand(string msg)
		{
		}

		#endregion

		#endregion

		#region async

		public static ManualResetEvent allDone = new ManualResetEvent(false);

//    public AsynchronousSocketListener() {  
//    }  

		public static void StartListening()
		{
			// Establish the local endpoint for the socket.  
			// The DNS name of the computer  
			// running the listener is "host.contoso.com".  
			IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
			IPAddress ipAddress = ipHostInfo.AddressList[0];
			IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

			// Create a TCP/IP socket.  
			Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

			// Bind the socket to the local endpoint and listen for incoming connections.  
			try
			{
				listener.Bind(localEndPoint);
				listener.Listen(100);

				while (true)
				{
					// Set the event to nonsignaled state.  
					allDone.Reset();

					// Start an asynchronous socket to listen for connections.  
					Console.WriteLine("Waiting for a connection...");
					listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);

					// Wait until a connection is made before continuing.  
					allDone.WaitOne();
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
			}

			Console.WriteLine("\nPress ENTER to continue...");
			Console.Read();
		}

		public static void AcceptCallback(IAsyncResult ar)
		{
			// Signal the main thread to continue.  
			allDone.Set();

			// Get the socket that handles the client request.  
			Socket listener = (Socket) ar.AsyncState;
			Socket handler = listener.EndAccept(ar);

			// Create the state object.  
			StateObject state = new StateObject();
			state.workSocket = handler;
			handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
		}

		public static void ReadCallback(IAsyncResult ar)
		{
			String content = String.Empty;

			// Retrieve the state object and the handler socket  
			// from the asynchronous state object.  
			StateObject state = (StateObject) ar.AsyncState;
			Socket handler = state.workSocket;

			// Read data from the client socket.   
			int bytesRead = handler.EndReceive(ar);

			if (bytesRead > 0)
			{
				// There  might be more data, so store the data received so far.  
				state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

				// Check for end-of-file tag. If it is not there, read   
				// more data.  
				content = state.sb.ToString();
				if (content.IndexOf("<EOF>") > -1)
				{
					// All the data has been read from the   
					// client. Display it on the console.  
					Console.WriteLine("Read {0} bytes from socket. \n Data : {1}", content.Length, content);
					// Echo the data back to the client.  
					Send(handler, content);
				}
				else
				{
					// Not all data received. Get more.  
					handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback),
						state);
				}
			}
		}

		private static void Send(Socket handler, String data)
		{
			// Convert the string data to byte data using ASCII encoding.  
			byte[] byteData = Encoding.ASCII.GetBytes(data);

			// Begin sending the data to the remote device.  
			handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);
		}

		private static void SendCallback(IAsyncResult ar)
		{
			try
			{
				// Retrieve the socket from the state object.  
				Socket handler = (Socket) ar.AsyncState;

				// Complete sending the data to the remote device.  
				int bytesSent = handler.EndSend(ar);
				Console.WriteLine("Sent {0} bytes to client.", bytesSent);

				handler.Shutdown(SocketShutdown.Both);
				handler.Close();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
			}
		}

		#endregion
	}
}