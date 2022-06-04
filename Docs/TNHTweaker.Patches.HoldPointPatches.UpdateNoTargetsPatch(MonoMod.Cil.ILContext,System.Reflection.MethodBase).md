#### [TakeAndHoldTweaker](index.md 'index')
### [TNHTweaker.Patches](TNHTweaker.Patches.md 'TNHTweaker.Patches').[HoldPointPatches](TNHTweaker.Patches.HoldPointPatches.md 'TNHTweaker.Patches.HoldPointPatches')

## HoldPointPatches.UpdateNoTargetsPatch(ILContext, MethodBase) Method

Patches update method to treat zero encryptions as "NoTargets" mode <br/><br/>  
Related Features: <br/>  
- [ Treat hold phases with no encryptions the same as 'NoTargets' mode ](https://github.com/devyndamonster/TakeAndHoldTweaker/issues/104 'https://github.com/devyndamonster/TakeAndHoldTweaker/issues/104')<br/>

```csharp
public static void UpdateNoTargetsPatch(MonoMod.Cil.ILContext ctx, System.Reflection.MethodBase orig);
```
#### Parameters

<a name='TNHTweaker.Patches.HoldPointPatches.UpdateNoTargetsPatch(MonoMod.Cil.ILContext,System.Reflection.MethodBase).ctx'></a>

`ctx` [MonoMod.Cil.ILContext](https://docs.microsoft.com/en-us/dotnet/api/MonoMod.Cil.ILContext 'MonoMod.Cil.ILContext')

<a name='TNHTweaker.Patches.HoldPointPatches.UpdateNoTargetsPatch(MonoMod.Cil.ILContext,System.Reflection.MethodBase).orig'></a>

`orig` [System.Reflection.MethodBase](https://docs.microsoft.com/en-us/dotnet/api/System.Reflection.MethodBase 'System.Reflection.MethodBase')