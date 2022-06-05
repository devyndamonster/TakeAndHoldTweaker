#### [TakeAndHoldTweaker](index.md 'index')
### [TNHTweaker.Patches](TNHTweaker.Patches.md 'TNHTweaker.Patches')

## SupplyPointPatches Class

```csharp
public static class SupplyPointPatches
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; SupplyPointPatches

| Methods | |
| :--- | :--- |
| [ConfigureAtBeginningPatch(TNH_SupplyPoint, TNH_CharacterDef)](TNHTweaker.Patches.SupplyPointPatches.ConfigureAtBeginningPatch(FistVR.TNH_SupplyPoint,FistVR.TNH_CharacterDef).md 'TNHTweaker.Patches.SupplyPointPatches.ConfigureAtBeginningPatch(FistVR.TNH_SupplyPoint, FistVR.TNH_CharacterDef)') | Replaces entire call that configures the player beginning equipment when the game starts <br/><br/><br/>Related Features: <br/><br/>- [ Have starting equipment use our own EquipmentGroup loot system ](https://github.com/devyndamonster/TakeAndHoldTweaker/issues/101 'https://github.com/devyndamonster/TakeAndHoldTweaker/issues/101')<br/> |
| [SpawnBoxesReplacementPatch(TNH_SupplyPoint, int, int, bool)](TNHTweaker.Patches.SupplyPointPatches.SpawnBoxesReplacementPatch(FistVR.TNH_SupplyPoint,int,int,bool).md 'TNHTweaker.Patches.SupplyPointPatches.SpawnBoxesReplacementPatch(FistVR.TNH_SupplyPoint, int, int, bool)') | Replaces entire call that spawns supply point boxes <br/><br/><br/>Related Features: <br/><br/>- [ Allow you to set min and max boxes spawned at supply points ](https://github.com/devyndamonster/TakeAndHoldTweaker/issues/106 'https://github.com/devyndamonster/TakeAndHoldTweaker/issues/106')<br/><br/>- [ Allow you to set min and max tokens per supply point ](https://github.com/devyndamonster/TakeAndHoldTweaker/issues/107 'https://github.com/devyndamonster/TakeAndHoldTweaker/issues/107')<br/><br/>- [ Allow you to set min and max health drops per supply point ](https://github.com/devyndamonster/TakeAndHoldTweaker/issues/108 'https://github.com/devyndamonster/TakeAndHoldTweaker/issues/108')<br/> |
| [SpawnTakeEnemyGroupPatch(ILContext, MethodBase)](TNHTweaker.Patches.SupplyPointPatches.SpawnTakeEnemyGroupPatch(MonoMod.Cil.ILContext,System.Reflection.MethodBase).md 'TNHTweaker.Patches.SupplyPointPatches.SpawnTakeEnemyGroupPatch(MonoMod.Cil.ILContext, System.Reflection.MethodBase)') | Patches SpawnTakeEnemyGroup method to use the SupplyChallenges random sosig IDs <br/><br/><br/>Related Features: <br/><br/>- [ Allow multiple types of sosigs to spawn at supply points ](https://github.com/devyndamonster/TakeAndHoldTweaker/issues/109 'https://github.com/devyndamonster/TakeAndHoldTweaker/issues/109')<br/> |
