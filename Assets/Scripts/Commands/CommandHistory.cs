using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CommandHistory : MonoBehaviour
{
    //public event EventHandler CommandHistoryChanged;        
    private List<Cmd> m_lstCommands = new List<Cmd>(); 
    private int iCmdIndex = -1;
    

    public CommandHistory()
    {
       
    }

    /// <summary>
    /// Stores a command on top of history, clearing any previous/undone commands
    /// </summary>
    /// <param name="command">Command to store</param>
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

    /// <summary>
    /// Unexecute last command
    /// </summary>
    public void Undo()
    {
        Undo(1);
    }

    /// <summary>
    /// Undo multiple commands
    /// </summary>
    /// <param name="numCommands">Number of commands to undo</param>
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

    /// <summary>
    /// Redo next command in history
    /// </summary>
    public void Redo()
    {
        Redo(1);
    }

    /// <summary>
    /// Redo multiple commands
    /// </summary>
    /// <param name="numCommands">Number of commands to redo</param>
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

    /// <summary>
    /// Clears command history without executing or unexecuting commands
    /// </summary>
    public void Clear()
    {
        iCmdIndex = -1;

        m_lstCommands.Clear();

//            if (CommandHistoryChanged != null)
//                CommandHistoryChanged(this, EventArgs.Empty);
    }


    /// <summary>
    /// List of stored commands
    /// </summary>
    public List<Cmd> CommandList
    {
        get { return m_lstCommands; }
    }

    /// <summary>
    /// Gets the description of the last command in history
    /// </summary>
    public string UndoDescription
    {
        get
        {
            if (m_lstCommands.Count == 0 || iCmdIndex > m_lstCommands.Count - 1)
                return "N/A";

            Cmd prevCommand = (Cmd)m_lstCommands[iCmdIndex];

            string undoText = prevCommand.ToString();

            return undoText;
        }
    }

    /// <summary>
    /// Gets the description of the next command in history
    /// </summary>
    public string RedoDescription
    {
        get {

            if (iCmdIndex <= 0)
                return "N/A";

            Cmd nextCommand = (Cmd)m_lstCommands[iCmdIndex - 1];

            string redoText = nextCommand.ToString();

            return redoText;
        }
    }

    /// <summary>
    /// Last command's index in CommandList
    /// </summary>
    public int CmdIndex
    {
        get { return iCmdIndex; }
    }

}
