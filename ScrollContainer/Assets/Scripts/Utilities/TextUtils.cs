/*
 * Description:             TextUtils.cs
 * Author:                  TONYTANG
 * Create Date:             2020/02/14
 */

using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// TextUtils.cs
/// 文本静态工具类
/// </summary>
public static class TextUtils
{
    /// <summary>
    /// 获取指定文本文字的Prefer大小
    /// Note:
    /// 具体文本的显示实际大小跟文本的对象上是否挂Content Size Filter和Layout相关组件有关，
    /// 这里只是单纯的预计算指定文本文字的Prefer大小，想获取真实文本大小请使用GetTextStringRealSize
    /// </summary>
    /// <param name="s"></param>
    /// <param name="text"></param>
    /// <returns></returns>
    public static Vector2 GetTextStringPreferSize(string s, Text text)
    {
        Vector2 size = Vector2.zero;
        var textpixelsperunit = text.pixelsPerUnit;
        var textgeneratingsetting = text.GetGenerationSettings(text.rectTransform.rect.size);
        size.x = text.cachedTextGeneratorForLayout.GetPreferredWidth(s, textgeneratingsetting) / textpixelsperunit;
        size.y = text.cachedTextGeneratorForLayout.GetPreferredHeight(s, textgeneratingsetting) / textpixelsperunit;
        //Debug.Log($"文本:{s}的预计算大小:{size.ToString()}");
        return size;
    }

    /// <summary>
    /// 获取指定文本文字的真实大小
    /// Note:
    /// 此方案并未考虑Layout Group的情况，所以未考虑Layout Element的flexibleWidth和flexibleHeight
    /// </summary>
    /// <param name="s"></param>
    /// <param name="text"></param>
    /// <returns></returns>
    public static Vector2 GetTextStringRealSize(string s, Text text)
    {
        var contentsizefilter = text.GetComponent<ContentSizeFitter>();
        var textrectsize = text.rectTransform.rect.size;
        Vector2 realsize = textrectsize;
        if (contentsizefilter != null)
        {
            var layoutelement = text.GetComponent<LayoutElement>();
            var prefersize = Vector2.zero;
            if (contentsizefilter.horizontalFit == ContentSizeFitter.FitMode.PreferredSize || contentsizefilter.verticalFit == ContentSizeFitter.FitMode.PreferredSize)
            {
                var textpixelsperunit = text.pixelsPerUnit;
                var textgeneratingsetting = text.GetGenerationSettings(textrectsize);
                prefersize.x = text.cachedTextGeneratorForLayout.GetPreferredWidth(s, textgeneratingsetting) / textpixelsperunit;
                prefersize.y = text.cachedTextGeneratorForLayout.GetPreferredHeight(s, textgeneratingsetting) / textpixelsperunit;
            }
            switch (contentsizefilter.horizontalFit)
            {
                case ContentSizeFitter.FitMode.Unconstrained:
                    realsize.x = textrectsize.x;
                    break;
                case ContentSizeFitter.FitMode.MinSize:
                    realsize.x = layoutelement != null && layoutelement.minWidth != -1 ? layoutelement.minWidth : text.minWidth;
                    break;
                case ContentSizeFitter.FitMode.PreferredSize:
                    realsize.x = layoutelement != null && layoutelement.preferredWidth != -1 ? layoutelement.preferredWidth : prefersize.x;
                    break;
            }
            switch (contentsizefilter.verticalFit)
            {
                case ContentSizeFitter.FitMode.Unconstrained:
                    realsize.y = textrectsize.y;
                    break;
                case ContentSizeFitter.FitMode.MinSize:
                    realsize.y = layoutelement != null && layoutelement.minHeight != -1 ? layoutelement.minHeight : text.minHeight;
                    break;
                case ContentSizeFitter.FitMode.PreferredSize:
                    realsize.y = layoutelement != null && layoutelement.preferredHeight != -1 ? layoutelement.preferredHeight : prefersize.y;
                    break;
            }
        }
        //Debug.Log($"文本:{s}的预计算大小:{realsize.ToString()}");
        return realsize;
    }

    /// <summary>
    /// 获取指定TMP文本文字的真实大小(如果文本内容大小小于TMP RectTransform大小，以RectTransform大小为准)
    /// Note:
    /// 此方案要求传入的TextMeshProUGUI必须是激活状态
    /// </summary>
    /// <param name="s"></param>
    /// <param name="tmp"></param>
    /// <returns></returns>
    public static Vector2 GetTMPStringRealSize(string s, TextMeshProUGUI tmp)
    {
        var realsize = Vector2.zero;
        var rect = tmp.GetComponent<RectTransform>();
        if (string.IsNullOrEmpty(s) == false)
        {
            tmp.text = s;
            // ForceMeshUpdate(true)对隐藏的TMP对象并不起作用
            tmp.ForceMeshUpdate(true);
            // Note:
            // GetPreferredValues()在特定情况下得不出正确值会出现(0,0)
            // 现有方案时通过GetRenderedValues()+Margin来得出近乎正确的值
            //var prefervalue = tmp.GetPreferredValues();
            var renderervalue = tmp.GetRenderedValues();
            renderervalue.x += tmp.margin.x + tmp.margin.z;
            renderervalue.y += tmp.margin.y + tmp.margin.w;
            realsize.x = rect.rect.width >= renderervalue.x ? rect.rect.width : renderervalue.x;
            realsize.y = rect.rect.height >= renderervalue.y ? rect.rect.height : renderervalue.y;
            return realsize;
        }
        else
        {
            realsize.x = rect.rect.width;
            realsize.x = rect.rect.height;
            return realsize;
        }      
    }
}