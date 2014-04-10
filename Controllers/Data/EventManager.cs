using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using DayPilot.Web.Mvc.Recurrence;


/// <summary>
/// Summary description for EventManager
/// </summary>
public class EventManager
{
    private Controller controller;
    private string key;

    public EventManager(Controller controller, string key)
    {
        this.controller = controller;
        this.key = key;

        if (this.controller.Session[key] == null)
        {
            if (key == "recurring")
            {
                this.controller.Session[key] = generateDataRecurring();
            }
            else
            {
                this.controller.Session[key] = generateData();
            }
        }
    }

    public DataTable Data
    {
        get { return (DataTable) controller.Session[key]; }
    }

    public DataTable FilteredData(DateTime start, DateTime end, string keyword)
    {
        string where = String.Format("NOT (([end] <= '{0:s}') OR ([start] >= '{1:s}')) and [text] like '%{2}%'", start, end, keyword);
        DataRow[] rows = Data.Select(where);
        DataTable filtered = Data.Clone();

        foreach (DataRow r in rows)
        {
            filtered.ImportRow(r);
        }

        return filtered;
    }

    public EventManager(Controller controller) : this(controller, "default")
    {
    }

    private DataTable generateData()
    {
        DataTable dt = new DataTable();
        dt.Columns.Add("id", typeof (string));
        dt.Columns.Add("text", typeof (string));
        dt.Columns.Add("start", typeof (DateTime));
        dt.Columns.Add("end", typeof (DateTime));
        dt.Columns.Add("resource", typeof (string));
        dt.Columns.Add("color", typeof (string));
        dt.Columns.Add("allday", typeof (bool));

        dt.PrimaryKey = new DataColumn[] {dt.Columns["id"]};

        DataRow dr;

        //dr = dt.NewRow();
        //dr["id"] = 1;
        //dr["start"] = Convert.ToDateTime("15:00");
        //dr["end"] = Convert.ToDateTime("15:00");
        //dr["text"] = "Event 1";
        //dr["resource"] = "A";
        //dt.Rows.Add(dr);

        //dr = dt.NewRow();
        //dr["id"] = 2;
        //dr["start"] = Convert.ToDateTime("16:00");
        //dr["end"] = Convert.ToDateTime("17:00");
        //dr["text"] = "Event 2";
        //dr["resource"] = "A";
        //dr["color"] = "green";
        //dt.Rows.Add(dr);

        //dr = dt.NewRow();
        //dr["id"] = 3;
        //dr["start"] = Convert.ToDateTime("14:15").AddDays(1);
        //dr["end"] = Convert.ToDateTime("18:45").AddDays(1);
        //dr["text"] = "Event 3";
        //dr["resource"] = "A";
        //dt.Rows.Add(dr);

        //dr = dt.NewRow();
        //dr["id"] = 4;
        //dr["start"] = Convert.ToDateTime("16:30");
        //dr["end"] = Convert.ToDateTime("17:30");
        //dr["text"] = "Sales Dept. Meeting Once Again";
        //dr["resource"] = "B";
        //dt.Rows.Add(dr);

        //dr = dt.NewRow();
        //dr["id"] = 5;
        //dr["start"] = Convert.ToDateTime("8:00");
        //dr["end"] = Convert.ToDateTime("9:00");
        //dr["text"] = "Event 4";
        //dr["resource"] = "B";
        //dt.Rows.Add(dr);

        //dr = dt.NewRow();
        //dr["id"] = 6;
        //dr["start"] = Convert.ToDateTime("14:00");
        //dr["end"] = Convert.ToDateTime("20:00");
        //dr["text"] = "Event 6";
        //dr["resource"] = "C";
        //dt.Rows.Add(dr);


        //dr = dt.NewRow();
        //dr["id"] = 7;
        //dr["start"] = Convert.ToDateTime("11:00");
        //dr["end"] = Convert.ToDateTime("13:14");
        //dr["text"] = "Unicode test: 公曆 (requires Unicode fonts on the client side)";
        //dr["color"] = "red";
        //dt.Rows.Add(dr);

        //dr = dt.NewRow();
        //dr["id"] = 8;
        //dr["start"] = Convert.ToDateTime("13:14").AddDays(-1);
        //dr["end"] = Convert.ToDateTime("14:05").AddDays(-1);
        //dr["text"] = "Event 8";
        //dr["resource"] = "C";
        //dt.Rows.Add(dr);


        //dr = dt.NewRow();
        //dr["id"] = 9;
        //dr["start"] = Convert.ToDateTime("13:14").AddDays(7);
        //dr["end"] = Convert.ToDateTime("14:05").AddDays(7);
        //dr["text"] = "Event 9";
        //dr["resource"] = "C";
        //dt.Rows.Add(dr);

        //dr = dt.NewRow();
        //dr["id"] = 10;
        //dr["start"] = Convert.ToDateTime("13:14").AddDays(-7);
        //dr["end"] = Convert.ToDateTime("14:05").AddDays(-7);
        //dr["text"] = "Event 10";
        //dt.Rows.Add(dr);

        //dr = dt.NewRow();
        //dr["id"] = 11;
        //dr["start"] = Convert.ToDateTime("00:00").AddDays(8);
        //dr["end"] = Convert.ToDateTime("00:00").AddDays(15);
        //dr["text"] = "Event 11";
        //dr["resource"] = "D";
        //dr["allday"] = true;
        //dt.Rows.Add(dr);

        //dr = dt.NewRow();
        //dr["id"] = 12;
        //dr["start"] = Convert.ToDateTime("00:00").AddDays(-2);
        //dr["end"] = Convert.ToDateTime("00:00").AddDays(-1);
        //dr["text"] = "Event 12";
        //dr["resource"] = "D";
        //dr["allday"] = true;
        //dt.Rows.Add(dr);

        //dr = dt.NewRow();
        //dr["id"] = 13;
        //dr["start"] = DateTime.Now.AddDays(-7);
        //dr["end"] = DateTime.Now.AddDays(14);
        //dr["text"] = "Event 13";
        //dr["resource"] = "B";
        //dr["allday"] = true;
        //dt.Rows.Add(dr);

        //dr = dt.NewRow();
        //dr["id"] = 14;
        //dr["start"] = Convert.ToDateTime("7:45:00");
        //dr["end"] = Convert.ToDateTime("8:30:00");
        //dr["text"] = "Event 14";
        //dr["resource"] = "D";
        //dt.Rows.Add(dr);

        //dr = dt.NewRow();
        //dr["id"] = 16;
        //dr["start"] = Convert.ToDateTime("8:30:00").AddDays(1);
        //dr["end"] = Convert.ToDateTime("9:00:00").AddDays(1);
        //dr["text"] = "Event 16";
        //dr["resource"] = "D";
        //dt.Rows.Add(dr);


        //dr = dt.NewRow();
        //dr["id"] = 17;
        //dr["start"] = Convert.ToDateTime("8:00:00").AddDays(1);
        //dr["end"] = Convert.ToDateTime("8:00:01").AddDays(1);
        //dr["text"] = "Event 17";
        //dr["resource"] = "D";
        //dt.Rows.Add(dr);

        return dt;
    }

