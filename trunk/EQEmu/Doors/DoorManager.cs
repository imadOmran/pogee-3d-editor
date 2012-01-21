using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

using MySql.Data.MySqlClient;

namespace EQEmu.Doors
{
    public class DoorManager
    {
        private readonly MySqlConnection _connection;

        private List<Door> _doors = new List<Door>();
        public List<Door> Doors
        {
            get
            {
                return _doors;
            }
        }

        public DoorManager(MySqlConnection connection)
        {
            if (connection == null)
            {
                throw new NullReferenceException();
            }
            _connection = connection;
        }

        public void RetrieveDoors(string zone)
        {
            if (_connection == null)
            {
                throw new NullReferenceException();
            }
            if (_connection.State == System.Data.ConnectionState.Open)
            {
                try
                {
                    string sql = String.Format("SELECT * FROM doors WHERE zone='{0}'", zone);
                    MySqlCommand cmd = new MySqlCommand(sql, _connection);
                    MySqlDataReader rdr = cmd.ExecuteReader();

                    Door door;

                    while (rdr.Read())
                    {
                        door = new EQEmu.Doors.Door(null)
                        {
                            Location = new Point3D(rdr.GetDouble("pos_x"), rdr.GetDouble("pos_y"), rdr.GetDouble("pos_z")),
                            DoorId = rdr.GetInt16("doorid"),
                            Heading = rdr.GetFloat("heading"),
                            Zone = rdr.GetString("zone"),
                            Name = rdr.GetString("name"),
                            OpenType = (Door.OpenTypes)rdr.GetInt16("opentype"),
                            Guild = rdr.GetInt16("guild"),
                            KeyItem = rdr.GetInt16("keyitem"),
                            LockPick = rdr.GetInt16("lockpick"),
                            NumKeyRing = rdr.GetUInt16("nokeyring"),
                            TriggerDoor = rdr.GetInt16("triggerdoor"),
                            TriggerType = rdr.GetInt16("triggertype"),
                            DoorIsOpen = rdr.GetInt16("doorisopen"),
                            DoorParam = rdr.GetInt16("door_param"),

                            DestinationInstance = rdr.GetUInt16("dest_instance"),
                            DestinationZone = rdr.GetString("dest_zone"),
                            DestinationHeading = rdr.GetFloat("dest_heading"),
                            InvertState = rdr.GetInt16("invert_state"),
                            Incline = rdr.GetInt16("incline"),
                            Size = rdr.GetInt16("size"),

                            Buffer = rdr.GetFloat("buffer"),
                            //ClientVersionMask = rdr.GetUInt16("client_version_mask"),
                            IsLdonDoor = rdr.GetUInt16("is_ldon_door")
                        };
                        door.Created();

                        Doors.Add(door);
                    }
                    rdr.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            else
            {
                throw new Exception("Unknown Connection State");
            }
        }
    }
}
