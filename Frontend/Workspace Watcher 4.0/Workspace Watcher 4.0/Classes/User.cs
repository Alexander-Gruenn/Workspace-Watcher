using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Workspace_Watcher_4._0.Classes
{
    public class User
    {
        public string email;
        public string password;
        public string firstName;
        public string surname;
        public string role;
        public string accessToken;
        public string refreshToken;

        [JsonConstructor]
        public User(string email, string password, string firstname, string surname, string role)
        {
            this.firstName = firstname;
            this.surname = surname;
            this.email = email;
            this.password = password;
            this.role = role;
        }

        public User(string firstname, string lastname, string email, string password)
        {
            this.firstName = firstname;
            this.surname = lastname;
            this.email = email;
            this.password = password;
        }

        public User(string email, string password) 
        { 
            this.email=email;
            this.password = password;
        }

        public User() { }
    }
}
