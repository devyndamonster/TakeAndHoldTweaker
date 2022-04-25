using FistVR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TNHTweaker.Objects.CharacterData;
using UnityEngine;
using UnityEngine.UI;

namespace TNHTweaker.ObjectWrappers
{
    public class TNHMenuUIWrapper : MonoBehaviour
    {
        public static TNHMenuUIWrapper Instance;

        public int CurrentCharacterPage = 0;

        private TNH_UIManager baseUI;
        private Text statusText;
        private Text detailListText;
        private Text characterPageText;

        public void InitTNHUI(TNH_UIManager baseUI)
        {
            Instance = this;
            this.baseUI = baseUI;

            statusText = CreateStatusText();
            detailListText = CreateDetailListText();
            characterPageText = CreatePageNumberText();
            CreatePrevCharacterPageButton();
            CreateNextCharacterPageButton();
            ExpandCharacterUI();
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
            RefreshCharactersInCategory();
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
            int groupIndex = baseUI.Categories.FindIndex(o => o.CategoryName == character.Group);
            if (groupIndex > -1)
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
                category.CategoryName = character.Group;
                
                baseUI.Categories.Add(category);
                groupIndex = baseUI.Categories.FindIndex(o => o.CategoryName == character.Group);
                AddCategoryOption(category.CategoryName, groupIndex);
            }
        }

        private void AddCategoryOption(string categoryName, int categoryIndex)
        {
            baseUI.LBL_CategoryName[categoryIndex].gameObject.SetActive(true);
            baseUI.LBL_CategoryName[categoryIndex].text = (categoryIndex + 1).ToString() + ". " + categoryName;
        }


        public void DisplayNewCategory()
        {
            CurrentCharacterPage = 0;
            characterPageText.text = CurrentCharacterPage.ToString();
            RefreshCharactersInCategory();
        }

        public void RefreshCharactersInCategory()
        {
            int categoryIndex = baseUI.m_selectedCategory;
            int pageSize = baseUI.LBL_CharacterName.Count;

            for (int labelIndex = 0; labelIndex < pageSize; labelIndex++)
            {
                int characterIndex = labelIndex + (pageSize * CurrentCharacterPage);
                if (characterIndex < baseUI.Categories[categoryIndex].Characters.Count)
                {
                    baseUI.LBL_CharacterName[labelIndex].gameObject.SetActive(true);
                    TNH_CharacterDef def = baseUI.CharDatabase.GetDef(baseUI.Categories[categoryIndex].Characters[characterIndex]);
                    baseUI.LBL_CharacterName[labelIndex].text = (labelIndex + 1).ToString() + ". " + def.DisplayName;
                }
                else
                {
                    baseUI.LBL_CharacterName[labelIndex].gameObject.SetActive(false);
                }
            }
        }


        private Text CreateStatusText()
        {
            Text magazineCacheText = Instantiate(baseUI.SelectedCharacter_Title.gameObject, baseUI.SelectedCharacter_Title.transform.parent).GetComponent<Text>();
            magazineCacheText.transform.localPosition = new Vector3(0, 550, 0);
            magazineCacheText.transform.localScale = new Vector3(2, 2, 2);
            magazineCacheText.horizontalOverflow = HorizontalWrapMode.Overflow;
            magazineCacheText.text = "STATUS TEXT";

            return magazineCacheText;
        }


