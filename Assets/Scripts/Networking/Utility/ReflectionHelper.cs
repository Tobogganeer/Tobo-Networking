using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Linq;
using System;

namespace Tobo.Net
{
    public static class ReflectionHelper
    {
#if JUST_COMMENTING_THIS_OUT_LOL

    public static Type[] GetPackets()
    {
        return GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => ImplementsInterface(t, typeof(IPacket)) || ImplementsInterface(t, typeof(IServerPacket)))
            .ToArray();

        /*
        return GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => t.GetCustomAttributes(typeof(PacketAttribute), false).Length > 0)
            .ToArray();
        */

        // ImplementsInterface
    }

    public static bool ImplementsInterface(Type type, Type @interface)
    {
        /*
        if (type == null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        if (@interface == null)
        {
            throw new ArgumentNullException(nameof(@interface));
        }
        */

        var interfaces = type.GetInterfaces();
        if (@interface.IsGenericTypeDefinition)
        {
            foreach (var item in interfaces)
            {
                if (item.IsConstructedGenericType && item.GetGenericTypeDefinition() == @interface)
                {
                    return true;
                }
            }
        }
        else
        {
            foreach (var item in interfaces)
            {
                if (item == @interface)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public static List<Assembly> GetAssemblies()
    {
        string thisAssemblyName = Assembly.GetExecutingAssembly().GetName().FullName;

        return AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a
                .GetReferencedAssemblies()
                .Any(n => n.FullName == thisAssemblyName) || a.FullName == thisAssemblyName)
            .ToList();

        /*
        var assemblies = new List<Assembly>();

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.FullName.StartsWith("Mono.Cecil"))
                continue;

            if (assembly.FullName.StartsWith("UnityScript"))
                continue;

            if (assembly.FullName.StartsWith("Boo.Lan"))
                continue;

            if (assembly.FullName.StartsWith("System"))
                continue;

            if (assembly.FullName.StartsWith("I18N"))
                continue;

            if (assembly.FullName.StartsWith("UnityEngine"))
                continue;

            if (assembly.FullName.StartsWith("UnityEditor"))
                continue;

            if (assembly.FullName.StartsWith("mscorlib"))
                continue;

            assemblies.Add(assembly);
        }

        return assemblies;
        */
    }

    /*
    public void CreateMessageHandlersDictionary(byte messageHandlerGroupId)
    {
        MethodInfo[] methods = FindMessageHandlers();

        messageHandlers = new Dictionary<ushort, MessageHandler>(methods.Length);
        foreach (MethodInfo method in methods)
        {
            MessageHandlerAttribute attribute = method.GetCustomAttribute<MessageHandlerAttribute>();
            if (attribute.GroupId != messageHandlerGroupId)
                continue;

            if (!method.IsStatic)
                throw new NonStaticHandlerException(method.DeclaringType, method.Name);

            Delegate clientMessageHandler = Delegate.CreateDelegate(typeof(MessageHandler), method, false);
            if (clientMessageHandler != null)
            {
                // It's a message handler for Client instances
                if (messageHandlers.ContainsKey(attribute.MessageId))
                {
                    MethodInfo otherMethodWithId = messageHandlers[attribute.MessageId].GetMethodInfo();
                    throw new DuplicateHandlerException(attribute.MessageId, method, otherMethodWithId);
                }
                else
                    messageHandlers.Add(attribute.MessageId, (MessageHandler)clientMessageHandler);
            }
            else
            {
                // It's not a message handler for Client instances, but it might be one for Server instances
                Delegate serverMessageHandler = Delegate.CreateDelegate(typeof(Server.MessageHandler), method, false);
                if (serverMessageHandler == null)
                    throw new InvalidHandlerSignatureException(method.DeclaringType, method.Name);
            }
        }
    }
    */
#endif
    }
}