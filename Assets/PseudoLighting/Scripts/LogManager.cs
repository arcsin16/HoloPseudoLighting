using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(TextMesh))]
public class LogManager : MonoBehaviour {

    private TextMesh debugText;

    private void Start()
    {
        this.debugText = this.gameObject.GetComponent<TextMesh>();
    }

    void OnEnable()
    {
        // 以下のイベントハンドラーに登録することで、ログ出力時に呼び出されるようになる。
        Application.logMessageReceived += LogMessage;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= LogMessage;
    }

    private void LogMessage(string condition, string stackTrace, LogType type)
    {
        // このメソッド内でのDebug.Logxxx系の呼び出しは無限ループになるので禁止

        if (this.debugText == null)
        {
            return;
        }

        var newText = this.debugText.text + condition + "\n";
        this.debugText.text = Tail(newText, 10);
    }

    /// <summary>
    /// 文字列の後ろからn行分を切り出す
    /// </summary>
    /// <param name="text">文字列</param>
    /// <param name="lines">切り出す行数</param>
    /// <returns></returns>
    private string Tail(string text, int lines)
    {
        int index = text.Length;
        while(lines >= 0)
        {
            index = text.LastIndexOf("\n", index-1);
            if(index == -1)
            {
                return text;
            }
            lines--;
        }

        return text.Substring(index);
    }
}
