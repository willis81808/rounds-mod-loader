using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoundsMDK
{
    public interface IMod
    {
        string Initialize();
        void OnGUI();
    }
}