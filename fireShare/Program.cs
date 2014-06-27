using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Collections;

namespace fireShare
{
    using FireSharp;
    using FireSharp.Config;
    using FireSharp.Interfaces;
    using FireSharp.Response;
    using System.Net.Sockets;

    class Program
    {
        static void Main(string[] args)
        {
            //Host.Interaction test = new Host.Interaction("testuser1");
            //test.enter();
            //test.retrivePeers();

            Swarm swarm = new Swarm();
            swarm.enterSwarm();
            

        }
    }

    class Host
    {
       
        public class Interaction
        {
            private const string basename = "hosts/";
            private string username;
            private string user_url;
            private bool is_user_set = false;
            IFirebaseConfig config;
            IFirebaseClient client;
            FirebaseResponse response;
            Details details;
            Dictionary<string,Dictionary<string,string>> local_peers_details;

            public Interaction(string username)
            {
                //Set the username member
                this.username = username;

                //Instantiate the default config
                config = new FirebaseConfig
                {
                    AuthSecret = "h2X1OAqY2EOs0vIsyhomap4PKS4vMjnQyv57tEJd",
                    BasePath = "https://firesharing.firebaseio.com/"
                };

                //Instantiate the Firebase client using the above config
                client = new FirebaseClient(config);
                
                //Define the base url for the user
                user_url = basename + username;

                //Prepare details for delivery
                details = new Details();
            }

            public void enter()
            {
                //If the user has not been registed, register him
                if (!is_user_set)
                {
                    response = client.Set(user_url, "");
                }

                //Deliver the user details to the Firebase
                response = client.Update(user_url, details);

                //Print the response of the update
                Console.WriteLine(response.Body);
            }

            public void exit()
            {
                DeleteResponse delete_response = client.Delete(user_url);
                Console.WriteLine(delete_response.Success);
            }

            public void retrivePeers()
            {
                response = client.Get(basename);
                
                local_peers_details = response.ResultAs<Dictionary<string, Dictionary<string, string>>>();
                
                local_peers_details.Remove(username);

                //foreach (string outerkey in local_peers_details.Keys)
                //{
                //    Console.WriteLine(outerkey);
                
                //    foreach (string inner_key in local_peers_details[outerkey].Keys)
                //    {
                //        Console.WriteLine("\t" + inner_key);
                //        Console.WriteLine("\t\t" + local_peers_details[outerkey][inner_key]);
                //    }
                //}
            }


        
        }

        [Serializable]
        public class Details
        {
             //HostInfo Members
        public string local_IP { get; set; }
        public string computer_name { get; set; }

        //HostInfo Constructors
        public Details()
        {
            computer_name = Dns.GetHostName();

            //Process to fetch the host's local IP address
            IPAddress []  temp = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (IPAddress addr in temp)
            {
                if (addr.AddressFamily == AddressFamily.InterNetwork)
                {
                    local_IP = addr.ToString();
                    break;
                }
            }
        }
        //public void GetObjectData(SerializationInfo info, StreamingContext context)
        //{

        //}
        

        }

    }

    class Swarm
    {
        private const int destination_port = 11000;
        private const int source_port = 11001;
        IPAddress local_IP;
        IPEndPoint local_EP;
        IPAddress destination_IP;
        IPEndPoint destination_EP;
        ArrayList swarm;

        public Swarm()
        {
            //Initialize the swarm array for keeping track of hosts connected
            swarm = new ArrayList();

            //Find my local IP address
            IPAddress[] addresses = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (IPAddress addr in addresses)
            {
                if (addr.AddressFamily == AddressFamily.InterNetwork)
                {
                    local_IP = addr;
                    break;
                }
            }

            //Set Local Endpoint
            local_EP = new IPEndPoint(local_IP, source_port);

            //Set broadcast IP address to broadcast presence to
            destination_IP = IPAddress.Parse("192.168.127.255");
            //broadcast_IP = IPAddress.Broadcast;
            
            //Set broadcast Endpoint to broadcast presence to
            destination_EP = new IPEndPoint(destination_IP, destination_port);

        }


        //Sends broadcast from port 11001 that you are entering the swarm
        public void enterSwarm()
        {

            //Create UDP socket
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,
            ProtocolType.Udp);

            //Bind UDP socket to local source IP address and port 
            s.Bind(local_EP);

            //Allow Broadcast IP addresses
            s.EnableBroadcast = true;
            
            //Set the payload to be sent over the UDP socket
            //  This payload is me (the source) IP address
            byte[] sendbuf = Encoding.ASCII.GetBytes(local_IP.ToString());
            
            //Perform the broadcast
            s.SendTo(sendbuf, destination_EP);

            Console.WriteLine("Message sent to the broadcast address");
        }


        //Monitors the swarm on port 11000 for other hosts joining and leaving the swarm
        private static void monitorSwarm()
        {
            bool done = false;

            UdpClient listener = new UdpClient(destination_port);
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, destination_port);

            try
            {
                while (!done)
                {
                    Console.WriteLine("Waiting for broadcast");
                    byte[] bytes = listener.Receive(ref groupEP);

                    Console.WriteLine("Received broadcast from {0} :\n {1}\n",
                        groupEP.ToString(),
                        Encoding.ASCII.GetString(bytes, 0, bytes.Length));
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                listener.Close();
            }
        }
    }


}
