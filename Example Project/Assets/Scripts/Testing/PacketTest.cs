using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Tobo.Net;

public class PacketTest : MonoBehaviour
{
    public bool contentTests;
    public bool reflectionTests;

    void Start()
    {
        if (contentTests)
        {
            ValueTest();
            ArrayTest();
            StringTest();
        }

        if (reflectionTests)
        {
            ReflectionTest();
        }

        Packet.Register<TestPacket>();
        TestPacket p = new TestPacket { val = 1 };
        Debug.Log("Sent P");
        //p.Send();
    }

    void ValueTest()
    {
        ByteBuffer buffer = ByteBuffer.Get();
        int intValue = int.MaxValue - 1;
        float floatValue = Mathf.PI;
        ushort ushortValue = ushort.MaxValue - 1;
        byte byteValue = 123;
        Vector3 vecValue = Random.onUnitSphere;

        buffer.Write(intValue);
        buffer.Write(floatValue);
        buffer.Write(ushortValue);
        buffer.Write(byteValue);
        buffer.Write(vecValue);

        Assert.AreEqual(intValue, buffer.Read<int>(), buffer.Dump());
        Assert.AreEqual(floatValue, buffer.Read<float>(), buffer.Dump());
        Assert.AreEqual(ushortValue, buffer.Read<ushort>(), buffer.Dump());
        Assert.AreEqual(byteValue, buffer.Read<byte>(), buffer.Dump());
        Assert.AreEqual(vecValue, buffer.Read<Vector3>(), buffer.Dump());
    }

    void ArrayTest()
    {
        ByteBuffer buffer = ByteBuffer.Get();
        int[] intValues = new int[] { int.MaxValue - 1, int.MaxValue - 2 };
        float[] floatValues = new float[] { Mathf.PI, 5000f };
        ushort[] ushortValues = new ushort[] { ushort.MaxValue - 1, 0 };
        byte[] byteValues = new byte[] { 123, 234 };
        int[] empty = new int[0];
        Vector3[] vecValues = new Vector3[] { Random.onUnitSphere, Random.onUnitSphere, Random.onUnitSphere };
        byte closer = 55;

        buffer.Write(intValues);
        buffer.Write(floatValues);
        buffer.Write(ushortValues, ArrayLength.Int);
        buffer.Write(byteValues, ArrayLength.Byte);
        buffer.Write(empty);
        buffer.Write(vecValues, ArrayLength.None);
        buffer.Write(closer);

        AssertArray(intValues, buffer.Read<int>(-1), buffer);
        AssertArray(floatValues, buffer.Read<float>(-1), buffer);
        AssertArray(ushortValues, buffer.Read<ushort>(-1), buffer);
        AssertArray(byteValues, buffer.Read<byte>(-1), buffer);
        AssertArray(null, buffer.Read<int>(-1), buffer);
        AssertArray(vecValues, buffer.Read<Vector3>(3), buffer);
        Assert.AreEqual(closer, buffer.Read<byte>(), buffer.Dump());
    }

    void StringTest()
    {
        ByteBuffer buffer = ByteBuffer.Get();

        int open = 1;
        string strOne = "Hello ";
        int mid = 2;
        string strTwo = "World!";
        int close = 3;

        buffer.Write(open);
        buffer.Write(strOne);
        buffer.Write(mid);
        buffer.Write(strTwo);
        buffer.Write(close);

        Assert.AreEqual(open, buffer.Read<int>(), buffer.Dump());
        Assert.AreEqual(strOne, buffer.Read(), buffer.Dump());
        Assert.AreEqual(mid, buffer.Read<int>(), buffer.Dump());
        Assert.AreEqual(strTwo, buffer.Read(), buffer.Dump());
        Assert.AreEqual(close, buffer.Read<int>(), buffer.Dump());
    }

    void AssertArray<T>(T[] expected, T[] values, ByteBuffer buf)
    {
        if (expected == null && values == null) return;
        if ((expected == null) != (values == null))
            throw new System.NullReferenceException($"One passed array was null: expected is null? {expected == null} - values is null? {values == null}");
        Assert.AreEqual(expected.Length, values.Length);
        for (int i = 0; i < values.Length; i++)
        {
            Assert.AreEqual(expected[i], values[i], buf.Dump());
        }
    }


    void ReflectionTest()
    {
        /*
        System.Type[] types = ReflectionHelper.GetPackets();

        Debug.Log(types.Length + " types");

        foreach (var item in types)
        {
            Debug.Log(item.Name);
        }
        */
    }
}

public class TestPacket : Packet
{
    public int val;

    public override void Serialize(ByteBuffer buf)
    {
        buf.Write(val);
    }

    public override void Deserialize(ByteBuffer buf, Args args)
    {
        val = buf.Read<int>();
        Debug.Log(val);
    }
}

/*
public class TestPacket : IPacket
{
    public ushort ID => 2;
    public string Message { get; private set; }

    public TestPacket() { }


    public TestPacket(string message)
    {
        Message = message;
    }

    public void Send(ByteBuffer buf)
    {
        buf.Write(Message);
    }

    public void Handle(ByteBuffer buf)
    {
        Message = buf.Read();
    }
}

public class TestPacket2 : IServerPacket
{
    public ushort ID => 3;
    public string Message { get; private set; }

    public TestPacket2() { }

    public TestPacket2(string message)
    {
        Message = message;
    }

    public void Send(ByteBuffer buf)
    {
        buf.Write(Message);
    }

    public void Handle(ByteBuffer buf, Client c)
    {
        Message = buf.Read();
    }
}
*/
