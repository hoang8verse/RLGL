using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UIElements
{    
    public class ReadyScreen : Screen
    {        
        [SerializeField] TMPro.TextMeshProUGUI m_countDownText;

        private int m_time = 0;
        private float m_countDown = 0f;
        private bool m_isCountDown = false;
        private Action m_callbacks;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (m_isCountDown)
            {
                if (m_time >= 0)
                {
                    m_countDown += Time.deltaTime;
                    if (Mathf.CeilToInt(m_countDown) < m_time)
                    {
                        m_time--;
                        m_countDownText.text = m_time.ToString();
                    }
                }
                else
                {
                    m_isCountDown = false;
                    m_callbacks?.Invoke();
                    m_countDownText.gameObject.SetActive(false);                    
                }
            }
        }

        public void StartCountDown(int countDownTime, Action callback = null)
        {
            m_callbacks = callback;
            m_time = countDownTime;
            
            m_isCountDown = true;
            m_countDown = m_time;
            m_countDownText.text = m_time.ToString();
            m_countDownText.gameObject.SetActive(true);
        }
    }
}