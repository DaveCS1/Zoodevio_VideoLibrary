﻿/*
 * Accesses and executes direct queries to the Zoodevio database.
 * This is an SQLite database, database.sqlite, located in
 * the Zoodevio installation. 
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Net;
using System.Reflection;
using NUnit.Framework;

namespace Zoodevio.DataModel
{
    public static class Database
    {
        // location codes for LIKE queries
        public enum LikeLocation
        {
            Before,
            After,
            Both
        }

        // the directory where Zoodevio is executing
        private static readonly string PROJECT_DIRECTORY = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        // the database filename
        // never changed while Zoodevio is executing
        // in the future, perhaps could be read from an ini or registry key? 
        private static readonly string DATABASE_FILE = PROJECT_DIRECTORY + "\\..\\..\\database.sqlite";

        // the SQLite connection. all database access is done through this class
        // so this connection is private.
        private static SQLiteConnection _dbConnection = new SQLiteConnection(
            "Data Source=" + DATABASE_FILE + ";Version=3;PRAGMA foreign_keys = 1");

        // executes a basic select * query from a table
        public static SQLiteDataReader SimpleStarQuery(string table)
        {
            if (_dbConnection.State == ConnectionState.Open)
            {
                _dbConnection.Close();
            }
            _dbConnection.Open();
            List<IDataRecord> data = new List<IDataRecord>();
            // build the query
            SQLiteCommand com = new SQLiteCommand("select * from " + table, _dbConnection);
            return com.ExecuteReader();
        }

        // executes a basic read query (select * from table where column is value) 
        public static SQLiteDataReader SimpleReadQuery(string table, string column, string value)
        {
            if (_dbConnection.State == ConnectionState.Open)
            {
                _dbConnection.Close();
            }
            _dbConnection.Open(); 
            List<IDataRecord> data = new List<IDataRecord>();
            // build the query
            SQLiteCommand com =
                new SQLiteCommand("select * from " + table + " where " + column + " = " + value , _dbConnection);
            return com.ExecuteReader();

        }

        // iterate over a data reader object
        private static IEnumerable<IDataRecord> GetFromReader(IDataReader reader)
        {
            while (reader.Read()) yield return reader;
        }

        // get raw bytes of data from a row (for reading blobs) 
        // adapted from stackoverflow #625029
        public static byte[] GetBytes(IDataRecord row, int field)
        {
            const int CHUNK_SIZE = 2*1024;
            byte[] buffer = new byte[CHUNK_SIZE];
            long bytesRead;
            long fieldOffset = 0;
            using (MemoryStream stream = new MemoryStream())
            {
                while ((bytesRead = row.GetBytes(field, fieldOffset, buffer, 0, buffer.Length)) > 0)
                {
                    stream.Write(buffer, 0, (int) bytesRead);
                    fieldOffset += bytesRead;
                }
                return stream.ToArray();
            }
        }

        // perform a LIKE query on a given database for a given column/input string
        public static SQLiteDataReader ReadLikeQuery(string table, string column, string value, LikeLocation loc)
        {
            if (_dbConnection.State == ConnectionState.Open)
            {
                _dbConnection.Close();
            }
            _dbConnection.Open();
            SQLiteCommand com =
                new SQLiteCommand("select * from " + table + " where '" + column + "' LIKE '" + GetWildcardedString(value, loc) + "'", _dbConnection);
            _dbConnection.Close(); 
            return com.ExecuteReader();

        }

        // get the LIKE string for a given string, based on where the like wildcards should be
        private static string GetWildcardedString(string value, LikeLocation loc)
        {
            switch (loc)
            {
                case LikeLocation.Before:
                    return "%" + value;
                case LikeLocation.After:
                    return value + "%";
                case LikeLocation.Both:
                    return "%" + value + "%";
                default:
                    throw new ArgumentException("invalid LikeLocation provided!");
            }
        }

        // insert a new record into the database; return success or failure
        // note: data string should be formatted correctly (ints without quotes, etc.)
        public static Boolean SimpleInsertQuery(string table, 
            string[] rows, string[] data)
        {
            if (_dbConnection.State == ConnectionState.Open)
            {
                _dbConnection.Close();
            }
            _dbConnection.Open();
            string rowStatement = String.Join(", ", rows);
            string dataStatement = String.Join("', '", data);
            string query = "insert into " + table + " (" + rowStatement + ") values ('" + dataStatement + "')";
            Console.Write(query);
            SQLiteCommand com = new SQLiteCommand("insert into "+table+" ("+rowStatement+") values ('"+dataStatement+"')",_dbConnection);
            try
            {
                com.ExecuteNonQuery();
                _dbConnection.Close(); 
                return true;
            }
            catch(Exception e)
            {
                _dbConnection.Close();
                Console.Write(e.ToString());
                return false;
            }
        }

        // update a record with new values in the database; return success or failure
        // note: data string should be formatted correctly (ints without quotes, etc.)
        public static Boolean SimpleUpdateQuery(string table, string identifierField, int identifier,
            string[] rows, string[] data)
        {
            if (_dbConnection.State == ConnectionState.Open)
            {
                _dbConnection.Close();
            }
            _dbConnection.Open();
            string setCommand = BuildSetCommand(rows, data); 
            SQLiteCommand com = new SQLiteCommand("update " + table + " set " + setCommand + " WHERE " + identifierField + " = " + identifier,_dbConnection);
            try
            {
                com.ExecuteNonQuery();
                _dbConnection.Close();
                return true;
            }
            catch
            {
                _dbConnection.Close();
                return false;
            }
        }

        // builds a SET statement with a list of columns to set up 
        private static string BuildSetCommand(string[] rows, string[] data)
        {
            string output = "";
            for(int i = 0; i < rows.Length; i++)
            {
                output += rows[i] + " = " + data[i];
                if (i < rows.Length - 1)
                {
                    output += ", "; 
                }
            }
            return output; 
        }

        // deletes a row from the database
        // returns true if successful, false if failed
        public static bool SimpleDeleteQuery(string table, string identifier, int id)
        {
            if (_dbConnection.State == ConnectionState.Open)
            {
                _dbConnection.Close();
            }
            _dbConnection.Open();
            SQLiteCommand com = new SQLiteCommand("delete from " + table + " where " + id + " = " + id, _dbConnection);
            try
            {
                com.ExecuteNonQuery();
                _dbConnection.Close();
                return true;
            }
            catch
            {
                _dbConnection.Close();
                return false; 
            }
        }

        // truncates a database table
        // WARNING: THIS KILLS THE TABLE (silently)
        public static bool TruncateTable(string table)
        {
            if (_dbConnection.State == ConnectionState.Open)
            {
                _dbConnection.Close();
            }
            _dbConnection.Open(); 
            SQLiteCommand com = new SQLiteCommand("delete from " + table + "; vacuum", _dbConnection);
            // reset autoincrementing
            SQLiteCommand com2 = new SQLiteCommand("delete from sqlite_sequence where name = '" + table + "'");
            try
            {
                com.ExecuteNonQuery();
                com2.ExecuteNonQuery();
                _dbConnection.Close();
                return true;
            }
            catch
            {
                _dbConnection.Close();
                return false;
            }
        }

    }
}