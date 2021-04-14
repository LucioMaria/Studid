using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Plugin.CloudFirestore;
using Plugin.CloudFirestore.Attributes;

namespace Studid.Models
{
    public class ExamModel : IComparable
{
    [MapTo("examId")]
    public string examId { get; set; }

    [MapTo("examName")]
    public string examName { get; set; }

    [MapTo("cfu")]
    public int cfu { get; set; }

    [MapTo("date")]
    public Timestamp date { get; set; }

    public int CompareTo(object obj)
    {
        var em = obj as ExamModel;
        return examName.CompareTo(em.examName);
    }

    public override bool Equals(object obj)
    {
        if (obj is ExamModel)
        {
            ExamModel model = (ExamModel)obj;
            return this.examName.Equals(model.examName);
        }
        throw new Exception();
    }
}
}