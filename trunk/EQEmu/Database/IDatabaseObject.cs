using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EQEmu.Database
{
    public enum QueryType
    {
        INSERT,
        SELECT,
        DELETE,
        UPDATE
    }

    public delegate void ObjectDirtiedHandler(object sender,EventArgs args);

    public interface IDatabaseObject
    {
        string InsertString { get; }
        string UpdateString { get; }
        string DeleteString { get; }
        bool Dirty { get; }
        event ObjectDirtiedHandler ObjectDirtied;
    }
}
