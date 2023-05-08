using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

public sealed class XmlHelpers
{
    public static void WriteInt(XElement n, string elementName, int num)
    {
        XElement newElement = new XElement(elementName);
        newElement.Value = num.ToString();
        n.Add(newElement);
    }

    public static bool ReadInt(XElement n, string elementName, out int num)
    {
        bool readOk = false;
        XElement foundNode = n.Element(elementName);

        if (foundNode != null)
        {
            num = int.Parse(foundNode.Value);
            readOk = true;
        }
        else
        {
            num = -1;
        }

        return readOk;
    }

    public static void WriteTransform(XmlWriter w, Transform t)
    {
        w.WriteElementString("pos", ConvertVector3ToString(t.position));
        w.WriteElementString("rot", ConvertQuaternionToString(t.rotation));
        w.WriteElementString("scale", ConvertVector3ToString(t.localScale));
    }

    public static void ReadTransform(XElement n, Transform t)
    {
        t.position = ConvertStringToVector3(n.Element("pos").Value);
        t.rotation = ConvertStringToQuaternion(n.Element("rot").Value);
        t.localScale = ConvertStringToVector3(n.Element("scale").Value);
    }

    public static void WriteFloat(XElement n, string elementName, float num)
    {
        XElement newElement = new XElement(elementName);
        newElement.Value = num.ToString();
        n.Add(newElement);
    }

    public static bool ReadFloat(XElement n, string elementName, out float num)
    {
        bool readOk = false;
        XElement foundNode = n.Element(elementName);

        if (foundNode != null)
        {
            num = float.Parse(foundNode.Value);
            readOk = true;
        }
        else
        {
            num = -1;
        }

        return readOk;
    }

    public static bool ReadXFloat(XElement n, string elementName, out float num)
    {
        bool readOk = false;
        XElement foundNode = n.Element(elementName);

        if (foundNode != null)
        {
            num = float.Parse(foundNode.Value);
            readOk = true;
        }
        else
        {
            num = -1;
        }

        return readOk;
    }

    public static string ReadString(XElement n, string nodeName)
    {
        return n.Element(nodeName).Value;
    }

    public static void WriteString(XElement n, string nodeName, string str)
    {
        XElement newElement = new XElement(nodeName);
        newElement.Value = str;
        n.Add(newElement);
    }

    public static void WriteVector3(XElement n, string nodeName, Vector3 v)
    {
        XElement newElement = new XElement(nodeName);
        newElement.Value = ConvertVector3ToString(v);
        n.Add(newElement);
    }

    public static Vector3 ReadVector3(XElement n, string nodeName)
    {
        return ConvertStringToVector3(n.Element(nodeName).Value);
    }

    public static Vector3 ConvertStringToVector3(string s)
    {
        string[] splitted = s.Split(':');
        float[] v = new float[3];
        for (int i = 0; i < 3; i++)
        {
            v[i] = float.Parse(splitted[i]);
        }
        return new UnityEngine.Vector3(v[0], v[1], v[2]);
    }

    public static Quaternion ReadQuaternion(XElement n, string nodeName)
    {
        return ConvertStringToQuaternion(n.Element(nodeName).Value);
    }

    public static Quaternion ConvertStringToQuaternion(string s)
    {
        string[] splitted = s.Split(':');
        float[] v = new float[4];
        for (int i = 0; i < 4; i++)
        {
            v[i] = float.Parse(splitted[i]);
        }
        return new Quaternion(v[0], v[1], v[2], v[3]);
    }

    public static Color ConvertStringToColor(string s)
    {
        string[] splitted = s.Split(':');
        float[] v = new float[4];
        for (int i = 0; i < 4; i++)
        {
            v[i] = float.Parse(splitted[i]);
        }
        return new Color(v[0], v[1], v[2], v[3]);
    }

    public static string ConvertVector3ListToString(List<Vector3> v)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach (Vector3 vec in v)
        {
            sb.Append(ConvertVector3ToString(vec) + "_");
        }
        return sb.ToString();
    }

    public static List<Vector3> ConvertStringToVector3List(string s)
    {
        List<Vector3> vecs = new List<Vector3>();

        string[] splitted = s.Split('_');
        foreach (string part in splitted)
        {
            if (string.IsNullOrEmpty(part) == false)
            {
                vecs.Add(ConvertStringToVector3(part.Replace("_", "")));
            }
        }
        return vecs;
    }

    public static string ConvertVector3ToString(Vector3 v)
    {
        return v.x + ":" + v.y + ":" + v.z;
    }

    public static string ConvertColorToString(Color c)
    {
        return c.r + ":" + c.g + ":" + c.b + ":" + c.a;
    }

    public static void WriteQuaternion(XElement n, string nodeName, Quaternion q)
    {
        XElement newElement = new XElement(nodeName);
        newElement.Value = ConvertQuaternionToString(q);
        n.Add(newElement);
    }

    public static string ConvertQuaternionToString(Quaternion q)
    {
        return q.x + ":" + q.y + ":" + q.z + ":" + q.w;
    }
}
