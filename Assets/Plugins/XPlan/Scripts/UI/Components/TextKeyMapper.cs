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

        public void Refresh(StringTable st)
        {
            text.text = st.GetStr(key);
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
        public void Refresh(StringTable st)
        {
            text.text = st.GetStr(key);
        }
    }

    public class TextKeyMapper : MonoBehaviour
    {
        [SerializeField] private List<TextMap> textMapper;
        [SerializeField] private List<TmpMap> tmpMapper;

        // Start is called before the first frame update
        private void Awake()
        {
            textMapper  = new List<TextMap>();
            tmpMapper   = new List<TmpMap>();

            Text[] textComponents = gameObject.GetComponentsInChildren<Text>(true);

            foreach (Text textComponent in textComponents)
            {
                if(textComponent == null || textComponent.text == "")
                {
                    continue;
                }

                textMapper.Add(new TextMap(textComponent, textComponent.text));
            }

            TextMeshProUGUI[] tmpTextComponents = gameObject.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (TextMeshProUGUI tmpText in tmpTextComponents)
            {
                if(tmpText == null || tmpText.text == "")
                {
                    continue;
                }

                tmpMapper.Add(new TmpMap(tmpText, tmpText.text));
            }
        }

        public void RefreshText(StringTable st)
        {
            textMapper.ForEach(e04 => e04.Refresh(st));
            tmpMapper.ForEach(e04 => e04.Refresh(st));
        }
    }
}
