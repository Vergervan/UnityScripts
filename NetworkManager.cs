using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading;
using System.Net;
using BrainBlo.Debug;
using BrainBlo.Projects;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Threading.Tasks;
using System.Text;
using MessageType = BrainBlo.Projects.MessageType;
using BrainBlo.Network;

public struct Player
{
    public IPEndPoint ipend { get; set; }
    public GameObject Obj { get; set; }
}

public class NetworkManager : MonoBehaviour
{
    public UMSingleton sgltn;

    Thread thread;
    public static StartNetworkScript.MemberType memberType;
    public static Queue<Action> actions = new Queue<Action>();
    public GameObject cameraObj;
    public GameObject playerPrefab;
    public GameObject consoleObj;
    private List<Player> players = new List<Player>();
    private readonly object lockObj = new object();
    private bool isWorking = true;
    private Log log = Log.Initialize();
    void Start()
    {
        sgltn = UMSingleton.Initialize();
        log.OnGetLog += OnLogMessage;
        if (memberType == StartNetworkScript.MemberType.Server)
        {
            thread = new Thread(() => ServerListenMessage());
            cameraObj.transform.position = new Vector3(0, 30, -70);
            cameraObj.AddComponent<FreeMovement>();
            MouseScript ms = cameraObj.AddComponent<MouseScript>();
            ms.minimumVert = -90;
            ms.maximumVert = 90;
        }
        else if (memberType == StartNetworkScript.MemberType.Client)
        {
            thread = new Thread(() => ClientListenMessage());

            StartNetworkScript.um.Send(ToBinary(new BloMessage(MessageType.Connect, "First Network Test")));
        }
        else
        {
            cameraObj.transform.position = new Vector3(0, 30, -70);
            cameraObj.AddComponent<FreeMovement>();
            MouseScript ms = cameraObj.AddComponent<MouseScript>();
            ms.minimumVert = -90;
            ms.maximumVert = 90;
        }
        thread?.Start();
        log.Write("Start game as " + memberType.ToString());
        Debug.Log(memberType.ToString());
    }
    void Update()
    {
        if (actions.Count != 0) actions.Dequeue().Invoke();
    }
    
    private void ClientListenMessage()
    {
        while (isWorking)
        {
            byte[] message = StartNetworkScript.um.Receive();
            Debug.Log($"Получено {message.Length} байт от сервера");
        }
    }

    private void ServerListenMessage()
    {
        while (isWorking)
        {
            IPEndPoint ipend = new IPEndPoint(IPAddress.Any, StartNetworkScript.um.EndPoint.Port);
            byte[] message = StartNetworkScript.um.Receive(ref ipend);
            BloMessage bloMessage = DeserializeBinary<BloMessage>(message);

            if(!PlayerInList(ipend))
            {
                players.Add(new Player { ipend = ipend });
                Debug.Log("Добавлен новый пользователь: " + ipend.ToString());
            }
            Task.Run(() => ProcessMessage(bloMessage, ipend));
            Debug.Log($"Получено {message.Length} байт от {ipend.ToString()}");
        }
    }

    private void ProcessMessage(BloMessage bloMessage, IPEndPoint ipend)
    {
        Debug.Log(bloMessage.MessageType.ToString());
        switch (bloMessage.MessageType)
        {
            case MessageType.Debug:
                actions.Enqueue(() => {
                    consoleObj.GetComponent<ConsoleScript>().WriteText(ipend.ToString() + ": " + bloMessage.Message);
                });
                break;
            case MessageType.Connect:
                if (!PlayerInList(ipend))
                {
                    players.Add(new Player { ipend = ipend });
                    Debug.Log("Добавлен новый пользователь: " + ipend.ToString());
                }

                actions.Enqueue(() => {
                    consoleObj.GetComponent<ConsoleScript>().WriteText(ipend.ToString() + ": " + bloMessage.Message);
                });
                break;
            case MessageType.Disconnect:
                lock (lockObj)
                {
                    IEnumerable<Player> items = players.Where(item => item.ipend.ToString() == ipend.ToString());
                    foreach(var c in items)
                    {
                        players.Remove(c);
                    }
                }
                actions.Enqueue(() => {
                    consoleObj.GetComponent<ConsoleScript>().WriteText(ipend.ToString() + ": " + bloMessage.Message);
                });
                break;
        }
        
        Debug.Log("Вызван ProcessMessage");
        //MessageScript message = DeserializeBinary<MessageScript>(buffer);
        //switch (message.MessageType)
        //{
        //    case MessageType.Connect:
        //        log.WriteLine(message.Message);
        //        break;
        //}
    }

    private void OnLogMessage(string str)
    {
        actions.Enqueue(() => consoleObj.GetComponent<ConsoleScript>().WriteText(str));
    }

    private bool PlayerInList(IPEndPoint point)
    {
        lock (lockObj)
        {
            for(int i = 0; i < players.Count; i++)
            {
                if (point.ToString() == players[i].ipend.ToString())
                {
                    return true;
                }
                if(i == players.Count - 1)
                {
                    return false;
                }
            }
        }
        return false;
    }

    public byte[] ToBinary(object o)
    {
        BinaryFormatter bf = new BinaryFormatter();
        using (MemoryStream ms = new MemoryStream())
        {
            bf.Serialize(ms, o);
            return ms.ToArray();
        }
    }

    public T DeserializeBinary<T>(byte[] array)
    {
        BinaryFormatter bf = new BinaryFormatter();
        using (MemoryStream ms = new MemoryStream(array))
        {
            ms.Position = 0;
            return (T)bf.Deserialize(ms);
        }
    }

    ~NetworkManager()
    {
        StartNetworkScript.um.Send(ToBinary(new BloMessage(MessageType.Disconnect, "Disconnect")));
        Debug.Log("Called UdpMember dectructor");
        isWorking = false;
        thread.Abort();
        Debug.Log("Called manager destructor");
    }
}
