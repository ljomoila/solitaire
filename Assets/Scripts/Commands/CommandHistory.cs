using UnityEngine;
using System.Collections.Generic;

public class CommandHistory : MonoBehaviour
{
    //public event EventHandler CommandHistoryChanged;
    private List<Cmd> m_lstCommands = new List<Cmd>();
    private int iCmdIndex = -1;

    public CommandHistory() { }

    public void StoreCommand(Cmd command)
    {
        if (iCmdIndex > 0)
        {
            m_lstCommands.RemoveRange(0, iCmdIndex);
        }

        m_lstCommands.Insert(0, command);

        iCmdIndex = 0;

        NotificationCenter.DefaultCenter.PostNotification(this, "OnCommandHistoryChange");

        //            if (CommandHistoryChanged != null)
        //                CommandHistoryChanged(this, EventArgs.Empty);
    }

    public void Undo()
    {
        Undo(1);
    }

    public void Undo(int numCommands)
    {
        Cmd command = null;

        //Debug.Log("Undo cmdIndex "+iCmdIndex);

        do
        {
            if (m_lstCommands.Count == 0 || iCmdIndex > m_lstCommands.Count - 1)
                break;

            command = (Cmd)m_lstCommands[iCmdIndex];

            command.Unexecute();

            iCmdIndex++;
            numCommands--;
        } while (numCommands > 0);

        NotificationCenter.DefaultCenter.PostNotification(this, "OnCommandHistoryChange");

        //            if (CommandHistoryChanged != null)
        //                CommandHistoryChanged(command, EventArgs.Empty);
    }

    public void Redo()
    {
        Redo(1);
    }

    public void Redo(int numCommands)
    {
        Cmd command = null;

        do
        {
            if (iCmdIndex <= 0)
                break;

            iCmdIndex--;

            command = (Cmd)m_lstCommands[iCmdIndex];

            command.Execute();

            numCommands--;
        } while (numCommands > 0);

        NotificationCenter.DefaultCenter.PostNotification(this, "OnCommandHistoryChange");

        //            if (CommandHistoryChanged != null)
        //                CommandHistoryChanged(command, EventArgs.Empty);
    }

    public void Clear()
    {
        iCmdIndex = -1;

        m_lstCommands.Clear();

        //            if (CommandHistoryChanged != null)
        //                CommandHistoryChanged(this, EventArgs.Empty);
    }

    public List<Cmd> CommandList
    {
        get { return m_lstCommands; }
    }

    public static string NoCommandsStr = "N/A";

    public string UndoDescription
    {
        get
        {
            if (m_lstCommands.Count == 0 || iCmdIndex > m_lstCommands.Count - 1)
                return NoCommandsStr;

            Cmd prevCommand = (Cmd)m_lstCommands[iCmdIndex];

            string undoText = prevCommand.ToString();

            return undoText;
        }
    }

    public string RedoDescription
    {
        get
        {
            if (iCmdIndex <= 0)
                return NoCommandsStr;

            Cmd nextCommand = (Cmd)m_lstCommands[iCmdIndex - 1];

            string redoText = nextCommand.ToString();

            return redoText;
        }
    }

    public int CmdIndex
    {
        get { return iCmdIndex; }
    }
}
