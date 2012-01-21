using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Collections;
using System.Reflection;

namespace EQEmu.Database
{
    //need the sql query

    public class QueryConfig
    {
        private static string _XMLTagName = "property-mapping";
        public static string XMLTagName
        {
            get { return _XMLTagName; }
        }

        private Dictionary<Type, TypeQueries> _typeQueryMappings = new Dictionary<Type, TypeQueries>();
        public Dictionary<Type, TypeQueries> TypeQueryMappings
        {
            get { return _typeQueryMappings; }
        }

        public static QueryConfig Create(XElement root)
        {
            var config = new QueryConfig();

            if (root.Name != XMLTagName) return null;

            foreach (var query in root.Elements(TypeQueries.XMLTagName))
            {
                var type = query.Attribute("type");
                if (type == null) continue;

                var objType = Assembly.GetExecutingAssembly().GetType(type.Value);
                if (objType == null) continue;
                
                config._typeQueryMappings[objType] = TypeQueries.Create(query);
            }

            foreach (var query in root.Elements(TypeQueriesExtension.XMLTagName))
            {
                var type = query.Attribute("type");
                if (type == null) continue;

                var objType = Assembly.GetExecutingAssembly().GetType(type.Value);

                if (config._typeQueryMappings.ContainsKey(objType))
                {
                    var obj = TypeQueriesExtension.Create(query) as TypeQueriesExtension;
                    if (obj != null)
                    {
                        config.TypeQueryMappings[objType].ExtensionQueries.Add(obj);
                    }
                }
            }

            return config;
        }
    }

    public class TypeQueriesExtension : TypeQueries
    {
        private static string _XMLTagName = "query-extension";
        public new static string XMLTagName
        {
            get
            {
                return _XMLTagName;
            }
        }

        public string Name { get; set; }
    }

    public class TypeQueries
    {
        private List<TypeQueriesExtension> _extQueries = new List<TypeQueriesExtension>();
        public List<TypeQueriesExtension> ExtensionQueries
        {
            get { return _extQueries; }
        }

        private static string _XMLTagName = "query";
        private static string _XMLTagNameExt = "query-extension";
        private static string _XMLInsertTag = "insert";
        private static string _XMLDeleteTag = "delete";
        private static string _XMLUpdateTag = "update";
        private static string _XMLSelectTag = "select";      

        private static string _XMLSQLTag = "sql";
        private static string _XMLParamTag = "param";
        private static string _XMLStoreTag = "store";
        private static string _XMLStoreFieldAttr = "field";
        private static string _XMLStoreTypeAttr = "type";

        public static string XMLTagName
        {
            get { return _XMLTagName; }
        }

