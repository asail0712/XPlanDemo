using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using XPlan.Utility;

namespace XPlan.UI.Components
{
    [Serializable]
    public class TextMap
    {
        [SerializeField] private Text text;
        [SerializeField] private string key;

        public TextMap(Text text, string key) 
        {
            this.text   = text;
            this.key    = key;
        }

        public void Refresh()
        {
            text.text = StringTable.Instance.GetStr(key);
        }
    }

    [Serializable]
    public class TmpMap
    {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private string key;
        public TmpMap(TextMeshProUGUI text, string key)
        {
            this.text   = text;
            this.key    = key;
        }
        public void Refresh()
        {
            text.text = StringTable.Instance.GetStr(key);
        }
    }

    public class I18NTextProvider : MonoBehaviour
    {
        [SerializeField] private List<TextMap> textMapper;
        [SerializeField] private List<TmpMap> tmpMapper;
        
        public void Register(Text text, string key)
        {
            if (text == null || string.IsNullOrEmpty(key)) return;

            textMapper  ??= new List<TextMap>();
            TextMap tmp = new TextMap(text, key);
            tmp.Refresh();

            textMapper.Add(tmp);
        }

        public void Register(TextMeshProUGUI text, string key)
        {
            if (text == null || string.IsNullOrEmpty(key)) return;
            tmpMapper   ??= new List<TmpMap>();
            TmpMap tmp  = new TmpMap(text, key);
            tmp.Refresh();

            tmpMapper.Add(tmp);
        }

        public void RefreshText()
        {
            textMapper?.ForEach(e04 => e04.Refresh());
            tmpMapper?.ForEach(e04 => e04.Refresh());
        }
    }
}
