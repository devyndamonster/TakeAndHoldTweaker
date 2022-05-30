### [TNHTweaker.Patches](TNHTweaker.Patches.md 'TNHTweaker.Patches')

## HoldPointPatches Class

```csharp
public class HoldPointPatches
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; HoldPointPatches

| Methods | |
| :--- | :--- |
| [SpawnTargetGroupPatch(TNH_HoldPoint)](TNHTweaker.Patches.HoldPointPatches.SpawnTargetGroupPatch(FistVR.TNH_HoldPoint).md 'TNHTweaker.Patches.HoldPointPatches.SpawnTargetGroupPatch(FistVR.TNH_HoldPoint)') | Replaces entire call that spawns in encryptions with our own <br/><br/><br/>Related Features: <br/><br/>- [ Allow for min and max encryptions to be set for limited ammo mode ](https://github.com/devyndamonster/TakeAndHoldTweaker/issues/99 'https://github.com/devyndamonster/TakeAndHoldTweaker/issues/99')<br/><br/>- [ Allow for mixed encryption types to spawn in during hold phases ](https://github.com/devyndamonster/TakeAndHoldTweaker/issues/100 'https://github.com/devyndamonster/TakeAndHoldTweaker/issues/100')<br/> |
| [SpawnWarpInMarkersPatch(ILContext, MethodBase)](TNHTweaker.Patches.HoldPointPatches.SpawnWarpInMarkersPatch(MonoMod.Cil.ILContext,System.Reflection.MethodBase).md 'TNHTweaker.Patches.HoldPointPatches.SpawnWarpInMarkersPatch(MonoMod.Cil.ILContext, System.Reflection.MethodBase)') | Overrides logic that sets the number of encryptions that will spawn during a hold <br/><br/><br/>Related Features: <br/><br/>- [ Allow for min and max encryptions to be set for limited ammo mode ](https://github.com/devyndamonster/TakeAndHoldTweaker/issues/99 'https://github.com/devyndamonster/TakeAndHoldTweaker/issues/99')<br/> |
