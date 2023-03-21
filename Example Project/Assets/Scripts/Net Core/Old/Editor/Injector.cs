using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Cecil;
using Mono.Cecil.Cil;
//using UnityEditor.Compilation;
using UnityEditor;
using System.IO;
using System.Linq;
using System;
using System.Reflection;
using UnityEditor.Callbacks;

/*
public static class Injector
{
    public const string ToboNetworkHeader = "[Tobo Network] ";
    const string ToboNetworkFailureMessage = ToboNetworkHeader + "Failed post processing: ";
    const string AssemblyName = "Assembly-CSharp";
    //const string SessionStateKey = "TOBO_NETWORK_PROCESSED";


    [MenuItem("Tobo Networking/Generate IL")]
    //[PostProcessBuild(0)]
    public static void Run()
    {
        if (!Patchers.Validator.CanInject())
        {
            Debug.LogWarning(ToboNetworkHeader + "Cannot process currently, please fix any compilation errors or wait for compilation to complete!");
            return;
        }

        // Replace 'Assembly-CSharp-Editor' with just 'Assembly-CSharp'
        string pathToAssembly = Assembly.GetExecutingAssembly().Location.Replace("-Editor", "");

        EditorApplication.LockReloadAssemblies();
        bool unlocked = false;

        using (var assembly = AssemblyDefinition.ReadAssembly(pathToAssembly, new ReaderParameters { ReadWrite = true }))
        {
            if (Patch(assembly))
            {
                assembly.Write();

                unlocked = true;
                EditorApplication.UnlockReloadAssemblies();
                EditorUtility.RequestScriptReload();
            }
        }

        if (!unlocked)
            EditorApplication.UnlockReloadAssemblies();
    }

    [MenuItem("Tobo Networking/Reload Assemblies")]
    static void Reload()
    {
        EditorUtility.RequestScriptReload();
    }

    static bool ShouldProcess(AssemblyDefinition def)
    {
        return def.Name.Name == AssemblyName;
    }

    public static bool Patch(AssemblyDefinition assembly)
    {
        if (!ShouldProcess(assembly)) return false;

        string processedAttribName = assembly.MainModule.ImportReference(
                            typeof(PostProcessedAssemblyAttribute).GetConstructor(Type.EmptyTypes)).Name;

        if (assembly.HasCustomAttributes && assembly.CustomAttributes.Any(a => a.AttributeType.Name == processedAttribName))
        {
            Debug.Log(ToboNetworkHeader + "Already processed");
            return false;
        }
        else if (assembly.MainModule.HasCustomAttributes && assembly.MainModule.CustomAttributes.Any(a => a.AttributeType.Name == processedAttribName))
        {
            Debug.Log(ToboNetworkHeader + "Already processed");
            return false;
        }

        InjectorInstance injector = new InjectorInstance(assembly);
        injector.Run();

        return injector.modified;
        //if (injector.modified)
        //{
            /*
            var newType = new TypeDefinition("", "TestType", Mono.Cecil.TypeAttributes.Public
                | Mono.Cecil.TypeAttributes.AnsiClass
                | Mono.Cecil.TypeAttributes.AutoClass
                | Mono.Cecil.TypeAttributes.BeforeFieldInit);

            //newType.

            assembly.MainModule.Types.Add(newType);
            /
            //assembly.Write();

            //EditorApplication.UnlockReloadAssemblies();
            //EditorUtility.RequestScriptReload();

        //}
    }

    public static void Error(string message)
    {
        Debug.LogError(ToboNetworkFailureMessage + message);
    }

    class InjectorInstance
    {
        AssemblyDefinition assembly;
        ModuleDefinition module;

        bool serverSend;
        bool serverHandle;
        ServerSendMode serverMode;

        public bool modified { get; private set; }

        public InjectorInstance(AssemblyDefinition assembly)
        {
            this.assembly = assembly;
            this.module = assembly.MainModule;
        }


        

        public void Run()
        {
            const string IPacketType = "IPacket";

            // Used for logging, but also packet IDs
            int processedTypes = 0;

            TypeDefinition netManager = module.GetType("NetworkManager");
            if (netManager == null)
            {
                Error("Could not find NetworkManager type!");
                return;
            }

            var attributeConstructor =
                        module.ImportReference(
                            typeof(PostProcessedAssemblyAttribute).GetConstructor(Type.EmptyTypes));

            foreach (TypeDefinition type in module.Types)
            {
                if (type.HasInterfaces && type.Interfaces.Any(i => i.InterfaceType.Name == IPacketType))
                {
                    if (!Patchers.Validator.ValidDefinition(type, attributeConstructor.DeclaringType.Name)) // Logs error in method
                        return;

                    bool error = false;

                    // Check if packet is server->client (serverSide), or client->server
                    bool serverSide = Patchers.Side.IsServerPacket(type, ref error);
                    if (CheckErrorInline(error, "Server packet failure")) return;

                    Patchers.IDs.Patch(type, netManager, processedTypes + 1, ref error);
                    if (CheckErrorInline(error, "Send patch failure")) return;

                    Patchers.Send.Patch(type, serverSide, ref error);
                    if (CheckErrorInline(error, "Send patch failure")) return;

                    Patchers.Handle.Patch(type, serverSide, ref error);
                    if (CheckErrorInline(error, "Send patch failure")) return;

                    Patchers.Serialize.Patch(type, module, ref error);
                    if (CheckErrorInline(error, "Send patch failure")) return;

                    Patchers.Deserialize.Patch(type, ref error);
                    if (CheckErrorInline(error, "Send patch failure")) return;

                    type.CustomAttributes.Add(new CustomAttribute(attributeConstructor));
                    processedTypes++;
                }
            }

            if (processedTypes == 0)
                Debug.Log(ToboNetworkHeader + "Could not find any types to process.");
            else
            {
                Debug.Log(ToboNetworkHeader + $"Successfully processed {processedTypes} types.");

                assembly.CustomAttributes.Add(new CustomAttribute(attributeConstructor));
                module.CustomAttributes.Add(new CustomAttribute(attributeConstructor));
                modified = true;
            }
        }

        bool CheckErrorInline(bool error, string message)
        {
            // Just used to log a message
            if (error)
                Error(message);
            return error;
        }

        enum ServerSendMode
        {
            All,
            One,
            Except
        }
    }
}

static class DefinitionExtensions
{
    public static MethodDefinition GetMethodDef(this TypeDefinition type, string method) => type.Methods.FirstOrDefault(m => m.Name == method);
    public static FieldDefinition GetFieldDef(this TypeDefinition type, string field) => type.Fields.FirstOrDefault(f => f.Name == field);
}

[AttributeUsage(AttributeTargets.Module | AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct)]
public class PostProcessedAssemblyAttribute : Attribute { }
*/