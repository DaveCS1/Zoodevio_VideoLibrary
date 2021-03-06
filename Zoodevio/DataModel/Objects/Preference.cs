﻿/*
 * Represents a preference selection.
 * For example, defaulting to grid or library view.
 */

namespace Zoodevio.DataModel.Objects
{
    public class Preference
    {
        // the preference's id -- set by database
        public int Id { get;  }

        // the preference's name
        public string Name; 

        // the preference's data type in the DB
        // represented as a string because this is 
        // just about how to parse text data
        public string DataType;

        // the preference data
        public string Data; 

        // preferences must be constructed with all relevant data 
        // since they are never added or deleted, that's easy
        public Preference(int id, string name, string datatype, string data)
        {
            Id = id;
            Name = name;
            DataType = datatype;
            Data = data;
        }

    }
}