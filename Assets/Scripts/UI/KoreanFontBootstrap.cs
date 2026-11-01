using TMPro;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

/// <summary>Loads Malgun Gothic from Resources and applies a dynamic TMP font to every UI text.</summary>
public class KoreanFontBootstrap : MonoBehaviour
{
    private static TMP_FontAsset koreanFont;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeBeforeSceneLoad()
    {
        ConfigureGlobalKoreanFont();
    }

    private void Awake()
    {
        ConfigureGlobalKoreanFont();
    }

    private static void ConfigureGlobalKoreanFont()
    {
        if (koreanFont != null) return;

        Font sourceFont = Resources.Load<Font>("Fonts/MalgunGothic");
        if (sourceFont == null)
        {
            Debug.LogError("Korean source font was not found at Resources/Fonts/MalgunGothic.");
            return;
        }

        // Create the dynamic atlas at runtime. The editor-generated asset is not
        // used because it can be saved without its atlas texture reference.
        koreanFont = TMP_FontAsset.CreateFontAsset(
            sourceFont,
            90,
            9,
            GlyphRenderMode.SDFAA,
            1024,
            1024,
            AtlasPopulationMode.Dynamic,
            true);

        // Register this as TMP's global fallback as well as assigning it to
        // visible labels. This lets any newly entered Korean text add glyphs
        // dynamically instead of falling back to Liberation Sans squares.
        koreanFont.atlasPopulationMode = AtlasPopulationMode.Dynamic;
        koreanFont.isMultiAtlasTexturesEnabled = true;
        TMP_Settings.fallbackFontAssets.RemoveAll(font =>
            font == null || font.atlasTextures == null || font.atlasTextures.Length == 0 || font.atlasTextures[0] == null);
        if (!TMP_Settings.fallbackFontAssets.Contains(koreanFont))
        {
            TMP_Settings.fallbackFontAssets.Add(koreanFont);
        }
        TMP_Settings.defaultFontAsset = koreanFont;

        koreanFont.TryAddCharacters(
            "가나다라마바사아자차카타파하 정신력 잠자기 다음날 답장하기 무시하기 " +
            "식사 약복용 휴대폰 창문 돌아가기 0123456789 / :",
            out _);

        Debug.Log("Korean TMP font loaded: Malgun Gothic");
    }

    private void Update()
    {
        ConfigureGlobalKoreanFont();
        if (koreanFont == null) return;

        foreach (TMP_Text text in FindObjectsByType<TMP_Text>(FindObjectsSortMode.None))
        {
            if (!string.IsNullOrEmpty(text.text))
            {
                koreanFont.TryAddCharacters(text.text, out _);
            }

            // Scene-authored TMP labels may keep their former Liberation Sans
            // material after their font changes. Reset both references together.
            text.font = koreanFont;
            text.fontSharedMaterial = koreanFont.material;
            text.SetVerticesDirty();
        }
    }
}
