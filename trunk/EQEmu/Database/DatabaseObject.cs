using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Xml.Serialization;
using System.IO;

using MySql.Data.MySqlClient;

namespace EQEmu.Database
{
    /// <summary>
    /// The base object for objects that are retrieved and modified to a database
    /// 
    /// This object provides implementation for keeping track of what query statements
    /// are needed to keep it in sync with the database
    /// 
    /// After object creation the Created method should be invoked to data changes
    /// are now being tracked, indicated in the 'Dirty' property
    /// 
    /// The 'Dirtied' method should be invoked whenever the object is modified
    ///     i.e in the property setter
    /// 
    /// </summary>    
    abstract public class DatabaseObject : IDatabaseObject
    {
        //abstract public string InsertString { get; }
        //abstract public string UpdateString { get; }
        //abstract public string DeleteString { get; }        

        protected TypeQueries _queries;
        [Browsable(false)]
        public TypeQueries Queries
        {
            get { return _queries; }
        }

        protected QueryConfig _queryConfig;

        private string _selectString;
        private string _insertString;
        private string _updateString;
        private string _deleteString;
                
        [Browsable(false)]      
        public virtual string SelectString 
        {
            //get { return _selectString; }
            get 
            {
                if (SelectArgValues == null) return _selectString;
                return String.Format(_selectString, SelectArgValues); 
            }
        }

        [Browsable(false)]
        public virtual string InsertString
        {
            //get { return _insertString; }
            get {
                if (InsertArgValues == null) return _insertString;
                return String.Format(_insertString, InsertArgValues); 
            }
        }

        [Browsable(false)]
        public virtual string UpdateString
        {
            //get { return _updateString; }
            get 
            {
                if (UpdateArgValues == null) return _updateString;
                return String.Format(_updateString, UpdateArgValues); 
            }
        }

        [Browsable(false)]
        public virtual string DeleteString
        {
            //get { return _deleteString; }
            get 
            {
                if (DeleteArgValues == null) return _deleteString;    
                return String.Format(_deleteString, DeleteArgValues); 
            }
        }

        [Browsable(false)]
        public object[] SelectArgValues
        {
            get
            {
                if (_queries != null && _queries.SelectQuery != null)
                {
                    return ResolveArgs(_queries.SelectArgs);
                }
                else return null;
            }
        }

        [Browsable(false)]
        public object[] InsertArgValues
        {
            get
            {
                if (_queries != null && _queries.InsertQuery != null)
                {
                    return ResolveArgs(_queries.InsertArgs);
                }
                else return null;
            }
        }

        [Browsable(false)]
        public object[] UpdateArgValues
        {
            get
            {
                if (_queries != null && _queries.UpdateQuery != null)
                {
                    return ResolveArgs(_queries.UpdateArgs);
                }
                else return null;
            }
        }

        [Browsable(false)]
        public object[] DeleteArgValues
        {
            get
            {
                if (_queries != null && _queries.DeleteQuery != null)
                {
                    return ResolveArgs(_queries.DeleteArgs);
                }
                else return null;
            }
        }
        
        public object[] ResolveArgs(string[] propertyNames)
        {
            if (propertyNames == null) return null;

            object[] values = new object[propertyNames.Count()];
            for (int i=0; i < propertyNames.Count(); i++)
            {
                //var pinfo = GetType().GetProperty(_queries.SelectParameters[i].ToString());
                var pinfo = GetType().GetProperty(propertyNames[i]);
                if (pinfo == null) throw new Exception( String.Format("invalid property {0} specified!",propertyNames[i]) );

                var val = pinfo.GetValue(this, null);
                if (val != null)
                {
                    if (val is Enum)
                    {
                        values[i] = (int)val;
                    }
                    else if (val is bool)
                    {
                        values[i] = Convert.ToInt32((bool)val);
                    }
                    else
                    {
                        values[i] = val;
                    }
                    //if (val is Enum)
                    //{
                    //    values[i] = ((int)val).ToString();
                    //}
                    //else if (val is bool)
                    //{
                    //    values[i] = Convert.ToInt32((bool)val).ToString();
                    //}
                    //else
                    //{
                    //    values[i] = val.ToString();
                    //}
                }
                else
                {
                    values[i] = "";
                }
            }
            return values;
        }

        public void SetPropertiesFaster(TypeQueries queryInfo, Dictionary<string, object> dict)
        {
            var ptype = GetType();
            foreach (var i in queryInfo.SelectQueryFields)
            {
                var pinfo = ptype.GetProperty(i.Property);
                if (pinfo != null)
                {
                    switch (i.Type)
                    {
                        case SelectQueryFieldStore.DataTypes.Int:
                            pinfo.SetValue(this, Convert.ToInt32(dict[i.Column]), null);
                            break;
                        case SelectQueryFieldStore.DataTypes.Bool:
                            pinfo.SetValue(this, Convert.ToBoolean(dict[i.Column]), null);
                            break;
                        case SelectQueryFieldStore.DataTypes.Float:
                            pinfo.SetValue(this, (float)Convert.ToDouble(dict[i.Column]), null);
                            break;
                        case SelectQueryFieldStore.DataTypes.Long:
                            pinfo.SetValue(this, Convert.ToInt64(dict[i.Column]), null);
                            break;
                        case SelectQueryFieldStore.DataTypes.Short:
                            pinfo.SetValue(this, Convert.ToInt16(dict[i.Column]), null);
                            break;
                        case SelectQueryFieldStore.DataTypes.String:
                        default:
                            pinfo.SetValue(this, Convert.ToString(dict[i.Column]), null);
                            break;
                    }
                }
            }
        }

