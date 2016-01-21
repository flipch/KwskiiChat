using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ChatServer
{
    internal class Program
    {
        public static Hashtable clientsList = new Hashtable();

        public static int portNumber;

        public static string hostname = "Testing Server Name"; 

        private static void Main(string[] args)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Title = hostname;
            #region Port Setup
            var port = 8888;
            var check = false;
            string port_awnser = "";
            do
            {
                Console.WriteLine("Welcome to kwskii's Server Chat!\n " +
                              "\n" +
                              "Do you want the server to initialize at port 8888(Y\\N)");
                port_awnser = Console.ReadLine().ToUpper();
                check = validateAwnser(new[] {"Y", "N"}, port_awnser);
                if (check && port_awnser.StartsWith("Y"))
                {
                    Console.WriteLine("Ok starting server at " + port);
                }
                else if (!check)
                {   
                    Console.Clear();
                    Console.WriteLine("That's not a valid awnser!\nPlease enter Y or N: ");
                }
            } while (!check);

            char awnser = port_awnser[0];

            if (awnser == 'N')
            {
               Console.WriteLine("Choose your custom port for the server");
                bool isNumeric = false;
                do
                {
                    port_awnser = Console.ReadLine().ToUpper();
                    isNumeric = int.TryParse(port_awnser, out port);
                    if (isNumeric)
                        Console.WriteLine("Ok starting server at " + port);
                    else
                    {
                        Console.Clear();
                        Console.WriteLine("That's not a valid number!\nPlease enter your chosen port: ");
                    }
                } while (!isNumeric);
            }
#endregion
            portNumber = port;

            TcpListener serverSocket = new TcpListener(port);
            TcpClient clientSocket = default(TcpClient);
            int counter = 0;

            Console.Clear();
            for (int dots = 0; dots <= 5; ++dots)
            {   if (dots == 0)
                    serverSocket.Start();
                Console.Write("\r{0}Launching Server{0}", new string('*', dots));
                System.Threading.Thread.Sleep(100);
            }

            Console.WriteLine("\n\nServer is up and running!" +
                              "\nHostname : " + hostname +
                              "\nPort : " + portNumber +
                              "\nIP : " + GetPublicIp());


            while ((true))
            {
                counter += 1;
                clientSocket = serverSocket.AcceptTcpClient();
                byte[] bytesFrom = new byte[10025];
                string dataFromClient = null;

                NetworkStream networkStream = clientSocket.GetStream();
                networkStream.Read(bytesFrom, 0, (int)clientSocket.ReceiveBufferSize);
                dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom);
                dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));

                clientsList.Add(dataFromClient, clientSocket);

                Console.WriteLine(dataFromClient + " Joined chat room ");
                handleClinet client = new handleClinet();
                client.startClient(clientSocket, dataFromClient, clientsList);
            }

            clientSocket.Close();
            serverSocket.Stop();
            Console.WriteLine("exit");
            Console.ReadLine();
        }

        public static void broadcast(string msg, string uName, bool flag)
        {
            foreach (DictionaryEntry Item in clientsList)
            {
                TcpClient broadcastSocket;
                broadcastSocket = (TcpClient)Item.Value;
                NetworkStream broadcastStream = broadcastSocket.GetStream();
                Byte[] broadcastBytes = null;

                if (flag == true)
                {
                    broadcastBytes = Encoding.ASCII.GetBytes(uName + " says : " + msg);
                }
                else
                {
                    broadcastBytes = Encoding.ASCII.GetBytes(msg);
                }

                broadcastStream.Write(broadcastBytes, 0, broadcastBytes.Length);
                broadcastStream.Flush();
            }
        }

        public class handleClinet
        {
            TcpClient clientSocket;
            string clNo;
            Hashtable clientsList;

            public void startClient(TcpClient inClientSocket, string clineNo, Hashtable cList)
            {
                this.clientSocket = inClientSocket;
                this.clNo = clineNo;
                this.clientsList = cList;
                Thread ctThread = new Thread(doChat);
                ctThread.Start();
            }

            private void doChat()
            {
                int requestCount = 0;
                byte[] bytesFrom = new byte[10025];
                string dataFromClient = null;
                Byte[] sendBytes = null;
                string serverResponse = null;
                string rCount = null;
                requestCount = 0;

                while ((true))
                {
                    try
                    {
                        requestCount = requestCount + 1;
                        NetworkStream networkStream = clientSocket.GetStream();
                        networkStream.Read(bytesFrom, 0, (int)clientSocket.ReceiveBufferSize);
                        dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom);
                        dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));
                        Console.WriteLine("From client - " + clNo + " : " + dataFromClient);
                        rCount = Convert.ToString(requestCount);

                        Program.broadcast(dataFromClient, clNo, true);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }//end while
            }
        }

        public static string GetPublicIp()
        {
            String direction = "";
            WebRequest request = WebRequest.Create("http://checkip.dyndns.org/");
            using (WebResponse response = request.GetResponse())
            using (StreamReader stream = new StreamReader(response.GetResponseStream()))
            {
                direction = stream.ReadToEnd();
            }

            //Search for the ip in the html
            int first = direction.IndexOf("Address: ") + 9;
            int last = direction.LastIndexOf("</body>");
            direction = direction.Substring(first, last - first);

            return direction;
        }

        private static bool validateAwnser(string[] checkArrayStrings, string input)
        {
            foreach (var possibleCheck in checkArrayStrings)
            {
                if (input.StartsWith(possibleCheck))
                {
                    return true;
                }
            }
            return false;
        }
    }

 
}
