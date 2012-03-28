using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EQEmu.Spawns
{
    public class NPCPropertyTemplateManager
    {
       private IEnumerable<INPCPropertyTemplate> _templates = new List<INPCPropertyTemplate>();

       public IEnumerable<INPCPropertyTemplate> Templates
        {
            get { return _templates; }
            set
            {
                _templates = value;
            }
        }
    }
}
