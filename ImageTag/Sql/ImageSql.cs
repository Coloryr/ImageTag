using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Documents;

namespace ImageTag.Sql;

internal static class ImageSql
{
    private static readonly string DB = App.Local + "Image.db";
    public static readonly string Local = App.Local + "Images/";
    private static string connStr;
    public static Dictionary<ImageObj, List<ImageTagObj>> ImageTags;

    public static void Start()
    {
        connStr = new SqliteConnectionStringBuilder("Data Source=" + DB)
        {
            Mode = SqliteOpenMode.ReadWriteCreate
        }.ToString();
        using var sql = new SqliteConnection(connStr);
        sql.Execute(@"create table if not exists image (
  `id` integer NOT NULL PRIMARY KEY AUTOINCREMENT,
  `local` text,
  `uuid` text
);");

        sql.Execute(@"create table if not exists imagetags (
  `id` integer NOT NULL PRIMARY KEY AUTOINCREMENT,
  `uuid` text,
  `tag_group` text,
  `tag_uuid` text
);");

        Directory.CreateDirectory(Local);

        Refresh();
    }

    public static Dictionary<ImageObj, List<ImageTagObj>> GetAllTags() 
    {
        using var sql = new SqliteConnection(connStr);
        var res = new Dictionary<ImageObj, List<ImageTagObj>>();
        foreach (var item in GetAllImage())
        {
            var list = sql.Query<ImageTagObj>("SELECT tag_group,tag_uuid FROM imagetags WHERE uuid=@uuid", new { item.uuid });
            res.Add(item, list.ToList());
        }

        return res;
    }

    public static List<ImageObj> GetImageFromTag(string group, string uuid) 
    {
        using var sql = new SqliteConnection(connStr);
        var list = sql.Query<ImageObj>(@"SELECT
	image.local 
FROM
	imagetags
	INNER JOIN image ON imagetags.uuid = image.uuid 
WHERE
	imagetags.tag_uuid = @tag_uuid 
	AND imagetags.tag_group = @tag_group 
GROUP BY
	local", new { tag_uuid = uuid, tag_group = group });

        return list.ToList();
    }

    public static List<ImageObj> GetAllImage() 
    {
        using var sql = new SqliteConnection(connStr);
        var list = sql.Query<ImageObj>("SELECT local,uuid FROM image");

        return list.ToList();
    }

    public static void Refresh()
    {
        var info = new DirectoryInfo(Local);
        var files = info.GetFiles();
        List<ImageObj> data = new();
        var data1 = GetAllImage();

        foreach (var item in files)
        {
            data.Add(new()
            {
                local = item.Name
            });
        }

        var res = CheckData(data, data1);
        foreach (var item in res)
        {
            if (item != null)
            {
                AddImage(item.local);
            }
        }

        res = CheckData(data1, data);
        foreach (var item in res)
        {
            if (item != null)
            {
                RemoveImage(item.uuid);
            }
        }

        ImageTags = GetAllTags();
    }

    /// <summary>
    /// 差异数据
    /// </summary>
    public static List<ImageObj> CheckData(List<ImageObj> data1, List<ImageObj> data2)
    {
        return (from now in data1
                join old in data2
                on now.local equals old.local into var2Grp
                from grp in var2Grp.DefaultIfEmpty()
                select new
                {
                    now,
                    old = grp
                })
                .Where(a => a.old == null)
                .Select(a => a.now)
                .ToList();
    }

    public static void AddImage(string local) 
    {
        using var sql = new SqliteConnection(connStr);
        string uuid;
        while (true)
        {
            uuid = ImageTagUtils.NewUUID();
            var res1 = sql.Query("SELECT `id` FROM image WHERE uuid=@uuid", new { uuid });
            if (res1.Any())
                continue;
            break;
        }

        sql.Execute("INSERT INTO image(`uuid`,`local`) " +
            "VALUES (@uuid,@local)",
            new { uuid, local });
    }

    public static void RemoveImage(string uuid) 
    {
        using var sql = new SqliteConnection(connStr);
        sql.Execute("DELETE FROM image WHERE uuid=@uuid", new { uuid });
        sql.Execute("DELETE FROM imagetags WHERE uuid=@uuid", new { uuid });
    }

    public static void SetImageTag(ImageObj now, List<TagObj> tags)
    {
        using var sql = new SqliteConnection(connStr);
        sql.Execute("DELETE FROM imagetags WHERE uuid=@uuid", new { now.uuid });
        foreach (var item in tags)
        {
            sql.Execute("INSERT INTO imagetags(`uuid`,`tag_group`,`tag_uuid`) " +
            "VALUES (@uuid,@tag_group,@tag_uuid)",
            new { now.uuid, tag_group = item.group, tag_uuid = item.uuid });
        }
    }

    internal static void RemoveImageTag(string uuid)
    {
        using var sql = new SqliteConnection(connStr);
        sql.Execute("DELETE FROM imagetags WHERE uuid=@uuid", new { uuid });
    }
}
