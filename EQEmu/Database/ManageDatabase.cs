using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;
using System.Collections.ObjectModel;

namespace EQEmu.Database
{
    abstract public class ManageDatabase : DatabaseObject, IManageDatabase
    {
        private List<IDatabaseObject> _needsInserted = new List<IDatabaseObject>();

        [Browsable(false)]
        [XmlIgnore]
        public List<IDatabaseObject> NeedsInserted
        {
            get { return _needsInserted; }
        }

        private List<IDatabaseObject> _needsDeleted = new List<IDatabaseObject>();

        [Browsable(false)]
        [XmlIgnore]
        public List<IDatabaseObject> NeedsDeleted
        {
            get { return _needsDeleted; }
        }

        public ManageDatabase(QueryConfig config)
            : base(config)
        {

        }

        /// <summary>
        /// Use this method to get the proper query for the database currently being worked on
        /// /// </summary>
        /// <param name="needsUpdated"></param>
        /// <returns></returns>
        protected string GetQuery(IEnumerable<IDatabaseObject> needsUpdated=null)
        {
            string sql = Environment.NewLine + String.Format("-- {0}", this.ToString()) + Environment.NewLine;
            Type objType;

            if (_needsDeleted.Count > 0)
            {
                objType = _needsDeleted.ElementAt(0).GetType();

                sql += String.Format("-- DELETE {0}", objType.ToString()) + Environment.NewLine;
                foreach (IDatabaseObject obj in _needsDeleted)
                {
                    if (objType != obj.GetType())
                    {
                        objType = obj.GetType();
                        sql += String.Format("-- DELETE {0}", objType.ToString()) + Environment.NewLine;
                    }
                    sql += obj.DeleteString + Environment.NewLine;
                }
            }

            if (_needsInserted.Count > 0)
            {
                objType = _needsInserted.ElementAt(0).GetType();                
                sql += String.Format("-- INSERT NEW {0}",objType.ToString()) + Environment.NewLine;

                foreach (IDatabaseObject obj in _needsInserted)
                {
                    if (objType != obj.GetType())
                    {
                        objType = obj.GetType();
                        sql += String.Format("-- INSERT NEW {0}", objType.ToString()) + Environment.NewLine;
                    }

                    sql += obj.InsertString + Environment.NewLine;
                }
            }

            if (needsUpdated != null)
            {
                IList<IDatabaseObject> updates =
                    needsUpdated.Cast<IDatabaseObject>()
                    .Where( (x) =>                
                        {
                            var y = x as IManageDatabase;
                            var update = false;

                            if(x.Dirty == true) update = true;
                            if(y != null)
                            {
                                if(y.DirtyComponents.Count >0) update = true;
                                if(y.NeedsDeleted.Count>0) update = true;
                                if(y.NeedsInserted.Count>0) update = true;
                            }
                            return update && !_needsInserted.Contains(x) && !_needsDeleted.Contains(x);
                        } ).ToList();                        

                if (updates.Count > 0)
                {
                    sql += "-- UPDATING" + Environment.NewLine;
                    foreach (IDatabaseObject o in updates)
                    {
                        sql += o.UpdateString + Environment.NewLine;
                    }
                }
            }
            

            sql += Environment.NewLine;

            return sql;
        }

        protected virtual void AddObject(IDatabaseObject obj)
        {
            if (CreatedObj)
            {
                NeedsInserted.Add(obj);
            }
        }

        protected virtual void RemoveObject(IDatabaseObject obj)
        {
            if (NeedsInserted.Contains(obj)) NeedsInserted.Remove(obj);
            else
            {
                NeedsDeleted.Add(obj);
            }
        }

        protected virtual void ClearObjects()
        {
            NeedsDeleted.Clear();
            NeedsInserted.Clear();
        }

        [Browsable(false)]        
        [XmlIgnore]
        abstract public List<IDatabaseObject> DirtyComponents { get; }
    }
}
