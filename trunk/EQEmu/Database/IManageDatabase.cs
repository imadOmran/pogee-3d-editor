using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EQEmu.Database
{
    interface IManageDatabase
    {
        List<IDatabaseObject> NeedsInserted { get; }
        List<IDatabaseObject> NeedsDeleted { get; }

        List<IDatabaseObject> DirtyComponents { get; }
    }
}
