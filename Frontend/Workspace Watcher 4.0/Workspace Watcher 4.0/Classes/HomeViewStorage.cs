using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workspace_Watcher_4._0.Classes
{
    public class HomeViewStorage
    {
        static HomeViewStorage homeview;

        private HomeViewStorage()
        {
                // TODO: put logic for facedetection here!!!
        }

        public HomeViewStorage HomeViewFactory()
        {
            if(homeview == null)
            {
                homeview = new HomeViewStorage();
            }

            return homeview;
        }
    }
}
