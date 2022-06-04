#### [TakeAndHoldTweaker](index.md 'index')
### [TNHTweaker.Patches](TNHTweaker.Patches.md 'TNHTweaker.Patches')

## HoldPointPatches Class

```csharp
public static class HoldPointPatches
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; HoldPointPatches

| Methods | |
| :--- | :--- |
| [BeginAnalyzingNoTargetsPatch(ILContext, MethodBase)](TNHTweaker.Patches.HoldPointPatches.BeginAnalyzingNoTargetsPatch(MonoMod.Cil.ILContext,System.Reflection.MethodBase).md 'TNHTweaker.Patches.HoldPointPatches.BeginAnalyzingNoTargetsPatch(MonoMod.Cil.ILContext, System.Reflection.MethodBase)') | Patches BeginAnalyzing method to treat zero encryptions as "NoTargets" mode <br/><br/><br/>Related Features: <br/><br/>- [ Treat hold phases with no encryptions the same as 'NoTargets' mode ](https://github.com/devyndamonster/TakeAndHoldTweaker/issues/104 'https://github.com/devyndamonster/TakeAndHoldTweaker/issues/104')<br/> |
| [SpawnTargetGroupPatch(TNH_HoldPoint)](TNHTweaker.Patches.HoldPointPatches.SpawnTargetGroupPatch(FistVR.TNH_HoldPoint).md 'TNHTweaker.Patches.HoldPointPatches.SpawnTargetGroupPatch(FistVR.TNH_HoldPoint)') | Replaces entire call that spawns in encryptions with our own <br/><br/><br/>Related Features: <br/><br/>- [ Allow for min and max encryptions to be set for limited ammo mode ](https://github.com/devyndamonster/TakeAndHoldTweaker/issues/99 'https://github.com/devyndamonster/TakeAndHoldTweaker/issues/99')<br/><br/>- [ Allow for mixed encryption types to spawn in during hold phases ](https://github.com/devyndamonster/TakeAndHoldTweaker/issues/100 'https://github.com/devyndamonster/TakeAndHoldTweaker/issues/100')<br/> |
| [SpawnWarpInMarkersPatch(ILContext, MethodBase)](TNHTweaker.Patches.HoldPointPatches.SpawnWarpInMarkersPatch(MonoMod.Cil.ILContext,System.Reflection.MethodBase).md 'TNHTweaker.Patches.HoldPointPatches.SpawnWarpInMarkersPatch(MonoMod.Cil.ILContext, System.Reflection.MethodBase)') | Overrides logic that sets the number of encryptions that will spawn during a hold <br/><br/><br/>Related Features: <br/><br/>- [ Allow for min and max encryptions to be set for limited ammo mode ](https://github.com/devyndamonster/TakeAndHoldTweaker/issues/99 'https://github.com/devyndamonster/TakeAndHoldTweaker/issues/99')<br/> |
| [UpdateNoTargetsPatch(ILContext, MethodBase)](TNHTweaker.Patches.HoldPointPatches.UpdateNoTargetsPatch(MonoMod.Cil.ILContext,System.Reflection.MethodBase).md 'TNHTweaker.Patches.HoldPointPatches.UpdateNoTargetsPatch(MonoMod.Cil.ILContext, System.Reflection.MethodBase)') | Patches update method to treat zero encryptions as "NoTargets" mode <br/><br/><br/>Related Features: <br/><br/>- [ Treat hold phases with no encryptions the same as 'NoTargets' mode ](https://github.com/devyndamonster/TakeAndHoldTweaker/issues/104 'https://github.com/devyndamonster/TakeAndHoldTweaker/issues/104')<br/> |
