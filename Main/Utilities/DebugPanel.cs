﻿using FistVR;
using Sodalite;
using Sodalite.Api;
using Sodalite.UiWidgets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace TNHTweaker.Utilities
{
    public class DebugPanel : MonoBehaviour
    {
        private static LockablePanel _instance;

        public static LockablePanel Instance
        {
            get 
            {
                if (_instance == null) _instance = new LockablePanel();
                return _instance;
            }
            set => _instance = value;
        }

        private void Awake()
        {
            ConfigurePanel();
        }


        //This cool method modified from sodalite repo: https://github.com/H3VR-Modding/Sodalite/blob/f48a271ec8c5168b68a16b26e78719487e2d0ec5/src/Sodalite/src/Sodalite.cs#L106=
        private void ConfigurePanel()
        {
            UiWidget.CreateAndConfigureWidget(GetMainCanvas(), (GridLayoutWidget widget) =>
            {
                ConfigureWidgetRect(widget.RectTransform);
                ConfigureWidgetLayoutGroup(widget.LayoutGroup);
                
                widget.AddChild((ButtonWidget button) => ConfigureButtonWidget(button, "Complete Hold Phase", OnCompletePhaseButtonPressed));
                widget.AddChild((ButtonWidget button) => ConfigureButtonWidget(button, "Teleport To Hold", OnTeleportToHoldButtonPressed));
            });
        }

        private void ConfigureButtonWidget(ButtonWidget button, string text, EventHandler<ButtonClickEventArgs> onClick)
        {
            button.ButtonText.rectTransform.localRotation = Quaternion.identity;
            button.ButtonText.text = text;
            button.Pointable.ColorSelected = Color.white;
            button.Pointable.ButtonClicked += onClick;
        }

        private GameObject GetMainCanvas()
        {
            return transform.Find("OptionsCanvas_0_Main/Canvas").gameObject;
        }

        private void ConfigureWidgetRect(RectTransform rect)
        {
            rect.localScale = new Vector3(0.07f, 0.07f, 0.07f);
            rect.localPosition = Vector3.zero;
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(37f / 0.07f, 24f / 0.07f);
            rect.pivot = new Vector2(0.5f, 1f);
        }

        private void ConfigureWidgetLayoutGroup(GridLayoutGroup layoutGroup)
        {
            layoutGroup.cellSize = new Vector2(171, 50);
            layoutGroup.spacing = Vector2.one * 4;
            layoutGroup.startCorner = GridLayoutGroup.Corner.UpperLeft;
            layoutGroup.startAxis = GridLayoutGroup.Axis.Horizontal;
            layoutGroup.childAlignment = TextAnchor.UpperCenter;
            layoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layoutGroup.constraintCount = 3;
        }


        private void OnCompletePhaseButtonPressed(object sender, ButtonClickEventArgs args)
        {
            if (GM.TNH_Manager == null) return;
            TNHTweakerLogger.Log("Completing hold phase via DebugPanel!", TNHTweakerLogger.LogType.General);

            TNH_HoldPoint currentHoldPoint = GM.TNH_Manager.m_curHoldPoint;
            if (currentHoldPoint.m_systemNode == null ||
                currentHoldPoint.m_systemNode.m_mode == TNH_HoldPointSystemNode.SystemNodeMode.Passive) return;

            foreach(TNH_EncryptionTarget target in currentHoldPoint.m_activeTargets)
            {
                Destroy(target);
            }
            currentHoldPoint.m_activeTargets.Clear();

            currentHoldPoint.CompletePhase();
        }

        private void OnTeleportToHoldButtonPressed(object sender, ButtonClickEventArgs args)
        {
            if (GM.TNH_Manager == null) return;
            TNHTweakerLogger.Log("Teleporting player to hold via DebugPanel!", TNHTweakerLogger.LogType.General);

            TNH_HoldPoint currentHoldPoint = GM.TNH_Manager.m_curHoldPoint;
            if (currentHoldPoint.m_systemNode == null) return;

            GM.CurrentMovementManager.TeleportToPoint(currentHoldPoint.m_systemNode.transform.position, true, Vector3.forward);
        }


        public static void AddWristMenuButton()
        {
            WristMenuAPI.Buttons.Add(new WristMenuButton("Spawn TNH Debug Panel", SpawnDebugPanel));
        }

        private static void SpawnDebugPanel(object sender, ButtonClickEventArgs args)
        {
            FVRWristMenu wristMenu = WristMenuAPI.Instance;

            GameObject panel = Instance.GetOrCreatePanel();
            wristMenu.m_currentHand.RetrieveObject(panel.GetComponent<FVRPhysicalObject>());

            if(panel.GetComponent<DebugPanel>() == null)
            {
                panel.AddComponent<DebugPanel>();
            }
        }
    }
}