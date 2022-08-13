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
            AddGroup("方向");
            AddGroup("作品");
            AddGroup("角色");
            AddGroup("人数");

            var group = GetGroup("方向");

            AddTag(new TagObj()
            {
                group = group.uuid,
                name = "横向"
            });

            AddTag(new TagObj()
            {
                group = group.uuid,
                name = "纵向"
            });

            AddTag(new TagObj()
            {
                group = group.uuid,
                name = "矩形"
            });

            group = GetGroup("人数");

            AddTag(new TagObj()
            {
                group = group.uuid,
                name = "0"
            });
            AddTag(new TagObj()
            {
                group = group.uuid,
                name = "1"
            });
            AddTag(new TagObj()
            {
                group = group.uuid,
                name = "2"
            });
            AddTag(new TagObj()
            {
                group = group.uuid,
                name = "3"
            });
            AddTag(new TagObj()
            {
                group = group.uuid,
                name = "4"
            });
        }
    }

    public static List<TagGroupObj> GetAllGroup()
    {
        using var sql = new SqliteConnection(connStr);
        var list = sql.Query<TagGroupObj>("SELECT name,uuid FROM taggroup");

        return list.ToList();
    }

    public static TagGroupObj? GetGroup(string name)
    {
        using var sql = new SqliteConnection(connStr);
        var list = sql.Query<TagGroupObj>("SELECT name,uuid FROM taggroup WHERE name=@name", 
            new { name });

        return list.ToList().FirstOrDefault();
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

    public static void AddGroup(string name)
    {
        using var sql = new SqliteConnection(connStr);
        string uuid;
        while (true)
        {
            uuid = ImageTagUtils.NewUUID();
            var res1 = sql.Query("SELECT `id` FROM taggroup WHERE uuid=@uuid", new { uuid });
            if (res1.Any())
                continue;
            break;
        }

        sql.Execute("INSERT INTO taggroup(`uuid`,`name`) VALUES (@uuid,@name)", new { uuid, name });
    }

    public static void RemoveGroup(string uuid)
    {
        if (!HaveGroupU(uuid))
            return;

        using var sql = new SqliteConnection(connStr);
        sql.Execute("DELETE FROM taggroup WHERE uuid=@uuid", new { uuid });
        sql.Execute("DELETE FROM tags WHERE `group`=@uuid", new { uuid });
        sql.Execute("UPDATE tags SET bind_group='',bind_uuid='' WHERE bind_group=@uuid",
            new { uuid });
    }

    public static List<TagObj> GetTags(string group)
    {
        using var sql = new SqliteConnection(connStr);
        var list = sql.Query<TagObj>("SELECT uuid,name,`group`,bind_group,bind_uuid FROM tags " +
            "WHERE `group`=@group", new { group });

        return list.ToList();
    }

    public static TagObj? GetTag(string group, string uuid)
    {
        using var sql = new SqliteConnection(connStr);
        var list = sql.Query<TagObj>("SELECT uuid,name,`group`,bind_group,bind_uuid FROM tags " +
            "WHERE `group`=@group AND uuid=@uuid", new { group, uuid });

        return list.FirstOrDefault();
    }

    public static bool HaveTag(string group, string name)
    {
        using var sql = new SqliteConnection(connStr);
        var list = sql.Query<TagObj>("SELECT id FROM tags WHERE `group`=@group AND name=@name",
            new { group, name });

        return list.Any();
    }

    public static bool HaveTagU(string group, string uuid)
    {
        using var sql = new SqliteConnection(connStr);
        var list = sql.Query<TagObj>("SELECT id FROM tags WHERE `group`=@group AND uuid=@uuid",
            new { group, uuid });

        return list.Any();
    }

    public static void AddTag(TagObj tag)
    {
        using var sql = new SqliteConnection(connStr);
        string uuid;
        while (true)
        {
            uuid = ImageTagUtils.NewUUID();
            var res1 = sql.Query("SELECT `id` FROM taggroup WHERE uuid=@uuid", new { uuid });
            if (res1.Any())
                continue;
            break;
        }

        sql.Execute("INSERT INTO tags(`uuid`,`name`,`group`,`bind_group`,`bind_uuid`) " +
            "VALUES (@uuid,@name,@group,@bind_group,@bind_uuid)",
            new { uuid, tag.name, tag.group, tag.bind_uuid, tag.bind_group });
    }

    public static List<TagObj> GetGroupTag(string uuid) 
    {
        using var sql = new SqliteConnection(connStr);
        var list = sql.Query<TagObj>("SELECT uuid,name,`group`,bind_group,bind_uuid FROM tags " +
            "WHERE `group`=@uuid", new { uuid });

        return list.ToList();
    }

    public static void RemoveGroupTag(string group)
    {
        
    }

    public static void RemoveTag(string group, string uuid)
    {
        if (!HaveTagU(group, uuid))
            return;

        using var sql = new SqliteConnection(connStr);
        sql.Execute("DELETE FROM tags WHERE `group`=@group AND uuid=@uuid", new { group, uuid });
        sql.Execute("UPDATE tags SET bind_group='',bind_uuid='' WHERE bind_uuid=@uuid AND bind_group=@group",
            new { group, uuid });
    }

    public static void EditTag(TagObj obj)
    {
        using var sql = new SqliteConnection(connStr);
        sql.Execute("UPDATE tags SET bind_group=@bind_group,bind_uuid=@bind_uuid,name=@name WHERE `group`=@group AND uuid=@uuid",
            new { obj.group, obj.uuid, obj.bind_group, obj.bind_uuid, obj.name });
    }
}
