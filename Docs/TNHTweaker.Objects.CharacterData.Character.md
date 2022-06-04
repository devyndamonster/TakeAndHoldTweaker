#### [TakeAndHoldTweaker](index.md 'index')
### [TNHTweaker.Objects.CharacterData](TNHTweaker.Objects.CharacterData.md 'TNHTweaker.Objects.CharacterData')

## Character Class

Custom character class

```csharp
public class Character : UnityEngine.ScriptableObject
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; [UnityEngine.Object](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Object 'UnityEngine.Object') &#129106; [UnityEngine.ScriptableObject](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.ScriptableObject 'UnityEngine.ScriptableObject') &#129106; Character

| Fields | |
| :--- | :--- |
| [CharacterID](TNHTweaker.Objects.CharacterData.Character.CharacterID.md 'TNHTweaker.Objects.CharacterData.Character.CharacterID') | A unique itentifier for character |
| [Description](TNHTweaker.Objects.CharacterData.Character.Description.md 'TNHTweaker.Objects.CharacterData.Character.Description') | Description text that will appear below the character in the TNH menu |
| [DisplayName](TNHTweaker.Objects.CharacterData.Character.DisplayName.md 'TNHTweaker.Objects.CharacterData.Character.DisplayName') | Name of character that is displayed in TNH Menu |
| [EquipmentPools](TNHTweaker.Objects.CharacterData.Character.EquipmentPools.md 'TNHTweaker.Objects.CharacterData.Character.EquipmentPools') | A collection of EquipmentPools that can appear in the object constructor |
| [ForceAllAgentWeapons](TNHTweaker.Objects.CharacterData.Character.ForceAllAgentWeapons.md 'TNHTweaker.Objects.CharacterData.Character.ForceAllAgentWeapons') | When true, sosigs will always spawn with all of there equipment |
| [Group](TNHTweaker.Objects.CharacterData.Character.Group.md 'TNHTweaker.Objects.CharacterData.Character.Group') | The character category that the character will appear in. If a unique group name is entered here, a new category will be created |
| [Has_Item_Primary](TNHTweaker.Objects.CharacterData.Character.Has_Item_Primary.md 'TNHTweaker.Objects.CharacterData.Character.Has_Item_Primary') | When true, a primary item will spawn at the start of TNH |
| [Has_Item_Secondary](TNHTweaker.Objects.CharacterData.Character.Has_Item_Secondary.md 'TNHTweaker.Objects.CharacterData.Character.Has_Item_Secondary') | When true, a secondar item will spawn at the start of TNH |
| [Has_Item_Shield](TNHTweaker.Objects.CharacterData.Character.Has_Item_Shield.md 'TNHTweaker.Objects.CharacterData.Character.Has_Item_Shield') | When true, a shield will spawn at the start of TNH |
| [Has_Item_Tertiary](TNHTweaker.Objects.CharacterData.Character.Has_Item_Tertiary.md 'TNHTweaker.Objects.CharacterData.Character.Has_Item_Tertiary') | When true, a tertiary item will spawn at the start of TNH |
| [Has_Weapon_Primary](TNHTweaker.Objects.CharacterData.Character.Has_Weapon_Primary.md 'TNHTweaker.Objects.CharacterData.Character.Has_Weapon_Primary') | When true, a primary weapon will spawn at the start of TNH |
| [Has_Weapon_Secondary](TNHTweaker.Objects.CharacterData.Character.Has_Weapon_Secondary.md 'TNHTweaker.Objects.CharacterData.Character.Has_Weapon_Secondary') | When true, a secondary weapon will spawn at the start of TNH |
| [Has_Weapon_Tertiary](TNHTweaker.Objects.CharacterData.Character.Has_Weapon_Tertiary.md 'TNHTweaker.Objects.CharacterData.Character.Has_Weapon_Tertiary') | When true, a tertiary weapon will spawn at the start of TNH |
| [Item_Primary](TNHTweaker.Objects.CharacterData.Character.Item_Primary.md 'TNHTweaker.Objects.CharacterData.Character.Item_Primary') | A pool of possible starting primary items |
| [Item_Secondary](TNHTweaker.Objects.CharacterData.Character.Item_Secondary.md 'TNHTweaker.Objects.CharacterData.Character.Item_Secondary') | A pool of possible starting secondary items |
| [Item_Shield](TNHTweaker.Objects.CharacterData.Character.Item_Shield.md 'TNHTweaker.Objects.CharacterData.Character.Item_Shield') | A pool of possible starting shields |
| [Item_Tertiary](TNHTweaker.Objects.CharacterData.Character.Item_Tertiary.md 'TNHTweaker.Objects.CharacterData.Character.Item_Tertiary') | A pool of possible starting tertiary items |
| [Picture](TNHTweaker.Objects.CharacterData.Character.Picture.md 'TNHTweaker.Objects.CharacterData.Character.Picture') | The icon that gets displayed for the character in the TNH menu |
| [Progressions](TNHTweaker.Objects.CharacterData.Character.Progressions.md 'TNHTweaker.Objects.CharacterData.Character.Progressions') | A collection of progressions which the character could play through on standard length progression setting |
| [Progressions_Endless](TNHTweaker.Objects.CharacterData.Character.Progressions_Endless.md 'TNHTweaker.Objects.CharacterData.Character.Progressions_Endless') | A collection of progressions which the character could play through on endless length progression setting |
| [RequireSightTable](TNHTweaker.Objects.CharacterData.Character.RequireSightTable.md 'TNHTweaker.Objects.CharacterData.Character.RequireSightTable') | A group of items from which a random item will be spawned from when an item that requires sights is purchased |
| [StartingTokens](TNHTweaker.Objects.CharacterData.Character.StartingTokens.md 'TNHTweaker.Objects.CharacterData.Character.StartingTokens') | Number of tokens given to the player that the start of the game |
| [TableID](TNHTweaker.Objects.CharacterData.Character.TableID.md 'TNHTweaker.Objects.CharacterData.Character.TableID') | Determines the scoreboard that this character will end up. Usually should be unique to the character |
| [UsesPurchasePriceIncrement](TNHTweaker.Objects.CharacterData.Character.UsesPurchasePriceIncrement.md 'TNHTweaker.Objects.CharacterData.Character.UsesPurchasePriceIncrement') | When true, purchasing items at an object constructor will increase the cost of the selected pool each time |
| [ValidAmmoEras](TNHTweaker.Objects.CharacterData.Character.ValidAmmoEras.md 'TNHTweaker.Objects.CharacterData.Character.ValidAmmoEras') | List of eras determining what type of ammo can spawn |
| [ValidAmmoSets](TNHTweaker.Objects.CharacterData.Character.ValidAmmoSets.md 'TNHTweaker.Objects.CharacterData.Character.ValidAmmoSets') | List of sets determining what type of ammo can spawn |
| [Weapon_Primary](TNHTweaker.Objects.CharacterData.Character.Weapon_Primary.md 'TNHTweaker.Objects.CharacterData.Character.Weapon_Primary') | A pool of possible starting primary weapons (spawns in large crate) |
| [Weapon_Secondary](TNHTweaker.Objects.CharacterData.Character.Weapon_Secondary.md 'TNHTweaker.Objects.CharacterData.Character.Weapon_Secondary') | A pool of possible starting secondary weapons (spawns in small crate) |
| [Weapon_Tertiary](TNHTweaker.Objects.CharacterData.Character.Weapon_Tertiary.md 'TNHTweaker.Objects.CharacterData.Character.Weapon_Tertiary') | A pool of possible starting tertiary weapons |
