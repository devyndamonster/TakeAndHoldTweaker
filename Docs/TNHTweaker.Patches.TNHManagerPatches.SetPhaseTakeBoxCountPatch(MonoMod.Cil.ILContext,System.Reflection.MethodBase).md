#### [TakeAndHoldTweaker](index.md 'index')
### [TNHTweaker.Patches](TNHTweaker.Patches.md 'TNHTweaker.Patches').[TNHManagerPatches](TNHTweaker.Patches.TNHManagerPatches.md 'TNHTweaker.Patches.TNHManagerPatches')

## TNHManagerPatches.SetPhaseTakeBoxCountPatch(ILContext, MethodBase) Method

Patches SetPhase_Take method to use the SupplyChallenges min and max boxes values <br/><br/>  
Related Features: <br/>  
- [ Allow you to set min and max boxes spawned at supply points ](https://github.com/devyndamonster/TakeAndHoldTweaker/issues/106 'https://github.com/devyndamonster/TakeAndHoldTweaker/issues/106')<br/>

```csharp
public static void SetPhaseTakeBoxCountPatch(MonoMod.Cil.ILContext ctx, System.Reflection.MethodBase orig);
```
#### Parameters

<a name='TNHTweaker.Patches.TNHManagerPatches.SetPhaseTakeBoxCountPatch(MonoMod.Cil.ILContext,System.Reflection.MethodBase).ctx'></a>

`ctx` [MonoMod.Cil.ILContext](https://docs.microsoft.com/en-us/dotnet/api/MonoMod.Cil.ILContext 'MonoMod.Cil.ILContext')

<a name='TNHTweaker.Patches.TNHManagerPatches.SetPhaseTakeBoxCountPatch(MonoMod.Cil.ILContext,System.Reflection.MethodBase).orig'></a>

`orig` [System.Reflection.MethodBase](https://docs.microsoft.com/en-us/dotnet/api/System.Reflection.MethodBase 'System.Reflection.MethodBase')