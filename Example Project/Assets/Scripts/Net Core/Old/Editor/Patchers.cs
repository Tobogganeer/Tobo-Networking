using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UnityEditor;
using System.IO;
using System.Linq;
using System;
using System.Reflection;

// https://www.codersblock.org/blog//2014/06/integrating-monocecil-with-unity.html
// https://github.com/spectre1989/unity_cecil_example/blob/master/Assets/Editor/AssemblyPostProcessor.cs
// https://github.com/vis2k/Mirror/blob/master/Assets/Mirror/Editor/Weaver/EntryPoint/CompilationFinishedHook.cs
// https://github.com/vis2k/Mirror/blob/master/Assets/Mirror/Editor/Weaver/EntryPointILPostProcessor/ILPostProcessorHook.cs
// https://forum.unity.com/threads/solved-burst-and-mono-cecil.781148/
// https://www.reddit.com/r/csharp/comments/5qtpso/using_monocecil_in_c_with_unity/
// https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes?redirectedfrom=MSDN&view=net-6.0
// https://forum.unity.com/threads/how-does-unity-do-codegen-and-why-cant-i-do-it-myself.853867/

/*
public static class Patchers
{
    public static class Validator
    {
        public static bool CanInject()
        {
            if (EditorApplication.isCompiling)
                return false;

            Assembly assembly = Assembly.GetAssembly(typeof(SceneView));
            Type logEntries = assembly.GetType("UnityEditor.LogEntries");
            logEntries.GetMethod("Clear").Invoke(new object(), null);

            int count = (int)logEntries.GetMethod("GetCount").Invoke(new object(), null);

            if (count > 0)
                return false;
            return true;
        }

        public static bool ValidDefinition(TypeDefinition type, string processedAttribName)
        {
            if (!(type.IsClass || type.IsValueType))
            {
                Injector.Error($"Packet {type.Name} must be a class or struct!");
                return false;
            }

            MethodDefinition handle = type.GetMethodDef("Handle");
            if (handle == null)
            {
                Injector.Error($"Packet {type.Name} must have a handle method!");
                return false;
            }
            if (handle.Parameters.Count > 2 || handle.Parameters.Count == 0)
            {
                Injector.Error($"Packet {type.Name} must have a handle method with 1 or 2 parameters!");
                return false;
            }
            if (!handle.IsStatic)
            {
                Injector.Error($"Packet {type.Name} must have a static handle method!");
                return false;
            }

            if (type.HasCustomAttributes && type.CustomAttributes.Any(type => type.AttributeType.Name == processedAttribName))
            {
                Debug.Log(Injector.ToboNetworkHeader + "Already processed");
                return false;
            }

            return true;
        }
    }

    public static class Side
    {
        public static bool IsServerPacket(TypeDefinition type, ref bool error)
        {
            bool serverSend = type.GetMethodDef("SendTo") != null || type.GetMethodDef("SendToAll") != null;
            bool send = type.GetMethodDef("Send") != null;
            MethodDefinition handle = type.GetMethodDef("Handle");

            bool serverHandle = handle.Parameters.Count == 2;

            #region Error Handling
            if (send && serverHandle)
            {
                Injector.Error($"Packet {type} has client Send() but server Handle()!");
                error = true;
                return false;
            }

            if (serverSend && !serverHandle)
            {
                Injector.Error($"Packet {type} has server Send() but client Handle()!");
                error = true;
                return false;
            }

            if (!serverHandle && !send)
            {
                Injector.Error($"Packet {type} has client Handle() but no client Send() method!");
                error = true;
                return false;
            }

            if (serverHandle && !serverSend)
            {
                Injector.Error($"Packet {type} has server Handle() but no server Send() method!");
                error = true;
                return false;
            }
            #endregion

            if (serverSend && serverHandle) return true;
            if (send && !serverHandle) return false;

            Injector.Error($"Could not determine packet sidedness.");
            error = true;
            return false;
        }
    }

    public static class IDs
    {
        public static void Patch(TypeDefinition type, TypeDefinition networkManagerType, int packetNumber, ref bool error)
        {
            string fieldName = type.Name + "_id";

            if (type.GetFieldDef(fieldName) != null)
            {
                Injector.Error($"Packet {type} already has a field named {fieldName}!");
                error = true;
                return;
            }
        }
    }

    public static class Send
    {
        // Check if packet is client or server
        // Call Serialize method
        // Send packet via appropriate NetworkManager method
        // Check for any errors along the way

        public static void Patch(TypeDefinition type, bool isServer, ref bool error)
        {
            
        }
    }

    public static class Handle
    {
        // Check if packet is client or server
        // Call Serialize method
        // Send packet via appropriate NetworkManager method
        // Check for any errors along the way

        public static void Patch(TypeDefinition type, bool isServer, ref bool error)
        {

        }
    }

    public static class Serialize
    {
        // Check if existing serialize method exists
        // If so ensure a deserialize method exists
        // For auto serialize, make sure all fields are unmanaged or have the IBufferStruct interface
        // Serialize in order
        // Make sure to add appropriate packet id
        // Check for any errors along the way

        public static void Patch(TypeDefinition type, ModuleDefinition module, ref bool error)
        {

        }
    }

    public static class Deserialize
    {
        public static void Patch(TypeDefinition type, ref bool error)
        {

        }
    }
}
*/