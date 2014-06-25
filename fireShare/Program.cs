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
            //byte[] multicast_byte_address = {224,0,0,1};
            //IPAddress multicast_group = new IPAddress(multicast_byte_address);
            //MulticastOption multicast = new MulticastOption(multicast_group);
            //StartListener();
            Swarm.enterSwarm();
            

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
        public static void enterSwarm()
        {
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,
            ProtocolType.Udp);
            s.EnableBroadcast = true;
            
            IPAddress broadcast = IPAddress.Parse("192.168.127.255");
            IPEndPoint ep = new IPEndPoint(broadcast, 11000);
            //s.Connect(ep);
            //NetworkStream stream = new NetworkStream(s);
            //formatter.Serialize(stream, details);

            string local_IP = "";
            IPAddress[] temp = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (IPAddress addr in temp)
            {
                if (addr.AddressFamily == AddressFamily.InterNetwork)
                {
                    local_IP = addr.ToString();
                    break;
                }
            }
            byte[] sendbuf = Encoding.ASCII.GetBytes(local_IP);
            //stream.Close();

            s.SendTo(sendbuf, ep);
            

            Console.WriteLine("Message sent to the broadcast address");
        }
    }


}
