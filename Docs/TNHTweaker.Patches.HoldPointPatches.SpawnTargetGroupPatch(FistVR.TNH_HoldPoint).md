#### [TakeAndHoldTweaker](index.md 'index')
### [TNHTweaker.Patches](TNHTweaker.Patches.md 'TNHTweaker.Patches').[HoldPointPatches](TNHTweaker.Patches.HoldPointPatches.md 'TNHTweaker.Patches.HoldPointPatches')

## HoldPointPatches.SpawnTargetGroupPatch(TNH_HoldPoint) Method

Replaces entire call that spawns in encryptions with our own <br/><br/>  
Related Features: <br/>  
- [ Allow for min and max encryptions to be set for limited ammo mode ](https://github.com/devyndamonster/TakeAndHoldTweaker/issues/99 'https://github.com/devyndamonster/TakeAndHoldTweaker/issues/99')<br/>  
- [ Allow for mixed encryption types to spawn in during hold phases ](https://github.com/devyndamonster/TakeAndHoldTweaker/issues/100 'https://github.com/devyndamonster/TakeAndHoldTweaker/issues/100')<br/>

```csharp
public static bool SpawnTargetGroupPatch(FistVR.TNH_HoldPoint __instance);
```
#### Parameters

<a name='TNHTweaker.Patches.HoldPointPatches.SpawnTargetGroupPatch(FistVR.TNH_HoldPoint).__instance'></a>

`__instance` [FistVR.TNH_HoldPoint](https://docs.microsoft.com/en-us/dotnet/api/FistVR.TNH_HoldPoint 'FistVR.TNH_HoldPoint')

#### Returns
[System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')