        private Text CreateDetailListText()
        {
            Text itemsText = Instantiate(baseUI.SelectedCharacter_Title.gameObject, baseUI.SelectedCharacter_Title.transform.parent).GetComponent<Text>();
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


        private void ExpandCharacterUI()
        {
            GameObject templateCharacterLabel = baseUI.LBL_CharacterName[1].gameObject;
            OptionsPanel_ButtonSet buttonSet = templateCharacterLabel.transform.parent.GetComponent<OptionsPanel_ButtonSet>();
            List<FVRPointableButton> buttonList = buttonSet.ButtonsInSet.ToList();

            int maxButtons = 10;
            
            for (int buttonIndex = buttonSet.ButtonsInSet.Length; buttonIndex < maxButtons; buttonIndex++)
            {
                Text newCharacterLabel = Instantiate(templateCharacterLabel, templateCharacterLabel.transform.parent).GetComponent<Text>();
                Button newButton = newCharacterLabel.gameObject.GetComponent<Button>();

                newButton.onClick = new Button.ButtonClickedEvent();
                newButton.onClick.AddListener(() => { baseUI.SetSelectedCharacter(buttonIndex); });
                newButton.onClick.AddListener(() => { buttonSet.SetSelectedButton(buttonIndex); });

                baseUI.LBL_CharacterName.Add(newCharacterLabel);
                buttonList.Add(newCharacterLabel.gameObject.GetComponent<FVRPointableButton>());
            }

            buttonSet.ButtonsInSet = buttonList.ToArray();

            TightenTNHCharacterButtons();
        }


        private void TightenTNHCharacterButtons()
        {
            float prevY = baseUI.LBL_CharacterName[0].transform.localPosition.y;
            for (int i = 1; i < baseUI.LBL_CharacterName.Count; i++)
            {
                prevY = prevY - 35f;
                baseUI.LBL_CharacterName[i].transform.localPosition = new Vector3(250, prevY, 0);
            }
        }

        private void CreatePrevCharacterPageButton()
        {
            GameObject templateCharacterLabel = baseUI.LBL_CharacterName[1].gameObject;
            Text pageButton = Instantiate(templateCharacterLabel, templateCharacterLabel.transform.parent).GetComponent<Text>();
            pageButton.alignment = TextAnchor.MiddleCenter;
            pageButton.transform.localPosition = new Vector3(50, -425, 0);
            pageButton.text = "Prev";
            Button buttonComp = pageButton.gameObject.GetComponent<Button>();
            pageButton.GetComponent<BoxCollider>().size = new Vector3(100, 45, 13);

            buttonComp.onClick = new Button.ButtonClickedEvent();
            buttonComp.onClick.AddListener(() => { PrevCharacterPage(); });
        }

        private void CreateNextCharacterPageButton()
        {
            GameObject templateCharacterLabel = baseUI.LBL_CharacterName[1].gameObject;
            Text pageButton = Instantiate(templateCharacterLabel, templateCharacterLabel.transform.parent).GetComponent<Text>();
            pageButton.alignment = TextAnchor.MiddleCenter;
            pageButton.transform.localPosition = new Vector3(350, -425, 0);
            pageButton.text = "Next";
            Button buttonComp = pageButton.gameObject.GetComponent<Button>();
            pageButton.GetComponent<BoxCollider>().size = new Vector3(100, 45, 13);

            buttonComp.onClick = new Button.ButtonClickedEvent();
            buttonComp.onClick.AddListener(() => { NextCharacterPage(); });
        }

        private Text CreatePageNumberText()
        {
            GameObject templateCharacterLabel = baseUI.LBL_CharacterName[1].gameObject;
            Text pageText = Instantiate(templateCharacterLabel, templateCharacterLabel.transform.parent).GetComponent<Text>();
            pageText.transform.localPosition = new Vector3(400, -425, 0);
            pageText.text = "0";

            Destroy(pageText.GetComponent<Button>());
            Destroy(pageText.GetComponent<FVRPointableButton>());
            Destroy(pageText.GetComponent<BoxCollider>());

            return pageText;
        }

        private void PrevCharacterPage()
        {
            baseUI.PlayButtonSound(2);

            if (CurrentCharacterPage > 0)
            {
                CurrentCharacterPage -= 1;
                characterPageText.text = CurrentCharacterPage.ToString();
                RefreshCharactersInCategory();
            }
        }

        private void NextCharacterPage()
        {
            baseUI.PlayButtonSound(2);

            int pageSize = baseUI.LBL_CharacterName.Count;
            int characterCount = baseUI.Categories[baseUI.m_selectedCategory].Characters.Count;
            var totalPages = (int)Math.Ceiling((float)characterCount / pageSize);

            if (CurrentCharacterPage < totalPages - 1)
            {
                CurrentCharacterPage += 1;
                characterPageText.text = CurrentCharacterPage.ToString();
                RefreshCharactersInCategory();
            }
        }

        private void OnDestroy()
        {
            RemoveLoadingEvents();
            Instance = null;
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