    private DataTable generateDataRecurring()
    {
        DataTable dt = new DataTable();
        dt.Columns.Add("id", typeof (string));
        dt.Columns.Add("text", typeof (string));
        dt.Columns.Add("start", typeof (DateTime));
        dt.Columns.Add("end", typeof (DateTime));
        dt.Columns.Add("resource", typeof (string));
        dt.Columns.Add("color", typeof (string));
        dt.Columns.Add("allday", typeof (bool));
        dt.Columns.Add("recurrence", typeof (string));

        dt.PrimaryKey = new DataColumn[] {dt.Columns["id"]};

        DataRow dr;

        dr = dt.NewRow();
        dr["id"] = 1;
        dr["start"] = Convert.ToDateTime("10:00");
        dr["end"] = Convert.ToDateTime("11:30");
        dr["text"] = "Daily";
        dr["resource"] = "A";
        dr["recurrence"] = RecurrenceRule.FromDateTime(Convert.ToString(dr["id"]), Convert.ToDateTime(dr["start"])).Daily().Times(15).Encode();
        dt.Rows.Add(dr);

        dr = dt.NewRow();
        dr["id"] = 2;
        dr["start"] = Convert.ToDateTime("13:00");
        dr["end"] = Convert.ToDateTime("18:00");
        dr["text"] = "Weekly";
        dr["resource"] = "B";
        dr["color"] = "green";
        dr["recurrence"] = RecurrenceRule.FromDateTime(Convert.ToString(dr["id"]), Convert.ToDateTime(dr["start"])).Weekly().Times(5).Encode();
        dt.Rows.Add(dr);


        return dt;
    }

    public void EventEdit(string id, string name)
    {
        DataRow dr = Data.Rows.Find(id);
        if (dr != null)
        {
            dr["text"] = name;
            Data.AcceptChanges();
        }
    }

    public void EventMove(string id, DateTime start, DateTime end, string resource)
    {
        DataRow dr = Data.Rows.Find(id);
        if (dr != null)
        {
            dr["start"] = start;
            dr["end"] = end;
            dr["resource"] = resource;
            Data.AcceptChanges();
        }
    }

    public void EventMove(string id, DateTime start, DateTime end)
    {
        DataRow dr = Data.Rows.Find(id);
        if (dr != null)
        {
            dr["start"] = start;
            dr["end"] = end;
            Data.AcceptChanges();
        }
        else // external drag&drop
        {
            
        }
    }

    public Event Get(string id)
    {
        DataRow dr = Data.Rows.Find(id);
        if (dr == null)
        {
            //return new Event();
            return null;
        }
        return new Event()
                   {
                       Id = (string) dr["id"],
                       Text = (string) dr["text"]
                   };
    }
    internal void EventCreate(DateTime start, DateTime end, string text, string resource, string id)
    {
        DataRow dr = Data.NewRow();

        dr["id"] = id;
        dr["start"] = start;
        dr["end"] = end;
        dr["text"] = text;
        dr["resource"] = resource;

        Data.Rows.Add(dr);
        Data.AcceptChanges();
    }

    internal void EventCreate(DateTime start, DateTime end, string text, string resource)
    {
        EventCreate(start, end, text, resource, Guid.NewGuid().ToString());
    }

    public class Event
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }

    public void EventDelete(string id)
    {
        DataRow dr = Data.Rows.Find(id);
        if (dr != null)
        {
            Data.Rows.Remove(dr);
            Data.AcceptChanges();
        }
    }
}
