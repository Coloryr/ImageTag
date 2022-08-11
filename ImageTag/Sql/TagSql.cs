using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.IO;

namespace ImageTag.Sql;

internal static class TagSql
{
    private static readonly string DB = App.Local + "Tags.db";
    private static string connStr;

    public static void Start()
    {
        bool isNew = !File.Exists(DB);

        connStr = new SqliteConnectionStringBuilder("Data Source=" + DB)
        {
            Mode = SqliteOpenMode.ReadWriteCreate
        }.ToString();
        using var sql = new SqliteConnection(connStr);
        sql.Execute(@"create table if not exists taggroup (
  `id` integer NOT NULL PRIMARY KEY AUTOINCREMENT,
  `name` text,
  `uuid` text
);");

        sql.Execute(@"create table if not exists tags (
  `id` integer NOT NULL PRIMARY KEY AUTOINCREMENT,
  `name` text,
  `uuid` text,
  `group` text,
  `bind_group` text,
  `bind_uuid` text
);");

        if (isNew)
        {
            NewGroup("方向");
            NewGroup("作品");
            NewGroup("角色");
            NewGroup("人数");
        }
    }

    public static List<TagGroupObj> GetAllGroup()
    {
        using var sql = new SqliteConnection(connStr);
        var list = sql.Query<TagGroupObj>("SELECT name,uuid FROM taggroup");

        return list.ToList();
    }

    public static bool HaveGroup(string name)
    {
        using var sql = new SqliteConnection(connStr);
        var list = sql.Query("SELECT id FROM taggroup WHERE name=@name", new { name });

        return list.Any();
    }

    public static bool HaveGroupU(string uuid)
    {
        using var sql = new SqliteConnection(connStr);
        var list = sql.Query("SELECT id FROM taggroup WHERE uuid=@uuid", new { uuid });

        return list.Any();
    }

    public static void NewGroup(string name)
    {
        using var sql = new SqliteConnection(connStr);
        string uuid;
        while (true)
        {
            uuid = ImageTagUtils.NewUUID();
            var res1 = sql.Query($"SELECT `id` FROM taggroup WHERE uuid=@uuid", new { uuid });
            if (res1.Any())
                continue;
            break;
        }

        sql.Execute($"INSERT INTO taggroup(`uuid`,`name`) " +
            $"VALUES (@uuid,@name)", new { uuid, name });
    }

    public static void RemoveGroup(string uuid)
    {
        if (!HaveGroup(uuid))
            return;

        using var sql = new SqliteConnection(connStr);
        sql.Execute($"DELETE FROM taggroup WHERE uuid=@uuid", new { uuid });
        sql.Execute($"DELETE FROM tags WHERE group=@uuid", new { uuid });
        sql.Execute("UPDATE tags SET bind_group='',bind_uuid='' WHERE bind_group=@uuid",
            new { uuid });
    }

    public static List<TagObj> GetTags(string group)
    {
        using var sql = new SqliteConnection(connStr);
        var list = sql.Query<TagObj>("SELECT uuid,name,group,bind_group,bind_uuid FROM tag " +
            "WHERE group=@group", new { group });

        return list.ToList();
    }

    public static TagObj? GetTag(string group, string uuid)
    {
        using var sql = new SqliteConnection(connStr);
        var list = sql.Query<TagObj>("SELECT uuid,name,group,bind_group,bind_uuid FROM tag " +
            "WHERE group=@group AND uuid=@uuid", new { group, uuid });

        return list.FirstOrDefault();
    }

    public static bool HaveTag(string group, string uuid)
    {
        using var sql = new SqliteConnection(connStr);
        var list = sql.Query<TagObj>("SELECT id FROM tag WHERE group=@group AND uuid=@uuid",
            new { group, uuid });

        return list.Any();
    }

    public static void NewTag(TagObj tag)
    {
        using var sql = new SqliteConnection(connStr);
        string uuid;
        while (true)
        {
            uuid = ImageTagUtils.NewUUID();
            var res1 = sql.Query($"SELECT `id` FROM taggroup WHERE uuid=@uuid", new { uuid });
            if (res1.Any())
                continue;
            break;
        }

        sql.Execute($"INSERT INTO tag(`uuid`,`name`,`group`,`bind_group`,`bind_uuid`) " +
            $"VALUES (@uuid,@name,@group,@bind_group,@bind_uuid)",
            new { uuid, tag.name, tag.group, tag.bind_uuid, tag.bind_group });
    }

    public static void RemoveGroupTag(string group)
    {
        
    }

    public static void RemoveTag(string group, string uuid)
    {
        if (!HaveTag(group, uuid))
            return;

        using var sql = new SqliteConnection(connStr);
        sql.Execute($"DELETE FROM tags WHERE group=@group AND uuid=@uuid", new { group, uuid });
        sql.Execute("UPDATE tags SET bind_group='',bind_uuid='' WHERE bind_uuid=@uuid AND bind_group=@group",
            new { group, uuid });
    }
}
