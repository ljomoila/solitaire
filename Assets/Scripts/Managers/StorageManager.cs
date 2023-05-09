using System.Xml;
using System.Xml.Linq;
using UnityEngine;

public class StorageManager
{
    private static string filename = "gameState.xml";

    private static string GetFilePath()
    {
        return Application.persistentDataPath + "/" + filename;
    }

    public static XDocument LoadStoredState()
    {
        string filePath = GetFilePath();

        if (System.IO.File.Exists(filePath))
        {
            return XDocument.Load(filePath);
        }

        return null;
    }

    public static void StoreState()
    {
        Game activeGame = GameManager.Instance.ActiveGame;

        XmlWriterSettings s = new XmlWriterSettings();
        s.Indent = true;
        s.NewLineOnAttributes = true;

        if (!activeGame.Piles.Contains(activeGame.stock))
            activeGame.Piles.Add(activeGame.stock);

        using (XmlWriter w = XmlWriter.Create(GetFilePath(), s))
        {
            w.WriteStartDocument();
            w.WriteStartElement("game");

            w.WriteElementString(
                "viewType",
                StateManager.Instance.activeState is Menu ? "menu" : "game"
            );
            w.WriteElementString("time", GameManager.Instance.gameTime.Time.ToString());
            w.WriteElementString("gameType", activeGame.gameType.ToString());
            w.WriteElementString("gameTime", GameManager.Instance.gameTime.Time.ToString());

            w.WriteStartElement("piles");

            foreach (CardPile p in activeGame.Piles)
            {
                w.WriteStartElement("pile");
                w.WriteElementString("type", p.Type.ToString());

                if (p.cards.Count > 0)
                {
                    w.WriteStartElement("cards");
                    foreach (Card c in p.cards)
                    {
                        w.WriteStartElement("card");
                        w.WriteElementString("suit", c.suit.ToString());
                        w.WriteElementString("number", c.number.ToString());
                        w.WriteElementString("turned", c.IsTurned().ToString());
                        w.WriteElementString(
                            "position",
                            XmlHelpers.ConvertVector3ToString(c.transform.localPosition)
                        );
                        w.WriteEndElement();
                    }
                    w.WriteEndElement();
                }

                w.WriteEndElement();
            }

            w.WriteEndElement();
            w.WriteEndElement();
            w.WriteEndDocument();
        }

        if (activeGame.Piles.Contains(activeGame.stock))
            activeGame.Piles.Remove(activeGame.stock);
    }
}
