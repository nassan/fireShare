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
    using RestSharp;

    class Program
    {
        static void Main(string[] args)
        {
            Host.Interaction test = new Host.Interaction("testuser1");
            //test.enter();
            test.retriveLocalPeers();

        }
    }


    class Host
    {
       
        public class Interaction
        {
            private const string basename = "hosts/";
            private string user_url;
            private bool is_user_set = false;
            IFirebaseConfig config;
            IFirebaseClient client;
            FirebaseResponse response;
            Details details;

            Dictionary<string,string> local_peers_details;

            public Interaction(string username)
            {
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

            public void retriveLocalPeers()
            {
                response = client.Get(basename);
                local_peers_details = response.ResultAs<Dictionary<string, string>>();

                foreach (string key in local_peers_details.Keys)
                {
                    Console.WriteLine(key);
                    Console.WriteLine("\t" + local_peers_details[key].GetType());
                }
            }
        
        }

        class Details
        {
             //HostInfo Members
        public string local_IP { get; set; }
        public string computer_name { get; set; }

        //HostInfo Constructors
        public Details()
        {
            computer_name = Dns.GetHostName();

            //Process to fetch the host's local IP address
            IPHostEntry temp = Dns.GetHostEntry(computer_name);
            foreach (IPAddress addr in temp.AddressList)
            {
                if (addr.AddressFamily == AddressFamily.InterNetwork)
                {
                    local_IP = addr.ToString();
                    break;
                }
            }
        }

        }

    }
}
