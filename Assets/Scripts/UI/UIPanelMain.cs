using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPanelMain : MonoBehaviour, IMenu
{
    [SerializeField] private Button btnTimer;

    [SerializeField] private Button btnMoves;

    [SerializeField] private Toggle autoWin;
    [SerializeField] private Toggle autoLose;

    private UIMainManager m_mngr;

    private void Awake()
    {
        btnMoves.onClick.AddListener(OnClickMoves);
        btnTimer.onClick.AddListener(OnClickTimer);
        autoLose.onValueChanged.AddListener(autoLoseValueChanged);
        autoWin.onValueChanged.AddListener(autoWinValueChanged);
    }

    private void autoWinValueChanged(bool arg0)
    {
        if (arg0)
        {
            autoLose.isOn = false;
        }
        m_mngr.SetAuto(autoWin.isOn|| autoLose.isOn, true);
    }

    private void autoLoseValueChanged(bool arg0)
    {
        if (arg0)
        {
            autoWin.isOn = false;
        }
    m_mngr.SetAuto(autoWin.isOn|| autoLose.isOn, false);
    }

    private void OnDestroy()
    {
        if (btnMoves) btnMoves.onClick.RemoveAllListeners();
        if (btnTimer) btnTimer.onClick.RemoveAllListeners();
    }

    public void Setup(UIMainManager mngr)
    {
        m_mngr = mngr;
    }

    private void OnClickTimer()
    {
        m_mngr.LoadLevelTimer();
    }

    private void OnClickMoves()
    {
        m_mngr.LoadLevelMoves();
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }
}