        public static TypeQueries Create(XElement element)
        {
            if (element.Name != _XMLTagName && element.Name != _XMLTagNameExt) return null;

            TypeQueries tq;
            if (element.Name == XMLTagName) tq = new TypeQueries();
            else
            {
                tq = new TypeQueriesExtension();
                var name = element.Attribute("name");
                if (name != null) (tq as TypeQueriesExtension).Name = name.Value;
            }

            var insert = element.Element(_XMLInsertTag);
            //if (insert == null) throw new Exception( String.Format("Undefined insert query - missing '{0}' tag",_XMLInsertTag) );
            if( insert != null )
            {
                var sql = insert.Element(_XMLSQLTag);
                if (sql == null) throw new Exception(String.Format("Undefined query string - missing '{0}' tag", _XMLSQLTag));
                tq.InsertQuery = sql.Value;

                var list = new List<string>();
                foreach (var param in insert.Elements(_XMLParamTag))
                {
                    //tq._insertParameters.Add(param.Value);
                    list.Add(param.Value);
                }
                tq._insertArgs = list.ToArray();
            }

            var delete = element.Element(_XMLDeleteTag);
            //if (delete == null) throw new Exception(String.Format("Undefined delete query - missing '{0}' tag", _XMLDeleteTag));
            if( delete != null )
            {
                var sql = delete.Element(_XMLSQLTag);
                if (sql == null) throw new Exception(String.Format("Undefined query string - missing '{0}' tag", _XMLSQLTag));
                tq.DeleteQuery = sql.Value;

                var list = new List<string>();
                foreach (var param in delete.Elements(_XMLParamTag))
                {
                    list.Add(param.Value);
                    //tq._deleteParameters.Add(param.Value);
                }

                tq._deleteArgs = list.ToArray();
            }

            var update = element.Element(_XMLUpdateTag);
            //if (update == null) throw new Exception(String.Format("Undefined update query - missing '{0}' tag", _XMLUpdateTag));
            if( update != null )
            {
                var sql = update.Element(_XMLSQLTag);
                if (sql == null) throw new Exception(String.Format("Undefined query string - missing '{0}' tag", _XMLSQLTag));
                tq.UpdateQuery = sql.Value;

                var list = new List<string>();
                foreach (var param in update.Elements(_XMLParamTag))
                {                    
                    //tq._updateParameters.Add(param.Value);
                    list.Add(param.Value);
                }
                tq._updateArgs = list.ToArray();
            }

            var select = element.Element(_XMLSelectTag);
            if (select != null)
            {
                var sql = select.Element(_XMLSQLTag);
                if (sql == null) throw new Exception(String.Format("Undefined query string - missing '{0}' tag", _XMLSQLTag));
                tq.SelectQuery = sql.Value;

                var list = new List<string>();
                foreach (var param in select.Elements(_XMLParamTag))
                {
                    //tq._selectParameters.Add(param.Value);
                    list.Add(param.Value);
                }
                tq._selectArgs = list.ToArray();

                foreach (var store in select.Elements(_XMLStoreTag))
                {
                    var field = store.Attribute(_XMLStoreFieldAttr);
                    if (field == null) continue;

                    var type = store.Attribute(_XMLStoreTypeAttr);
                    if (type == null) continue;

                    var property = store.Value;
                    if (property == null) continue;

                    SelectQueryFieldStore.DataTypes etype;
                    switch(type.Value)
                    {
                        case "int": etype = SelectQueryFieldStore.DataTypes.Int;
                            break;
                        case "float": etype = SelectQueryFieldStore.DataTypes.Float;
                            break;
                        case "string": etype = SelectQueryFieldStore.DataTypes.String;
                            break;
                        case "bool": etype = SelectQueryFieldStore.DataTypes.Bool;
                            break;
                        case "long": etype = SelectQueryFieldStore.DataTypes.Long;
                            break;
                        default:
                            etype = SelectQueryFieldStore.DataTypes.String;
                            break;
                    }

                    tq._selectQueryFields.Add(new SelectQueryFieldStore(field.Value, property, etype));
                }
            }

            return tq;
        }

        public TypeQueries()
        {

        }

        private string[] _insertArgs;
        public string[] InsertArgs
        {
            get { return _insertArgs; }
        }

        public string InsertQuery
        {
            get;
            set;
        }

        private string[] _deleteArgs;
        public string[] DeleteArgs
        {
            get { return _deleteArgs; }
        }

        public string DeleteQuery
        {
            get;
            set;
        }

        private string[] _updateArgs;
        public string[] UpdateArgs
        {
            get { return _updateArgs; }
        }

        public string UpdateQuery
        {
            get;
            set;
        }

        private List<SelectQueryFieldStore> _selectQueryFields = new List<SelectQueryFieldStore>();
        public List<SelectQueryFieldStore> SelectQueryFields
        {
            get { return _selectQueryFields; }
        }

        private string[] _selectArgs;
        public string[] SelectArgs
        {
            get { return _selectArgs; }
        }

        public string SelectQuery
        {
            get;
            set;
        }
    }

    public class SelectQueryFieldStore
    {
        public enum DataTypes
        {
            Int,
            Float,
            String,
            Bool,
            Long
        }

        private string _column;
        public string Column
        {
            get { return _column; }
        }

        private string _property;
        public string Property
        {
            get { return _property; }
        }

        private DataTypes _datatype;
        public DataTypes Type
        {
            get { return _datatype; }
        }

        public SelectQueryFieldStore(string columnName, string propertyName, DataTypes dataType)
        {
            _column = columnName;
            _property = propertyName;
            _datatype = dataType;
        }
    }
}
