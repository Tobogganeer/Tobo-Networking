using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tobo.Net;

public class Player : MonoBehaviour
{
    public static Dictionary<ushort, Player> All = new Dictionary<ushort, Player>();

    public ushort ID { get; private set; }
    private string username;

    private void OnDestroy()
    {
        All.Remove(ID);
    }

    /*
    private void Move(Vector3 newPosition, Vector3 forward)
    {
        transform.position = newPosition;
        forward.y = 0;
        transform.forward = forward.normalized;
    }
    */

    /*
    internal static void Spawn(ushort id, string username, Vector3 position, bool shouldSendSpawn = false)
    {
        Player player;
        if (id == NetworkManager.Singleton.Client.Id)
            player = Instantiate(NetworkManager.Singleton.LocalPlayerPrefab, position, Quaternion.identity).GetComponent<Player>();
        else
            player = Instantiate(NetworkManager.Singleton.PlayerPrefab, position, Quaternion.identity).GetComponent<Player>();

        player.ID = id;
        player.username = username;
        player.name = $"Player {id} ({username})";

        All.Add(id, player);
        if (shouldSendSpawn)
            player.SendSpawn();
    }

    private void SendSpawn()
    {
        Message message = Message.Create(MessageSendMode.Reliable, MessageId.SpawnPlayer);
        message.AddUShort(ID);
        message.AddString(username);
        message.AddVector3(transform.position);
        NetworkManager.Singleton.Client.Send(message);
    }

    [MessageHandler((ushort)MessageId.SpawnPlayer)]
    private static void SpawnPlayer(Message message)
    {
        Spawn(message.GetUShort(), message.GetString(), message.GetVector3());
    }

    internal void SendSpawn(ushort newPlayerId)
    {
        Message message = Message.Create(MessageSendMode.Reliable, MessageId.SpawnPlayer);
        message.AddUShort(ID);
        message.AddString(username);
        message.AddVector3(transform.position);
        NetworkManager.Singleton.Server.Send(message, newPlayerId);
    }
    #endregion
    */
}
