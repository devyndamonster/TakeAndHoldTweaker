#### [TakeAndHoldTweaker](index.md 'index')
### [TNHTweaker.Patches](TNHTweaker.Patches.md 'TNHTweaker.Patches').[TNHManagerPatches](TNHTweaker.Patches.TNHManagerPatches.md 'TNHTweaker.Patches.TNHManagerPatches')

## TNHManagerPatches.GenerateValidPatrolPatch(ILContext, MethodBase) Method

Patches GenerateValidPatrol method to call our own GeneratePatrol method <br/><br/>  
Related Features: <br/>  
- [ Allow multiple types of sosigs to spawn in patrols ](https://github.com/devyndamonster/TakeAndHoldTweaker/issues/111 'https://github.com/devyndamonster/TakeAndHoldTweaker/issues/111')<br/>

```csharp
public static void GenerateValidPatrolPatch(MonoMod.Cil.ILContext ctx, System.Reflection.MethodBase orig);
```
#### Parameters

<a name='TNHTweaker.Patches.TNHManagerPatches.GenerateValidPatrolPatch(MonoMod.Cil.ILContext,System.Reflection.MethodBase).ctx'></a>

`ctx` [MonoMod.Cil.ILContext](https://docs.microsoft.com/en-us/dotnet/api/MonoMod.Cil.ILContext 'MonoMod.Cil.ILContext')

<a name='TNHTweaker.Patches.TNHManagerPatches.GenerateValidPatrolPatch(MonoMod.Cil.ILContext,System.Reflection.MethodBase).orig'></a>

`orig` [System.Reflection.MethodBase](https://docs.microsoft.com/en-us/dotnet/api/System.Reflection.MethodBase 'System.Reflection.MethodBase')