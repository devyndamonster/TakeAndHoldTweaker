#### [TakeAndHoldTweaker](index.md 'index')
### [TNHTweaker.Patches](TNHTweaker.Patches.md 'TNHTweaker.Patches').[SupplyPointPatches](TNHTweaker.Patches.SupplyPointPatches.md 'TNHTweaker.Patches.SupplyPointPatches')

## SupplyPointPatches.SpawnTakeEnemyGroupPatch(ILContext, MethodBase) Method

Patches SpawnTakeEnemyGroup method to use the SupplyChallenges random sosig IDs <br/><br/>  
Related Features: <br/>  
- [ Allow multiple types of sosigs to spawn at supply points ](https://github.com/devyndamonster/TakeAndHoldTweaker/issues/109 'https://github.com/devyndamonster/TakeAndHoldTweaker/issues/109')<br/>

```csharp
public static void SpawnTakeEnemyGroupPatch(MonoMod.Cil.ILContext ctx, System.Reflection.MethodBase orig);
```
#### Parameters

<a name='TNHTweaker.Patches.SupplyPointPatches.SpawnTakeEnemyGroupPatch(MonoMod.Cil.ILContext,System.Reflection.MethodBase).ctx'></a>

`ctx` [MonoMod.Cil.ILContext](https://docs.microsoft.com/en-us/dotnet/api/MonoMod.Cil.ILContext 'MonoMod.Cil.ILContext')

<a name='TNHTweaker.Patches.SupplyPointPatches.SpawnTakeEnemyGroupPatch(MonoMod.Cil.ILContext,System.Reflection.MethodBase).orig'></a>

`orig` [System.Reflection.MethodBase](https://docs.microsoft.com/en-us/dotnet/api/System.Reflection.MethodBase 'System.Reflection.MethodBase')