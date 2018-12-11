using System;
using MySql.Data;
using MySql.Data.MySqlClient;

public class DBConnection {
    private DBConnection () {
    }

    private string databaseName = string.Empty;
    public string DatabaseName {
        get { return databaseName; }
        set { databaseName = value; }
    }

    public string Password { get; set; }
    private MySqlConnection connection = null;
    public MySqlConnection Connection {
        get { return connection; }
    }

    private static DBConnection _instance = null;
    public static DBConnection Instance () {
        _instance = new DBConnection();
        return _instance;
    }

    public bool IsConnect () {
        if (Connection == null) {
            if (String.IsNullOrEmpty(databaseName))
                return false;
            string connstring = string.Format("Server=localhost; database={0}; UID=root; password=***REMOVED***", databaseName);
            connection = new MySqlConnection(connstring);
            connection.Open();
        }

        return true;
    }

    public void Close () {
        connection.Close();
        _instance = null;
    }
}