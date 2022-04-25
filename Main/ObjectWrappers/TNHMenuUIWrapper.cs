using FistVR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TNHTweaker.Objects.CharacterData;
using UnityEngine;
using UnityEngine.UI;

namespace TNHTweaker.ObjectWrappers
{
    public class TNHMenuUIWrapper : MonoBehaviour
    {
        private TNH_UIManager baseUI;

        private Text statusText;
        private Text detailListText;

        public void InitTNHUI(TNH_UIManager baseUI)
        {
            this.baseUI = baseUI;

            statusText = CreateStatusText(baseUI);
            detailListText = CreateDetailListText(baseUI);
            ExpandCharacterUI(baseUI);
            SetupLoadingEvents();

            foreach(Character character in TNHTweaker.CustomCharacterDict.Values)
            {
                AddCharacterToUI(character);
            }

            StartCoroutine(WaitUntilLoaded());
        }


        private IEnumerator WaitUntilLoaded()
        {
            SceneLoader sceneHotDog = FindObjectOfType<SceneLoader>();
            sceneHotDog.gameObject.SetActive(false);

            yield return UpdateOtherloaderProgressText();
            yield return UpdateMagpatcherProgressText();
            SetProgressTextComplete();

            sceneHotDog.gameObject.SetActive(true);
        }


        private IEnumerator UpdateOtherloaderProgressText()
        {
            float progress = OtherLoader.LoaderStatus.GetLoaderProgress();
            while (progress < 1)
            {
                progress = OtherLoader.LoaderStatus.GetLoaderProgress();
                detailListText.text = GetLoadingItems();
                statusText.text = "LOADING ITEMS : " + (int)(progress * 100) + "%";
                yield return null;
            }
        }


        private IEnumerator UpdateMagpatcherProgressText()
        {
            float progress = MagazinePatcher.PatcherStatus.PatcherProgress;
            while (!MagazinePatcher.PatcherStatus.CachingFailed && progress < 1)
            {
                progress = MagazinePatcher.PatcherStatus.PatcherProgress;
                detailListText.text = MagazinePatcher.PatcherStatus.CacheLog;
                statusText.text = "CACHING ITEMS : " + (int)(progress * 100) + "%";
                yield return null;
            }

            if (MagazinePatcher.PatcherStatus.CachingFailed)
            {
                statusText.text = "CACHING FAILED! SEE ABOVE";
                throw new Exception("Magazine Caching Failed!");
            }
        }

        private void SetProgressTextComplete()
        {
            detailListText.text = "";
            statusText.text = "LOADING COMPLETE";
        }


        public static string GetLoadingItems()
        {
            List<string> loading = OtherLoader.LoaderStatus.LoadingItems;

            for (int i = 0; i < loading.Count; i++)
            {
                string colorHex = ColorUtility.ToHtmlStringRGBA(new Color(0.5f, 0.5f, 0.5f, Mathf.Clamp(((float)loading.Count - i) / loading.Count, 0, 1)));
                loading[i] = "<color=#" + colorHex + ">Loading Assets (" + loading[i].Split(':')[1].Trim() + ")</color>";
            }

            loading.Reverse();

            return string.Join("\n", loading.ToArray());
        }


        private void AddLoadedCharacter(Character character)
        {
            AddCharacterToUI(character);
            RefreshCharactersInCategory(baseUI.m_selectedCategory);
        }
        
        private void AddCharacterToUI(Character character)
        {
            TNH_CharacterDef baseCharacter = TNHTweaker.BaseCharacterDict[character];

            //Add to character DB
            if (!baseUI.CharDatabase.Characters.Contains(baseCharacter))
            {
                baseUI.CharDatabase.Characters.Add(baseCharacter);
            }

            //Add to UI lists
            int groupIndex = (int)character.Group;
            if (groupIndex < baseUI.Categories.Count())
            {
                if (!baseUI.Categories[groupIndex].Characters.Contains(character.CharacterID))
                {
                    baseUI.Categories[groupIndex].Characters.Add(character.CharacterID);
                }
            }
            else
            {
                TNH_UIManager.CharacterCategory category = new TNH_UIManager.CharacterCategory();
                category.Characters = new List<TNH_Char>();
                category.Characters.Add(character.CharacterID);
                category.CategoryName = "Custom Category";

                baseUI.Categories.Add(category);
                AddCategoryOption(category.CategoryName, groupIndex);
            }
        }

