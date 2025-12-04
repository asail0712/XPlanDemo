using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using XPlan.Utility;

namespace XPlan.UI.Components
{
    [Serializable]
    public class TextMap
    {
        [SerializeField] public Text text;
        [SerializeField] public string key;

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
        [SerializeField] public TextMeshProUGUI text;
        [SerializeField] public string key;
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

        // Start is called before the first frame update
        private void Awake()
        {
            Text[] textComponents = gameObject.GetComponentsInChildren<Text>(true);

            foreach (Text textComponent in textComponents)
            {
                if (textComponent == null)
                {
                    continue;
                }

                if(!textComponent.text.StartsWith("key_", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                Register(textComponent, textComponent.text);
            }

            TextMeshProUGUI[] tmpTextComponents = gameObject.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (TextMeshProUGUI tmpText in tmpTextComponents)
            {
                if (tmpText == null)
                {
                    continue;
                }

                if (!tmpText.text.StartsWith("key_", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                Register(tmpText, tmpText.text);
            }
        }

        public void Register(Text text, string key)
        {
            if (text == null || string.IsNullOrEmpty(key)) return;
            textMapper  ??= new List<TextMap>();

            TextMap tmp = textMapper.FirstOrDefault(e04 => e04.text == text);
            if(tmp == null)
            {
                tmp = new TextMap(text, key);
            }
            else
            {
                tmp.key = key;
            }

            tmp.Refresh();

            textMapper.AddUnique(tmp);
        }

        public void Register(TextMeshProUGUI text, string key)
        {
            if (text == null || string.IsNullOrEmpty(key)) return;
            tmpMapper   ??= new List<TmpMap>();

            TmpMap tmp = tmpMapper.FirstOrDefault(e04 => e04.text == text);
            if (tmp == null)
            {
                tmp = new TmpMap(text, key);
            }
            else
            {
                tmp.key = key;
            }

            tmp.Refresh();

            tmpMapper.AddUnique(tmp);
        }

        public void RefreshText()
        {
            textMapper?.ForEach(e04 => e04.Refresh());
            tmpMapper?.ForEach(e04 => e04.Refresh());
        }
    }
}
