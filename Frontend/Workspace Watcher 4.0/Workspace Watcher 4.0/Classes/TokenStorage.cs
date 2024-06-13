using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Workspace_Watcher_4._0.Classes
{
    public static class TokenStorage
    {
        public static Tokens Tokens { get; private set; } = null;

        public static void SetTokens(Tokens tokens)
        {
            Tokens = tokens;   
        }
    }
}
