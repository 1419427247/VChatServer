using System.Data;
using System.Data.SQLite;
using System.Reflection;
using System.Text;

namespace VChatService.Orm;
public class SqliteHelper
{
    SQLiteConnection connection;
    public SqliteHelper(string connectionString)
    {
        connection = new SQLiteConnection(connectionString);
        connection.Open();
        Assembly assembly = Assembly.GetExecutingAssembly();

        // 获取所有具有Table属性的类
        var tableTypes = assembly.GetTypes()
            .Where(t => t.GetCustomAttribute<TableAttribute>() != null);

        foreach (var type in tableTypes)
        {
            VChat.logger.Info($"Loading table : {type.Name}");
            CreateTable(type);
        }
        VChat.logger.Info("All tables loaded");
    }
    public T Insert<T>(T value) where T : notnull
    {
        Type type = value.GetType();
        TableAttribute? tableAttribute = type.GetCustomAttribute<TableAttribute>();
        if (tableAttribute == null)
        {
            throw new Exception("没有Table属性");
        }
        string tableName = tableAttribute.Name;
        StringBuilder insertCommandText = new StringBuilder($"INSERT INTO {tableName} (");
        List<object?> parameters = new List<object?>();
        var properties = type.GetProperties();
        foreach (PropertyInfo property in properties)
        {
            ColumnAttribute? columnAttribute = property.GetCustomAttribute<ColumnAttribute>();
            if (columnAttribute != null)
            {
                insertCommandText.Append($"{columnAttribute.Name},");
                parameters.Add(property.GetValue(value));
            }
        }
        insertCommandText.Remove(insertCommandText.Length - 1, 1);
        insertCommandText.Append(") VALUES (");
        for (int i = 0; i < parameters.Count; i++)
        {
            insertCommandText.Append("?,");
        }
        insertCommandText.Remove(insertCommandText.Length - 1, 1);
        insertCommandText.Append(")");
        ExecuteNonQuery(insertCommandText.ToString(), parameters.ToArray());
        return value;
    }
    public T? Select<T>(T value) where T : notnull
    {
        Type type = value.GetType();
        TableAttribute? tableAttribute = type.GetCustomAttribute<TableAttribute>();
        if (tableAttribute == null)
        {
            throw new Exception("没有Table属性");
        }
        string tableName = tableAttribute.Name;
        StringBuilder selectCommandText = new StringBuilder($"SELECT * FROM {tableName} WHERE ");
        List<object> parameters = new List<object>();
        var properties = type.GetProperties();
        foreach (PropertyInfo property in properties)
        {
            ColumnAttribute? columnAttribute = property.GetCustomAttribute<ColumnAttribute>();
            if (columnAttribute != null)
            {
                object? parameter = property.GetValue(value);
                if (parameter == null)
                {
                    continue;
                }
                selectCommandText.Append($"{columnAttribute.Name}=? AND ");
                parameters.Add(parameter);
            }
        }
        selectCommandText.Remove(selectCommandText.Length - 5, 5);
        DataTable? dataTable = ExecuteQuery(selectCommandText.ToString(), parameters.ToArray());
        if (dataTable == null)
        {
            return default;
        }
        DataRow dataRow = dataTable.Rows[0];
        foreach (PropertyInfo property in properties)
        {
            ColumnAttribute? columnAttribute = property.GetCustomAttribute<ColumnAttribute>();
            if (columnAttribute != null)
            {
                property.SetValue(value, dataRow[columnAttribute.Name] == DBNull.Value ? default : dataRow[columnAttribute.Name]);
            }
        }
        return value;
    }
    public T Update<T>(T value)
    {
        Type type = value.GetType();
        TableAttribute? tableAttribute = type.GetCustomAttribute<TableAttribute>();
        if (tableAttribute == null)
        {
            throw new Exception("没有Table属性");
        }
        string tableName = tableAttribute.Name;
        StringBuilder updateCommandText = new StringBuilder($"UPDATE {tableName} SET ");
        List<object?> parameters = new List<object?>();
        var properties = type.GetProperties();
        foreach (PropertyInfo property in properties)
        {
            ColumnAttribute? columnAttribute = property.GetCustomAttribute<ColumnAttribute>();
            if (columnAttribute != null)
            {
                object? parameter = property.GetValue(value);
                if (parameter == null)
                {
                    continue;
                }
                updateCommandText.Append($"{columnAttribute.Name}=?,");
                parameters.Add(parameter);
            }
        }
        updateCommandText.Remove(updateCommandText.Length - 1, 1);
        updateCommandText.Append(" WHERE ");
        foreach (PropertyInfo property in properties)
        {
            PrimaryKeyAttribute? primaryKeyAttribute = property.GetCustomAttribute<PrimaryKeyAttribute>();
            if (primaryKeyAttribute != null)
            {
                ColumnAttribute? columnAttribute = property.GetCustomAttribute<ColumnAttribute>();
                if (columnAttribute != null)
                {
                    updateCommandText.Append($"{columnAttribute.Name}=?");
                    parameters.Add(property.GetValue(value));
                }
            }
        }
        ExecuteNonQuery(updateCommandText.ToString(), parameters.ToArray());
        return value;
    }
    public T Delete<T>(T value)
    {
        Type type = value.GetType();
        TableAttribute? tableAttribute = type.GetCustomAttribute<TableAttribute>();
        if (tableAttribute == null)
        {
            throw new Exception("没有Table属性");
        }
        string tableName = tableAttribute.Name;
        StringBuilder deleteCommandText = new StringBuilder($"DELETE FROM {tableName} WHERE ");
        List<object?> parameters = new List<object?>();
        var properties = type.GetProperties();
        foreach (PropertyInfo property in properties)
        {
            PrimaryKeyAttribute? primaryKeyAttribute = property.GetCustomAttribute<PrimaryKeyAttribute>();
            if (primaryKeyAttribute != null)
            {
                ColumnAttribute? columnAttribute = property.GetCustomAttribute<ColumnAttribute>();
                if (columnAttribute != null)
                {
                    deleteCommandText.Append($"{columnAttribute.Name}=?");
                    parameters.Add(property.GetValue(value));
                }
            }
        }
        ExecuteNonQuery(deleteCommandText.ToString(), parameters.ToArray());
        return value;
    }
    private void CreateTable(Type type)
    {
        var tableAttribute = type.GetCustomAttribute<TableAttribute>();
        string tableName = tableAttribute!.Name;
        string checkTableExistsCommandText = $"SELECT name FROM sqlite_master WHERE type='table' AND name=?";
        if (ExecuteQuery(checkTableExistsCommandText, new object[] { tableName }) == null)
        {
            VChat.logger.Info($"{tableName} not exists, creating...");
            StringBuilder createTableCommandText = new StringBuilder($"CREATE TABLE {tableName} (");
            List<object> parameters = new List<object>();
            var properties = type.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                ColumnAttribute? columnAttribute = property.GetCustomAttribute<ColumnAttribute>();
                if (columnAttribute != null && GetColumnType(property) != null)
                {
                    createTableCommandText.Append($"{columnAttribute.Name} {GetColumnType(property)}");
                }
                PrimaryKeyAttribute? primaryKeyAttribute = property.GetCustomAttribute<PrimaryKeyAttribute>();
                if (primaryKeyAttribute != null)
                {
                    createTableCommandText.Append(" PRIMARY KEY");
                }
                createTableCommandText.Append(",");
            }
            createTableCommandText.Remove(createTableCommandText.Length - 1, 1);
            createTableCommandText.Append(")");
            ExecuteNonQuery(createTableCommandText.ToString(), parameters.ToArray());
        }
        else
        {
            string checkColumnExistsCommandText = $"PRAGMA table_info({tableName})";
            DataTable? dataTable = ExecuteQuery(checkColumnExistsCommandText, new object[] { });
            if (dataTable == null)
            {
                VChat.logger.Error($"table {tableName} exists, but cannot get columns");
                return;
            }
            VChat.logger.Info($"table {tableName} exists, checking columns");
            var properties = type.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                ColumnAttribute? columnAttribute = property.GetCustomAttribute<ColumnAttribute>();
                if (columnAttribute != null && GetColumnType(property) != null)
                {
                    string columnName = columnAttribute.Name;
                    bool columnExists = false;
                    foreach (DataRow row in dataTable.Rows)
                    {
                        if (row["name"].ToString() == columnName)
                        {
                            columnExists = true;
                            break;
                        }
                    }
                    if (columnExists == false)
                    {
                        string addColumnCommandText = $"ALTER TABLE {tableName} ADD COLUMN {columnName} {GetColumnType(property)}";
                        ExecuteNonQuery(addColumnCommandText, new object[] { });
                        VChat.logger.Info($"column {columnName} added");
                    }
                }
            }
        }
        VChat.logger.Info($"table {tableName} checked");
    }
    public void ExecuteNonQuery(string query, object?[] parameters)
    {
        using (SQLiteCommand cmd = new SQLiteCommand(query, connection))
        {
            foreach (var parameter in parameters)
            {
                cmd.Parameters.Add(new SQLiteParameter(null, parameter));
            }
            cmd.ExecuteNonQuery();
        }
    }
public DataTable? ExecuteQuery(string query, object?[] parameters)
{
    using (SQLiteCommand cmd = new SQLiteCommand(query, connection))
    {
        foreach (var parameter in parameters)
        {
            cmd.Parameters.Add(new SQLiteParameter(null, parameter));
        }
        if (cmd.ExecuteScalar() == null)
        {
            return null;
        }
        else
        {
            using (SQLiteDataAdapter dataAdapter = new SQLiteDataAdapter(cmd))
            {
                DataTable dataTable = new DataTable();
                dataAdapter.Fill(dataTable);
                return dataTable;
            }
        }
    }
}

private string GetColumnType(PropertyInfo property)
{
    ColumnAttribute? columnAttribute = property.GetCustomAttribute<ColumnAttribute>();
    if (columnAttribute != null)
    {
        switch (property.PropertyType.Name)
        {
            case "Int32":
                return "INTEGER";
            case "Int64":
                return "INTEGER";
            case "Float":
                return "REAL";
            case "Double":
                return "REAL";
            case "String":
                return "TEXT";
            default:
                throw new Exception("不支持的类型");
        }
    }
    throw new Exception("没有Column属性");
}
}