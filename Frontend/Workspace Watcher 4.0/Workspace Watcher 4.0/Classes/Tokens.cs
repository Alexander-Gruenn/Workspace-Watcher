using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workspace_Watcher_4._0.Classes
{
    public class Tokens
    {
        public string accessToken;
        public string refreshToken;

        public Tokens(string access, string refresh) 
        { 
            accessToken = access;
            refreshToken = refresh;
        }
    }
}