        public void SetProperties(TypeQueries queryInfo, Dictionary<string, object> dict)
        {
            var ptype = GetType();

            foreach (var i in queryInfo.SelectQueryFields)
            {
                try
                {

                    var pinfo = ptype.GetProperty(i.Property);
                    if (pinfo != null)
                    {
                        switch (i.Type)
                        {
                            case SelectQueryFieldStore.DataTypes.Int:
                                pinfo.SetValue(this, Convert.ToInt32(dict[i.Column]), null);
                                break;
                            case SelectQueryFieldStore.DataTypes.Bool:
                                pinfo.SetValue(this, Convert.ToBoolean(dict[i.Column]), null);
                                break;
                            case SelectQueryFieldStore.DataTypes.Float:
                                pinfo.SetValue(this, (float)Convert.ToDouble(dict[i.Column]), null);
                                break;
                            case SelectQueryFieldStore.DataTypes.Long:
                                pinfo.SetValue(this, Convert.ToInt64(dict[i.Column]), null);
                                break;
                            case SelectQueryFieldStore.DataTypes.Short:
                                pinfo.SetValue(this, Convert.ToInt16(dict[i.Column]), null);
                                break;
                            case SelectQueryFieldStore.DataTypes.String:
                            default:
                                pinfo.SetValue(this, Convert.ToString(dict[i.Column]), null);
                                break;
                        }
                    }
                }
                catch (Exception) 
                { 
                    //couldn't set that property.....  
                }
            }
        }
        
        public void SetProperty(object obj, SelectQueryFieldStore item,MySqlDataReader rdr)
        {
            var pinfo = obj.GetType().GetProperty(item.Property);
            if (pinfo != null)
            {
                switch (item.Type)
                {
                    case SelectQueryFieldStore.DataTypes.Int:
                        pinfo.SetValue(obj, rdr.GetInt32(item.Column), null);
                        break;
                    case SelectQueryFieldStore.DataTypes.Bool:
                        pinfo.SetValue(obj, Convert.ToBoolean(rdr.GetInt32(item.Column)), null);
                        break;
                    case SelectQueryFieldStore.DataTypes.Float:
                        pinfo.SetValue(obj, rdr.GetFloat(item.Column), null);
                        break;
                    case SelectQueryFieldStore.DataTypes.String:
                        pinfo.SetValue(obj, rdr.GetString(item.Column), null);
                        break;
                    case SelectQueryFieldStore.DataTypes.Long:
                        pinfo.SetValue(obj, rdr.GetInt64(item.Column), null);
                        break;
                    case SelectQueryFieldStore.DataTypes.Short:
                        pinfo.SetValue(obj, rdr.GetInt16(item.Column), null);
                        break;
                    default:
                        pinfo.SetValue(obj, rdr.GetString(item.Column), null);
                        break;
                }
            }
        }

        public DatabaseObject(QueryConfig config)
        {
            InitConfig(config);
        }

        public void InitConfig(QueryConfig config)
        {
            _queryConfig = config;

            if (config == null) return;

            var type = config.TypeQueryMappings.Keys.FirstOrDefault(x => this.GetType().IsSubclassOf(x));
            if (type == null) type = this.GetType();

            if (config != null && type != null && config.TypeQueryMappings.ContainsKey(type))
            {
                _queries = config.TypeQueryMappings[type];

                _selectString = _queries.SelectQuery;
                ThrowIfDeleteDetected(_selectString);

                _deleteString = _queries.DeleteQuery;

                _updateString = _queries.UpdateQuery;
                ThrowIfDeleteDetected(_updateString);

                _insertString = _queries.InsertQuery;
                ThrowIfDeleteDetected(_insertString);
            } 
        }

        private void ThrowIfDeleteDetected(string str)
        {
            if (str == null) return;
            if (str.ToLower().Contains(" delete "))
            {
                throw new Exception("Delete statement detected in string!");
            }
        }

        [XmlIgnore]
        public bool Dirty
        {
            get;
            protected set;
        }

        private bool _created = false;
        protected bool CreatedObj
        {
            get { return _created; }
        }

        /// <summary>
        /// This method should be called immediately after setting all data properties
        /// Modifications to the object after this method is called will flag it as being dirty
        ///     i.e. after populating the structure from a database query
        /// </summary>
        public void Created()
        {
            if (_created == false)
            {
                _created = true;
            }
            else return;
        }

        /// <summary>
        /// Undoes a Created method call, if this method is called be sure to call the Created method again
        /// so the object can lock its state and determine when it is dirtied.
        /// </summary>
        public void UnlockObject()
        {
            _created = false;
        }

        protected void Dirtied()
        {
            if (_created == true)
            {
                Dirty = true;
                OnObjectDirtied();
            }
        }

        public event ObjectDirtiedHandler ObjectDirtied;

        protected void OnObjectDirtied()
        {
            var e = ObjectDirtied;
            if (e != null)
            {
                ObjectDirtied(this, new EventArgs());
            }
        }

        public virtual void SaveXML(string file)
        {
            using (FileStream fs = new FileStream(file, FileMode.OpenOrCreate))
            {
                XmlSerializer x = new XmlSerializer(GetType());
                x.Serialize(fs, this);
            }
        }

        public virtual void LoadXML(string file)
        {
            using (FileStream fs = new FileStream(file, FileMode.Open))
            {
                XmlSerializer x = new XmlSerializer(GetType());
                x.Serialize(fs, this);
            }
        }
    }
}
