using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EQEmu.Spawns
{
    public class NpcPropertyTemplateManager
    {
       private IEnumerable<INpcPropertyTemplate> _templates = new List<INpcPropertyTemplate>();

       public IEnumerable<INpcPropertyTemplate> Templates
        {
            get { return _templates; }
            set
            {
                _templates = value;
            }
        }
    }
}
