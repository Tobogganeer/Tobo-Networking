using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tobo.Net;

public class LANTest : MonoBehaviour
{
    NetworkDiscovery netDiscovery;

    //public string beaconType = "appID";
    //public ushort beaconPort = 1234;

    private void Start()
    {
        netDiscovery = GetComponent<NetworkDiscovery>();
    }

    [ContextMenu("CreateServer")]
    void CreateServer()
    {
        netDiscovery.InitServer();
        
        //netDiscovery.RegisterResponseData("Game mode", "Deathmatch");

        
    }

    [ContextMenu("CreateClient")]
    void CreateClient()
    {
        netDiscovery.OnServerDiscovered += (NetworkDiscovery.DiscoveryInfo info) =>
        {
            Debug.Log($"FOUND THING: {info.ServerAddress}:{info.ServerPort}");
        };

        netDiscovery.Search();
    }

    private void OnDestroy()
    {
        netDiscovery.CloseServer();
    }

    /*
    Beacon beacon;
    Probe probe;

    private void Update()
    {
        probe?.Update(Time.deltaTime);
    }

    [ContextMenu("CreateBeacon")]
    void CreateBeacon()
    {
        beacon = new Beacon(beaconType, beaconPort);
        beacon.BeaconData = "New Beacon " + Dns.GetHostName();
        beacon.Start();
    }

    [ContextMenu("DestroyBeacon")]
    void DestroyBeacon()
    {
        beacon?.Stop();
    }

    [ContextMenu("CreateProbe")]
    void CreateProbe()
    {
        probe = new Probe(beaconType);
        // Event raised on separate thread
        probe.BeaconsUpdated += (beacons) => MainThread.Execute(() => {
        foreach (var beacon in beacons)
            {
                Debug.Log(beacon.Address + ": " + beacon.Data);
            }
        });

        probe.Start();
    }

    [ContextMenu("DestroyProbe")]
    void DestroyProbe()
    {
        probe?.Stop();
    }

    /*
    
    var beacon = new Beacon("myApp", 1234);
    beacon.BeaconData = "My Application Server on " + Dns.GetHostName();
    beacon.Start();

    // ...

    beacon.Stop();

    */

    /*
    
    var probe = new Probe("myApp");
    // Event is raised on separate thread so need synchronization
    probe.BeaconsUpdated += beacons => Dispatcher.BeginInvoke((Action)(() => {
        for (var beacon in beacons)
        {
            Console.WriteLine(beacon.Address + ": " + beacon.Data);
        }        
    }));

    probe.Start();

    // ...

probe.Stop();

    */
}