        private void AddCategoryOption(string categoryName, int categoryIndex)
        {
            baseUI.LBL_CategoryName[categoryIndex].gameObject.SetActive(true);
            baseUI.LBL_CategoryName[categoryIndex].text = (categoryIndex + 1).ToString() + ". " + categoryName;
        }

        private void RefreshCharactersInCategory(int categoryIndex)
        {
            for (int i = 0; i < baseUI.LBL_CharacterName.Count; i++)
            {
                if (i < baseUI.Categories[categoryIndex].Characters.Count)
                {
                    baseUI.LBL_CharacterName[i].gameObject.SetActive(true);
                    TNH_CharacterDef def = baseUI.CharDatabase.GetDef(baseUI.Categories[categoryIndex].Characters[i]);
                    baseUI.LBL_CharacterName[i].text = (i + 1).ToString() + ". " + def.DisplayName;
                }
                else
                {
                    baseUI.LBL_CharacterName[i].gameObject.SetActive(false);
                }
            }
        }


        private Text CreateStatusText(TNH_UIManager manager)
        {
            Text magazineCacheText = Instantiate(manager.SelectedCharacter_Title.gameObject, manager.SelectedCharacter_Title.transform.parent).GetComponent<Text>();
            magazineCacheText.transform.localPosition = new Vector3(0, 550, 0);
            magazineCacheText.transform.localScale = new Vector3(2, 2, 2);
            magazineCacheText.horizontalOverflow = HorizontalWrapMode.Overflow;
            magazineCacheText.text = "STATUS TEXT";

            return magazineCacheText;
        }


        private Text CreateDetailListText(TNH_UIManager manager)
        {
            Text itemsText = Instantiate(manager.SelectedCharacter_Title.gameObject, manager.SelectedCharacter_Title.transform.parent).GetComponent<Text>();
            itemsText.transform.localPosition = new Vector3(-30, 630, 0);
            itemsText.transform.localScale = new Vector3(1, 1, 1);
            itemsText.text = "";
            itemsText.supportRichText = true;
            itemsText.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            itemsText.alignment = TextAnchor.LowerLeft;
            itemsText.verticalOverflow = VerticalWrapMode.Overflow;
            itemsText.horizontalOverflow = HorizontalWrapMode.Overflow;

            return itemsText;
        }


        private void ExpandCharacterUI(TNH_UIManager manager)
        {
            GameObject templateCharacterLabel = manager.LBL_CharacterName[1].gameObject;
            OptionsPanel_ButtonSet buttonSet = templateCharacterLabel.transform.parent.GetComponent<OptionsPanel_ButtonSet>();
            List<FVRPointableButton> buttonList = buttonSet.ButtonsInSet.ToList();

            int maxButtons = 12;
            
            for (int buttonIndex = buttonSet.ButtonsInSet.Length; buttonIndex < maxButtons; buttonIndex++)
            {
                Text newCharacterLabel = Instantiate(templateCharacterLabel, templateCharacterLabel.transform.parent).GetComponent<Text>();
                Button newButton = newCharacterLabel.gameObject.GetComponent<Button>();

                newButton.onClick = new Button.ButtonClickedEvent();
                newButton.onClick.AddListener(() => { manager.SetSelectedCharacter(buttonIndex); });
                newButton.onClick.AddListener(() => { buttonSet.SetSelectedButton(buttonIndex); });

                manager.LBL_CharacterName.Add(newCharacterLabel);
                buttonList.Add(newCharacterLabel.gameObject.GetComponent<FVRPointableButton>());
            }

            buttonSet.ButtonsInSet = buttonList.ToArray();

            TightenTNHCharacterButtons(manager);
        }

        private void TightenTNHCharacterButtons(TNH_UIManager manager)
        {
            float prevY = manager.LBL_CharacterName[0].transform.localPosition.y;
            for (int i = 1; i < manager.LBL_CharacterName.Count; i++)
            {
                prevY = prevY - 35f;
                manager.LBL_CharacterName[i].transform.localPosition = new Vector3(250, prevY, 0);
            }
        }

        private void OnDestroy()
        {
            RemoveLoadingEvents();
        }

        private void SetupLoadingEvents()
        {
            CharacterLoader.OnCharacterLoaded += AddLoadedCharacter;
        }

        private void RemoveLoadingEvents()
        {
            CharacterLoader.OnCharacterLoaded -= AddLoadedCharacter;
        }


    }
}
